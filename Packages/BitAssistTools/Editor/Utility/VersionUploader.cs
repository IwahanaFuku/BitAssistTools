using UnityEngine;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using System.IO;
using Newtonsoft.Json.Linq;

namespace VersionUploader
{
    [InitializeOnLoad]
    public class VersionCheckWindow : EditorWindow
    {
        static PackageManagerUpdater pmu;
        static void Open()
        {
            if (EditorApplication.isPlaying)
            {
                return;
            }

            pmu = new PackageManagerUpdater();
            bool shouldShowWindow = pmu.CompareVersions();

            if(shouldShowWindow)
            {
                ShowWindow();
            }
        }
        
        //Initial Open
        [InitializeOnLoadMethod]
        static void OpenOnStart()
        {
            EditorApplication.delayCall += Open;
        }

        [MenuItem("Bit Assist Tools/バージョンアップデート",priority = 200)]
        private static void ShowWindow()
        {
            VersionCheckWindow window = GetWindow<VersionCheckWindow>("Bit Assist Tools Update");
            Vector2 windowSize = new Vector2(350, 115);
            window.minSize = windowSize;
            window.maxSize = windowSize;
            window.Show();
        }

        const string RELEASENOTE_URL = "https://scrapbox.io/BitAssistTools/%E3%83%AA%E3%83%AA%E3%83%BC%E3%82%B9%E3%83%8E%E3%83%BC%E3%83%88";

        void OnGUI()
        {
            GUIStyle hyperlinkStyle = new GUIStyle(GUI.skin.label);
            hyperlinkStyle.normal.textColor = new Color(32f / 255f, 157f / 255f, 240f / 255f); // #209df0の色
            hyperlinkStyle.stretchWidth = true;
            hyperlinkStyle.wordWrap = true;

            GUILayout.Space(10);

            if (pmu.CompareVersions())
            {
                GUILayout.Label($"{pmu.editorpackageinfo.showName} の最新バージョン【{pmu.editorpackageinfo.latestVersion}】がリリースされています。 \nアップデートしますか？");
                GUILayout.Label($"現在のバージョン：{pmu.editorpackageinfo.currentVersion} ");
                GUILayout.Label("リリースノートを確認", hyperlinkStyle);
                if (Event.current.type == EventType.MouseDown && GUILayoutUtility.GetLastRect().Contains(Event.current.mousePosition))
                {
                    Application.OpenURL(RELEASENOTE_URL);
                }

                GUILayout.FlexibleSpace();
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("いいえ"))
                {
                    CloseWindow();
                }

                if (GUILayout.Button("はい"))
                {
                    pmu.UpdateCurrentPackageVersion();
                    CloseWindow();
                }
                GUILayout.EndHorizontal();
            }
            else
            {
                GUILayout.Label($"ご使用の{pmu.editorpackageinfo.showName} は最新です");
                GUILayout.Label($"現在のバージョン：{pmu.editorpackageinfo.currentVersion} ");
                GUILayout.Label("リリースノートを確認", hyperlinkStyle);
                if (Event.current.type == EventType.MouseDown && GUILayoutUtility.GetLastRect().Contains(Event.current.mousePosition))
                {
                    Application.OpenURL(RELEASENOTE_URL);
                }
                
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("閉じる"))
                {
                    CloseWindow();
                }
            }
        }

        private void CloseWindow()
        {
            Close();
        }
    }
}


namespace VersionUploader
{
    public class PackageManagerUpdater
    {
        public EditorPackageInfo editorpackageinfo = new EditorPackageInfo
        {
            name = "com.iwahana.bitassisttools",
            showName = "Bit Assist Tools"
        };

        public PackageManagerUpdater()
        {
            editorpackageinfo.currentVersion = GetCurrentPackageVersion();
            editorpackageinfo.latestVersion = GetLatestPackageVersion();
        }

        public bool IsNewerVersionsExist()
        {
            bool result = false;

            if (!string.IsNullOrEmpty(editorpackageinfo.currentVersion))
            {
                if (!string.IsNullOrEmpty(editorpackageinfo.latestVersion))
                {
                    result = CompareVersions();
                }
                else
                {
                    Debug.LogError("Failed to fetch the latest version of the package.");
                }
            }

            return result;
        }

        public string GetCurrentPackageVersion()
        {
            string manifestPath = Path.GetFullPath("Packages/manifest.json");
            string currentVersion = null;

            if (File.Exists(manifestPath))
            {
                string jsonContent = File.ReadAllText(manifestPath);
                JObject jsonObject = JObject.Parse(jsonContent);
                JObject dependencies = jsonObject["dependencies"] as JObject;

                if (dependencies != null && dependencies[editorpackageinfo.name] != null)
                {
                    currentVersion = dependencies[editorpackageinfo.name].ToString();
                }
                else
                {
                    Debug.LogError($"Version information for '{editorpackageinfo.name}' not found.");
                }
            }
            else
            {
                Debug.LogError($"File not found at path: {manifestPath}");
            }

            return currentVersion;
        }

        public string GetLatestPackageVersion()
        {
            ListRequest listRequest = Client.List(true);
            while (!listRequest.IsCompleted)
            {
                // Wait for the request to complete
            }

            if (listRequest.Status == StatusCode.Success)
            {
                foreach (var package in listRequest.Result)
                {
                    if (package.name == editorpackageinfo.name)
                    {
                        return package.versions.latestCompatible;
                    }
                }
            }
            else
            {
                Debug.LogError($"Failed to fetch package information: {listRequest.Error.message}");
            }

            return null;
        }

        // セマンティックバージョンの代償を比較して、新しいバージョンがあればtrueを返すメソッド。
        public bool CompareVersions()
        {
            bool result = false;
            int comparisonResult = CompareSemanticVersions();

            if (comparisonResult < 0)
            {
                result = true;
            }
            return result;
        }

        // セマンティックバージョンを比較するメソッド
        int CompareSemanticVersions()
        {
            string[] partsA = editorpackageinfo.currentVersion.Split('.');
            string[] partsB = editorpackageinfo.latestVersion.Split('.');

            for (int i = 0; i < 3; i++)
            {
                int partA = i < partsA.Length ? int.Parse(partsA[i]) : 0;
                int partB = i < partsB.Length ? int.Parse(partsB[i]) : 0;

                if (partA < partB)
                {
                    return -1;
                }
                else if (partA > partB)
                {
                    return 1;
                }
            }

            return 0;
        }

        public void UpdateCurrentPackageVersion()
        {
            Client.Add(editorpackageinfo.name + "@" + editorpackageinfo.latestVersion);
        }
    }

    public class EditorPackageInfo
    {
        public string name;
        public string showName;
        public string currentVersion;
        public string latestVersion;
    }
}