using System;
using BitAssistTools.Domain.Models.Settings;
using BitAssistTools.Domain.Services;
using BitAssistTools.Infrastructure.Unity;

namespace BitAssistTools.Application.Services
{
    /// <summary>
    /// アプリケーション設定管理サービス
    /// </summary>
    public class SettingsService : ISettingsService
    {
        private static SettingsService _instance;
        private readonly SettingsRepository _repository = new SettingsRepository();
        private AppSettings? _cachedSettings;

        public static SettingsService Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new SettingsService();
                }
                return _instance;
            }
        }

        public event Action<AppSettings> OnSettingsChanged;

        public AppSettings GetSettings()
        {
            if (!_cachedSettings.HasValue)
            {
                _cachedSettings = _repository.Load();
            }
            return _cachedSettings.Value;
        }

        public void UpdateSettings(AppSettings settings)
        {
            try
            {
                _repository.Save(settings);
                _cachedSettings = settings;
                OnSettingsChanged?.Invoke(settings);
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"Failed to update settings: {ex.Message}");
                throw;
            }
        }

        public void SetAutoUpdateNotification(bool enabled)
        {
            var currentSettings = GetSettings();
            var newSettings = currentSettings.WithAutoUpdateNotification(enabled);
            UpdateSettings(newSettings);
        }

        public bool IsAutoUpdateNotificationEnabled()
        {
            return GetSettings().IsAutoUpdateNotificationEnabled;
        }
    }
}