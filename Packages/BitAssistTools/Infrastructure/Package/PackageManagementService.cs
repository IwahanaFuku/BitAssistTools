using UnityEngine;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using System.IO;
using Newtonsoft.Json.Linq;

namespace BitAssistTools.Infrastructure.Package
{
    /// <summary>
    /// パッケージバージョン管理の初期化サービス
    /// システム全体のライフサイクル管理を担当
    /// </summary>
    [InitializeOnLoad]
    public static class PackageManagementService
    {
        static PackageManagementService()
        {
            EditorApplication.delayCall += CheckVersionOnStartup;
        }

        private static void CheckVersionOnStartup()
        {
            if (EditorApplication.isPlaying) return;

            var settingsService = SettingsServiceLocator.GetService();
            if (settingsService != null && !settingsService.IsAutoUpdateNotificationEnabled())
            {
                return;
            }

            var checker = new PackageVersionChecker();
            if (checker.HasNewerVersion())
            {
                OnUpdateAvailable?.Invoke();
            }
        }

        public static System.Action OnUpdateAvailable;
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