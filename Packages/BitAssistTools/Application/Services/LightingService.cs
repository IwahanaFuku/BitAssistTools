using System;
using System.Linq;
using UnityEngine;
using UnityEditor;
using BitAssistTools.Domain.Models.Lighting;

namespace BitAssistTools.Application.Services
{
    /// <summary>
    /// ライティング操作のアプリケーションサービス
    /// DDDベースの純粋な実装
    /// </summary>
    public static class LightingService
    {
        /// <summary>
        /// 選択されたGameObjectのLight Intensityを変更（DDDベース、Undo対応）
        /// </summary>
        public static void ApplyLightIntensityOperation(LightOperationRule rule)
        {
            try
            {
                if (!rule.IsValid())
                {
                    Debug.LogError($"無効なライト操作ルールです: {rule}");
                    return;
                }

                Undo.SetCurrentGroupName("Light Intensity Operation");
                int undoGroup = Undo.GetCurrentGroup();

                var selectedObjects = UnityEditor.Selection.gameObjects;
                if (selectedObjects == null || selectedObjects.Length == 0)
                {
                    Debug.LogError("ライトコンポーネントを持つGameObjectを選択してください。");
                    return;
                }

                // Lightコンポーネントを持つオブジェクトを取得
                var lightsToModify = selectedObjects
                    .Select(obj => obj.GetComponent<Light>())
                    .Where(light => light != null)
                    .ToArray();

                if (lightsToModify.Length == 0)
                {
                    Debug.LogWarning("選択されたオブジェクトにLightコンポーネントが見つかりません。");
                    return;
                }

                // 各Lightコンポーネントの変更をUndo対応で実行
                foreach (var light in lightsToModify)
                {
                    Undo.RecordObject(light, "Light Intensity Change");
                    
                    float newIntensity = rule.ApplyTo(light.intensity);
                    
                    // 負数にならないように制限
                    newIntensity = Mathf.Max(0.0f, newIntensity);
                    
                    light.intensity = newIntensity;
                }

                Undo.CollapseUndoOperations(undoGroup);
                
            }
            catch (Exception ex)
            {
                Debug.LogError($"ライト強度変更中にエラーが発生しました: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// 現在の選択に対してライト操作が実行可能かどうかを判定
        /// </summary>
        public static bool CanApplyLightOperation()
        {
            var selectedObjects = UnityEditor.Selection.gameObjects;
            if (selectedObjects == null || selectedObjects.Length == 0)
                return false;

            return selectedObjects.Any(obj => obj.GetComponent<Light>() != null);
        }

        /// <summary>
        /// 選択されたオブジェクトのライト数を取得
        /// </summary>
        public static int GetSelectedLightCount()
        {
            var selectedObjects = UnityEditor.Selection.gameObjects;
            if (selectedObjects == null || selectedObjects.Length == 0)
                return 0;

            return selectedObjects.Count(obj => obj.GetComponent<Light>() != null);
        }

        /// <summary>
        /// 選択されたライトの現在の強度値の統計を取得
        /// </summary>
        public static LightIntensityStats GetLightIntensityStats()
        {
            var selectedObjects = UnityEditor.Selection.gameObjects;
            if (selectedObjects == null || selectedObjects.Length == 0)
                return new LightIntensityStats(0, 0.0f, 0.0f, 0.0f);

            var intensities = selectedObjects
                .Select(obj => obj.GetComponent<Light>())
                .Where(light => light != null)
                .Select(light => light.intensity)
                .ToArray();

            if (intensities.Length == 0)
                return new LightIntensityStats(0, 0.0f, 0.0f, 0.0f);

            return new LightIntensityStats(
                intensities.Length,
                intensities.Min(),
                intensities.Max(),
                intensities.Average()
            );
        }

        /// <summary>
        /// ライト強度の統計情報
        /// </summary>
        public readonly struct LightIntensityStats
        {
            public readonly int Count;
            public readonly float MinIntensity;
            public readonly float MaxIntensity;
            public readonly float AverageIntensity;

            public LightIntensityStats(int count, float min, float max, float average)
            {
                Count = count;
                MinIntensity = min;
                MaxIntensity = max;
                AverageIntensity = average;
            }

            public bool HasLights => Count > 0;

            public override string ToString()
            {
                if (!HasLights) return "ライトなし";
                return $"{Count}個のライト (強度: {MinIntensity:F2}～{MaxIntensity:F2}, 平均: {AverageIntensity:F2})";
            }
        }
    }
}