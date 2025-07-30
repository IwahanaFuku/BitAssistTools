namespace BitAssistTools.Domain.Services
{
    /// <summary>
    /// 設定サービスのインターフェース
    /// </summary>
    public interface ISettingsService
    {
        bool IsAutoUpdateNotificationEnabled();
    }
}