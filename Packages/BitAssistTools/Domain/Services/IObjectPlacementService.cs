using System.Collections.Generic;
using BitAssistTools.Domain.Models.Transform;
using BitAssistTools.Domain.Models.Placement;
using UnityEngine;

namespace BitAssistTools.Domain.Services
{
    /// <summary>
    /// オブジェクト配置操作を担当するドメインサービス
    /// </summary>
    public interface IObjectPlacementService
    {
        /// <summary>
        /// 複製戦略に基づいてオブジェクトを複製
        /// </summary>
        /// <param name="sourceObjects">複製元オブジェクト配列</param>
        /// <param name="strategy">複製戦略</param>
        /// <param name="referenceTransform">参照Transform（ローカル座標の場合）</param>
        /// <returns>複製されたGameObject配列</returns>
        GameObject[] DuplicateObjects(
            GameObject[] sourceObjects, 
            DuplicationStrategy strategy,
            UnityEngine.Transform referenceTransform = null);

        /// <summary>
        /// 配置ルールに基づいてオブジェクトを整列配置
        /// </summary>
        /// <param name="targetObjects">整列対象のGameObject配列</param>
        /// <param name="rule">配置ルール</param>
        /// <returns>整列実行結果</returns>
        PlacementResult AlignObjects(GameObject[] targetObjects, PlacementRule rule);

        /// <summary>
        /// Prefabかどうかを判定し、適切な複製方法を選択
        /// </summary>
        /// <param name="sourceObject">複製元オブジェクト</param>
        /// <returns>複製されたGameObject</returns>
        GameObject CreateOptimalCopy(GameObject sourceObject);

        /// <summary>
        /// 階層構造を保持した複製を実行
        /// </summary>
        /// <param name="sourceObject">複製元オブジェクト</param>
        /// <param name="preserveHierarchy">階層構造を保持するか</param>
        /// <returns>複製されたGameObject</returns>
        GameObject DuplicateWithHierarchy(GameObject sourceObject, bool preserveHierarchy);

        /// <summary>
        /// 指定した親の下に子オブジェクトを配置
        /// </summary>
        /// <param name="childObjects">子オブジェクト配列</param>
        /// <param name="parentTransform">親Transform</param>
        /// <param name="maintainWorldPosition">ワールド座標を維持するか</param>
        void SetParent(GameObject[] childObjects, UnityEngine.Transform parentTransform, bool maintainWorldPosition = true);

        /// <summary>
        /// オブジェクトの配置可能性を検証
        /// </summary>
        /// <param name="targetObjects">検証対象オブジェクト</param>
        /// <param name="strategy">複製戦略</param>
        /// <returns>検証結果</returns>
        PlacementValidationResult ValidatePlacement(GameObject[] targetObjects, DuplicationStrategy strategy);
    }

    /// <summary>
    /// 配置操作の結果を表現する値オブジェクト
    /// </summary>
    public readonly struct PlacementResult
    {
        public readonly bool Success;
        public readonly int ProcessedCount;
        public readonly string ErrorMessage;
        public readonly System.TimeSpan ExecutionTime;

        public PlacementResult(bool success, int processedCount, string errorMessage = null, System.TimeSpan executionTime = default)
        {
            Success = success;
            ProcessedCount = processedCount;
            ErrorMessage = errorMessage;
            ExecutionTime = executionTime;
        }

        public static PlacementResult CreateSuccess(int processedCount, System.TimeSpan executionTime = default)
        {
            return new PlacementResult(true, processedCount, null, executionTime);
        }

        public static PlacementResult CreateFailure(string errorMessage, int processedCount = 0)
        {
            return new PlacementResult(false, processedCount, errorMessage);
        }

        public override string ToString()
        {
            return Success 
                ? $"PlacementResult(Success: {ProcessedCount} objects, Time: {ExecutionTime.TotalMilliseconds:F2}ms)"
                : $"PlacementResult(Failed: {ErrorMessage}, Processed: {ProcessedCount})";
        }
    }

    /// <summary>
    /// 配置検証の結果を表現する値オブジェクト
    /// </summary>
    public readonly struct PlacementValidationResult
    {
        public readonly bool IsValid;
        public readonly IReadOnlyList<string> ValidationErrors;
        public readonly IReadOnlyList<string> Warnings;

        public PlacementValidationResult(bool isValid, IReadOnlyList<string> validationErrors, IReadOnlyList<string> warnings)
        {
            IsValid = isValid;
            ValidationErrors = validationErrors ?? new List<string>().AsReadOnly();
            Warnings = warnings ?? new List<string>().AsReadOnly();
        }

        public static PlacementValidationResult CreateValid(IReadOnlyList<string> warnings = null)
        {
            return new PlacementValidationResult(true, null, warnings);
        }

        public static PlacementValidationResult CreateInvalid(IReadOnlyList<string> errors, IReadOnlyList<string> warnings = null)
        {
            return new PlacementValidationResult(false, errors, warnings);
        }

        public override string ToString()
        {
            if (IsValid)
            {
                return Warnings?.Count > 0 
                    ? $"PlacementValidationResult(Valid with {Warnings.Count} warnings)"
                    : "PlacementValidationResult(Valid)";
            }
            else
            {
                return $"PlacementValidationResult(Invalid: {ValidationErrors.Count} errors, {Warnings?.Count ?? 0} warnings)";
            }
        }
    }
}