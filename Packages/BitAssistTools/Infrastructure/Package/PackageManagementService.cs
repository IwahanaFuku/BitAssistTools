using UnityEngine;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using System.IO;
using Newtonsoft.Json.Linq;

namespace BitAssistTools.Infrastructure.Package
{
    /// <summary>
    /// パッケージバージョン管理の統合サービス
    /// システム全体のライフサイクル管理を担当
    /// </summary>
    [InitializeOnLoad]
    public class PackageManagementService : EditorWindow
    {
        private static PackageVersionChecker _checker;
        private const string RELEASE_NOTE_URL = "https://scrapbox.io/BitAssistTools/%E3%83%AA%E3%83%AA%E3%83%BC%E3%82%B9%E3%83%8E%E3%83%BC%E3%83%88";

        static PackageManagementService()
        {
            EditorApplication.delayCall += CheckVersionOnStartup;
        }

        [MenuItem("Bit Assist Tools/バージョンアップデート", priority = 200)]
        private static void ShowWindow()
        {
            var window = GetWindow<PackageManagementService>("Bit Assist Tools Update");
            Vector2 windowSize = new Vector2(350, 115);
            window.minSize = windowSize;
            window.maxSize = windowSize;
            window.Show();
        }

        private static void CheckVersionOnStartup()
        {
            if (EditorApplication.isPlaying) return;

            _checker = new PackageVersionChecker();
            if (_checker.HasNewerVersion())
            {
                ShowWindow();
            }
        }

        private void OnGUI()
        {
            if (_checker == null)
            {
                _checker = new PackageVersionChecker();
            }

            var hyperlinkStyle = new GUIStyle(GUI.skin.label)
            {
                normal = { textColor = new Color(32f / 255f, 157f / 255f, 240f / 255f) },
                stretchWidth = true,
                wordWrap = true
            };

            GUILayout.Space(10);

            if (_checker.HasNewerVersion())
            {
                DrawUpdateAvailableUI(hyperlinkStyle);
            }
            else
            {
                DrawUpToDateUI(hyperlinkStyle);
            }
        }

        private void DrawUpdateAvailableUI(GUIStyle hyperlinkStyle)
        {
            var info = _checker.GetPackageInfo();
            GUILayout.Label($"{info.DisplayName} の最新バージョン【{info.LatestVersion}】がリリースされています。 \nアップデートしますか？");
            GUILayout.Label($"現在のバージョン：{info.CurrentVersion} ");
            
            DrawReleaseNoteLink(hyperlinkStyle);

            GUILayout.FlexibleSpace();
            GUILayout.BeginHorizontal();
            
            if (GUILayout.Button("いいえ"))
            {
                Close();
            }

            if (GUILayout.Button("はい"))
            {
                _checker.UpdatePackage();
                Close();
            }
            
            GUILayout.EndHorizontal();
        }

        private void DrawUpToDateUI(GUIStyle hyperlinkStyle)
        {
            var info = _checker.GetPackageInfo();
            GUILayout.Label($"ご使用の{info.DisplayName} は最新です");
            GUILayout.Label($"現在のバージョン：{info.CurrentVersion} ");
            
            DrawReleaseNoteLink(hyperlinkStyle);
            
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("閉じる"))
            {
                Close();
            }
        }

        private void DrawReleaseNoteLink(GUIStyle hyperlinkStyle)
        {
            GUILayout.Label("リリースノートを確認", hyperlinkStyle);
            if (Event.current.type == EventType.MouseDown && 
                GUILayoutUtility.GetLastRect().Contains(Event.current.mousePosition))
            {
                Application.OpenURL(RELEASE_NOTE_URL);
            }
        }
    }

    /// <summary>
    /// パッケージバージョン情報
    /// </summary>
    public readonly struct PackageInfo
    {
        public readonly string PackageName;
        public readonly string DisplayName;
        public readonly string CurrentVersion;
        public readonly string LatestVersion;

        public PackageInfo(string packageName, string displayName, string currentVersion, string latestVersion)
        {
            PackageName = packageName;
            DisplayName = displayName;
            CurrentVersion = currentVersion;
            LatestVersion = latestVersion;
        }
    }

    /// <summary>
    /// パッケージバージョンチェッカー
    /// </summary>
    public class PackageVersionChecker
    {
        private const string PACKAGE_NAME = "com.iwahana.bitassisttools";
        private const string DISPLAY_NAME = "Bit Assist Tools";
        
        private readonly PackageInfo _packageInfo;

        public PackageVersionChecker()
        {
            _packageInfo = new PackageInfo(
                PACKAGE_NAME,
                DISPLAY_NAME,
                GetCurrentVersion(),
                GetLatestVersion()
            );
        }

        public bool HasNewerVersion()
        {
            if (string.IsNullOrEmpty(_packageInfo.CurrentVersion) || 
                string.IsNullOrEmpty(_packageInfo.LatestVersion))
            {
                return false;
            }

            return CompareSemanticVersions(_packageInfo.CurrentVersion, _packageInfo.LatestVersion) < 0;
        }

        public PackageInfo GetPackageInfo()
        {
            return _packageInfo;
        }

        public void UpdatePackage()
        {
            Client.Add($"{PACKAGE_NAME}@{_packageInfo.LatestVersion}");
        }

        private string GetCurrentVersion()
        {
            string manifestPath = Path.GetFullPath("Packages/manifest.json");
            
            if (!File.Exists(manifestPath))
            {
                Debug.LogError($"Manifest file not found: {manifestPath}");
                return null;
            }

            try
            {
                string jsonContent = File.ReadAllText(manifestPath);
                var jsonObject = JObject.Parse(jsonContent);
                var dependencies = jsonObject["dependencies"] as JObject;

                return dependencies?[PACKAGE_NAME]?.ToString();
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Failed to parse manifest.json: {ex.Message}");
                return null;
            }
        }

        private string GetLatestVersion()
        {
            var listRequest = Client.List(true);
            
            while (!listRequest.IsCompleted)
            {
                // Wait for completion
            }

            if (listRequest.Status == StatusCode.Success)
            {
                foreach (var package in listRequest.Result)
                {
                    if (package.name == PACKAGE_NAME)
                    {
                        return package.versions.latestCompatible;
                    }
                }
            }
            else
            {
                Debug.LogError($"Failed to fetch package information: {listRequest.Error?.message}");
            }

            return null;
        }

        private int CompareSemanticVersions(string versionA, string versionB)
        {
            var partsA = versionA.Split('.');
            var partsB = versionB.Split('.');

            for (int i = 0; i < 3; i++)
            {
                int partA = i < partsA.Length ? int.Parse(partsA[i]) : 0;
                int partB = i < partsB.Length ? int.Parse(partsB[i]) : 0;

                if (partA < partB) return -1;
                if (partA > partB) return 1;
            }

            return 0;
        }
    }
}