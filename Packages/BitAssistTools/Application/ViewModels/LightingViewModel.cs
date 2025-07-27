using System;
using UnityEngine;
using BitAssistTools.Domain.Models.Lighting;
using BitAssistTools.Application.Services;

namespace BitAssistTools.Application.ViewModels
{
    /// <summary>
    /// Lighting機能のViewModel
    /// UI状態とビジネスロジックを分離
    /// </summary>
    public class LightingViewModel
    {
        // UI展開状態管理
        public class UIExpansionState
        {
            public bool IntensityCalculation { get; set; } = true;
        }

        // ライト操作設定
        public class LightOperationSettings
        {
            public float IntensityValue { get; set; } = 1.0f;
            public ArithmeticOperation Operation { get; set; } = ArithmeticOperation.Addition;
        }

        // イベント定義
        public event Action OnUIStateChanged;
        public event Action<string> OnError;
        public event Action<string> OnSuccess;

        // プロパティ
        public UIExpansionState UI { get; private set; }
        public LightOperationSettings LightOperation { get; private set; }

        public LightingViewModel()
        {
            UI = new UIExpansionState();
            LightOperation = new LightOperationSettings();
        }

        /// <summary>
        /// ライト強度操作を実行
        /// </summary>
        public void ExecuteLightIntensityOperation()
        {
            try
            {
                var rule = new LightOperationRule(
                    LightOperation.IntensityValue,
                    LightOperation.Operation
                );

                if (!rule.IsValid())
                {
                    NotifyError("無効な操作値です。");
                    return;
                }

                if (!LightingService.CanApplyLightOperation())
                {
                    NotifyError("Lightコンポーネントを持つGameObjectを選択してください。");
                    return;
                }

                LightingService.ApplyLightIntensityOperation(rule);
                
                var stats = LightingService.GetLightIntensityStats();
                NotifySuccess($"ライト強度を変更しました: {stats}");
            }
            catch (Exception ex)
            {
                NotifyError($"ライト強度変更中にエラーが発生しました: {ex.Message}");
            }
        }

        /// <summary>
        /// ライト操作が実行可能かどうかを判定
        /// </summary>
        public bool CanExecuteLightOperation()
        {
            try
            {
                var rule = new LightOperationRule(
                    LightOperation.IntensityValue,
                    LightOperation.Operation
                );

                return rule.IsValid() && LightingService.CanApplyLightOperation();
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 現在の選択状況の統計を取得
        /// </summary>
        public LightingService.LightIntensityStats GetCurrentStats()
        {
            return LightingService.GetLightIntensityStats();
        }

        /// <summary>
        /// 操作値を設定
        /// </summary>
        public void UpdateIntensityValue(float value)
        {
            LightOperation.IntensityValue = value;
            NotifyUIStateChanged();
        }

        /// <summary>
        /// 操作種類を設定
        /// </summary>
        public void UpdateOperation(ArithmeticOperation operation)
        {
            LightOperation.Operation = operation;
            NotifyUIStateChanged();
        }

        /// <summary>
        /// 操作の文字列表現を取得
        /// </summary>
        public string GetOperationText()
        {
            return LightOperation.Operation == ArithmeticOperation.Addition ? "加算" : "乗算";
        }

        /// <summary>
        /// 操作のプレビューテキストを取得
        /// </summary>
        public string GetOperationPreview()
        {
            string symbol = LightOperation.Operation == ArithmeticOperation.Addition ? "+" : "×";
            return $"{symbol}{LightOperation.IntensityValue}";
        }

        /// <summary>
        /// UI状態変更通知
        /// </summary>
        public void NotifyUIStateChanged()
        {
            OnUIStateChanged?.Invoke();
        }

        /// <summary>
        /// エラー通知
        /// </summary>
        private void NotifyError(string message)
        {
            OnError?.Invoke(message);
        }

        /// <summary>
        /// 成功通知
        /// </summary>
        private void NotifySuccess(string message)
        {
            OnSuccess?.Invoke(message);
        }

        /// <summary>
        /// リソース解放
        /// </summary>
        public void Dispose()
        {
            // 現在は特別なクリーンアップは不要
        }
    }
}