using System.Collections.Generic;
using BitAssistTools.Domain.Models.Transform;
using BitAssistTools.Domain.Models.Placement;
using UnityEngine;

namespace BitAssistTools.Domain.Services
{
    /// <summary>
    /// Transform計算を担当するドメインサービス
    /// </summary>
    public interface ITransformCalculationService
    {
        /// <summary>
        /// 配置ルールに基づいてオブジェクトを整列
        /// </summary>
        /// <param name="transforms">対象のTransform配列</param>
        /// <param name="rule">適用する配置ルール</param>
        /// <returns>新しいTransformData配列</returns>
        TransformData[] AlignObjects(UnityEngine.Transform[] transforms, PlacementRule rule);

        /// <summary>
        /// 複製戦略に基づいて新しいTransform配列を計算
        /// </summary>
        /// <param name="originalTransform">元のTransform</param>
        /// <param name="strategy">複製戦略</param>
        /// <param name="referenceTransform">参照座標系（ローカル座標の場合）</param>
        /// <returns>複製されたTransformData配列</returns>
        TransformData[] CalculateDuplicatedTransforms(
            TransformData originalTransform, 
            DuplicationStrategy strategy,
            UnityEngine.Transform referenceTransform = null);

        /// <summary>
        /// 指定した軸における境界値を計算
        /// </summary>
        /// <param name="transforms">対象のTransform配列</param>
        /// <param name="axis">計算対象の軸</param>
        /// <param name="coordinateReference">座標参照</param>
        /// <returns>（最小値、最大値、中央値）</returns>
        (float min, float max, float center) CalculateAxisBounds(
            UnityEngine.Transform[] transforms, 
            Axis axis, 
            CoordinateReference coordinateReference);

        /// <summary>
        /// 座標系変換を実行
        /// </summary>
        /// <param name="transformData">変換対象のTransformData</param>
        /// <param name="fromCoordinate">変換元座標系</param>
        /// <param name="toCoordinate">変換先座標系</param>
        /// <param name="referenceTransform">参照Transform</param>
        /// <returns>変換後のTransformData</returns>
        TransformData ConvertCoordinateSystem(
            TransformData transformData,
            CoordinateReference fromCoordinate,
            CoordinateReference toCoordinate,
            UnityEngine.Transform referenceTransform);

        /// <summary>
        /// 複数のTransformの重心を計算
        /// </summary>
        /// <param name="transforms">対象のTransform配列</param>
        /// <param name="coordinateReference">座標参照</param>
        /// <returns>重心座標</returns>
        Vector3 CalculateCentroid(UnityEngine.Transform[] transforms, CoordinateReference coordinateReference);

        /// <summary>
        /// ランダム化されたTransformDataを生成
        /// </summary>
        /// <param name="baseTransform">ベースとなるTransformData</param>
        /// <param name="positionRange">位置のランダム範囲</param>
        /// <param name="rotationRange">回転のランダム範囲（度）</param>
        /// <param name="scaleRange">スケールのランダム範囲</param>
        /// <param name="flags">適用する変換フラグ</param>
        /// <returns>ランダム化されたTransformData</returns>
        TransformData RandomizeTransform(
            TransformData baseTransform,
            Vector3 positionRange,
            Vector3 rotationRange,
            Vector2 scaleRange,
            TransformFlags flags);
    }
}