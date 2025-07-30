using BitAssistTools.Domain.Services;

namespace BitAssistTools.Infrastructure.Package
{
    /// <summary>
    /// 設定サービスのサービスロケーター
    /// Infrastructure層からDomain層のインターフェースを介してApplication層にアクセス
    /// </summary>
    public static class SettingsServiceLocator
    {
        private static ISettingsService _instance;

        public static void RegisterService(ISettingsService service)
        {
            _instance = service;
        }

        public static ISettingsService GetService()
        {
            return _instance;
        }
    }
}