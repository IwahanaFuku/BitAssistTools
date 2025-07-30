using UnityEditor;
using BitAssistTools.Application.Services;
using BitAssistTools.Infrastructure.Package;
using BitAssistTools.Editor.View;

namespace BitAssistTools.Editor
{
    /// <summary>
    /// エディター起動時のサービス初期化（DI Container代替）
    /// Editor層でApplication層の具象クラスをInfrastructure層に注入
    /// </summary>
    [InitializeOnLoad]
    public static class ServiceBootstrapper
    {
        static ServiceBootstrapper()
        {
            InitializeServices();
            SubscribeToInfrastructureEvents();
        }

        private static void InitializeServices()
        {
            SettingsServiceLocator.RegisterService(BitAssistTools.Application.Services.SettingsService.Instance);
        }

        private static void SubscribeToInfrastructureEvents()
        {
            PackageManagementService.OnUpdateAvailable += PackageManagementView.ShowWindowIfUpdateAvailable;
        }
    }
}