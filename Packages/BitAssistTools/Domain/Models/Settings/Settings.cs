using System;

namespace BitAssistTools.Domain.Models.Settings
{
    /// <summary>
    /// アプリケーション設定を表すドメインモデル
    /// </summary>
    public readonly struct AppSettings : IEquatable<AppSettings>
    {
        public readonly bool IsAutoUpdateNotificationEnabled;

        public AppSettings(bool isAutoUpdateNotificationEnabled)
        {
            IsAutoUpdateNotificationEnabled = isAutoUpdateNotificationEnabled;
        }

        public static AppSettings Default => new AppSettings(true);

        public AppSettings WithAutoUpdateNotification(bool enabled)
        {
            return new AppSettings(enabled);
        }

        public bool Equals(AppSettings other)
        {
            return IsAutoUpdateNotificationEnabled == other.IsAutoUpdateNotificationEnabled;
        }

        public override bool Equals(object obj)
        {
            return obj is AppSettings other && Equals(other);
        }

        public override int GetHashCode()
        {
            return IsAutoUpdateNotificationEnabled.GetHashCode();
        }

        public static bool operator ==(AppSettings left, AppSettings right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(AppSettings left, AppSettings right)
        {
            return !left.Equals(right);
        }
    }
}