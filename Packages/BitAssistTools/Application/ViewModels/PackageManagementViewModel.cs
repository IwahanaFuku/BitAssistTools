using System;
using BitAssistTools.Application.Services;
using BitAssistTools.Infrastructure.Package;

namespace BitAssistTools.Application.ViewModels
{
    /// <summary>
    /// パッケージ管理ウィンドウのViewModel
    /// </summary>
    public class PackageManagementViewModel
    {
        private readonly PackageVersionChecker _checker;
        private readonly SettingsService _settingsService;

        public event Action OnUIStateChanged;
        public event Action<string> OnError;

        public string DisplayName { get; }
        public string CurrentVersion { get; }
        public string LatestVersion { get; }
        public bool HasNewerVersion { get; }

        public bool IsAutoUpdateNotificationEnabled
        {
            get => _settingsService.IsAutoUpdateNotificationEnabled();
            set
            {
                try
                {
                    _settingsService.SetAutoUpdateNotification(value);
                    OnUIStateChanged?.Invoke();
                }
                catch (Exception ex)
                {
                    OnError?.Invoke($"設定の保存に失敗しました: {ex.Message}");
                }
            }
        }

        public PackageManagementViewModel()
        {
            _checker = new PackageVersionChecker();
            _settingsService = SettingsService.Instance;
            
            var packageInfo = _checker.GetPackageInfo();
            DisplayName = packageInfo.DisplayName;
            CurrentVersion = packageInfo.CurrentVersion;
            LatestVersion = packageInfo.LatestVersion;
            HasNewerVersion = _checker.HasNewerVersion();

            _settingsService.OnSettingsChanged += OnSettingsChanged;
        }

        public void UpdatePackage()
        {
            try
            {
                _checker.UpdatePackage();
            }
            catch (Exception ex)
            {
                OnError?.Invoke($"アップデートに失敗しました: {ex.Message}");
            }
        }

        private void OnSettingsChanged(BitAssistTools.Domain.Models.Settings.AppSettings settings)
        {
            OnUIStateChanged?.Invoke();
        }

        public void Dispose()
        {
            _settingsService.OnSettingsChanged -= OnSettingsChanged;
        }
    }
}