using UnityEngine;
using UnityEditor;
using BitAssistTools.Domain.Models.Settings;
using Newtonsoft.Json;

namespace BitAssistTools.Infrastructure.Unity
{
    /// <summary>
    /// EditorPrefsを使用したアプリケーション設定の永続化
    /// </summary>
    public class SettingsRepository
    {
        private const string SETTINGS_KEY = "BitAssistTools.AppSettings";

        public AppSettings Load()
        {
            try
            {
                string jsonData = EditorPrefs.GetString(SETTINGS_KEY, string.Empty);
                
                if (string.IsNullOrEmpty(jsonData))
                {
                    return AppSettings.Default;
                }

                var settingsData = JsonConvert.DeserializeObject<SettingsData>(jsonData);
                return new AppSettings(settingsData.IsAutoUpdateNotificationEnabled);
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Failed to load settings: {ex.Message}");
                return AppSettings.Default;
            }
        }

        public void Save(AppSettings settings)
        {
            try
            {
                var settingsData = new SettingsData
                {
                    IsAutoUpdateNotificationEnabled = settings.IsAutoUpdateNotificationEnabled
                };
                
                string jsonData = JsonConvert.SerializeObject(settingsData, Formatting.Indented);
                EditorPrefs.SetString(SETTINGS_KEY, jsonData);
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Failed to save settings: {ex.Message}");
            }
        }

        [System.Serializable]
        private class SettingsData
        {
            public bool IsAutoUpdateNotificationEnabled;
        }
    }
}