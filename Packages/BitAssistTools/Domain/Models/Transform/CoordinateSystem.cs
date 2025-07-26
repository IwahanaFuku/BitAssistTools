using System;
using UnityEngine;

namespace BitAssistTools.Domain.Models.Transform
{
    /// <summary>
    /// 座標系の種類を表現する値オブジェクト
    /// </summary>
    public enum CoordinateReference
    {
        /// <summary>ワールド座標系</summary>
        Global,
        /// <summary>ローカル座標系</summary>
        Local
    }

    /// <summary>
    /// 座標軸を表現する値オブジェクト
    /// </summary>
    public enum Axis
    {
        X,
        Y,
        Z
    }

    /// <summary>
    /// 軸フラグを表現する値オブジェクト
    /// ビットフラグとして使用可能
    /// </summary>
    [Flags]
    public enum AxisFlags
    {
        None = 0,
        X = 1 << 0,
        Y = 1 << 1,
        Z = 1 << 2,
        XY = X | Y,
        XZ = X | Z,
        YZ = Y | Z,
        All = X | Y | Z
    }

    /// <summary>
    /// Transform操作の種類を表現する値オブジェクト
    /// </summary>
    [Flags]
    public enum TransformFlags
    {
        None = 0,
        Position = 1 << 0,
        Rotation = 1 << 1,
        Scale = 1 << 2,
        All = Position | Rotation | Scale
    }

    /// <summary>
    /// 座標系変換を担当するドメインサービス
    /// </summary>
    public static class CoordinateSystemConverter
    {
        /// <summary>
        /// グローバル座標からローカル座標に変換
        /// </summary>
        public static Vector3 GlobalToLocal(Vector3 globalPosition, UnityEngine.Transform localSpace)
        {
            if (localSpace == null)
                return globalPosition;

            return localSpace.InverseTransformPoint(globalPosition);
        }

        /// <summary>
        /// ローカル座標からグローバル座標に変換
        /// </summary>
        public static Vector3 LocalToGlobal(Vector3 localPosition, UnityEngine.Transform localSpace)
        {
            if (localSpace == null)
                return localPosition;

            return localSpace.TransformPoint(localPosition);
        }

        /// <summary>
        /// グローバル方向からローカル方向に変換
        /// </summary>
        public static Vector3 GlobalDirectionToLocal(Vector3 globalDirection, UnityEngine.Transform localSpace)
        {
            if (localSpace == null)
                return globalDirection;

            return localSpace.InverseTransformDirection(globalDirection);
        }

        /// <summary>
        /// ローカル方向からグローバル方向に変換
        /// </summary>
        public static Vector3 LocalDirectionToGlobal(Vector3 localDirection, UnityEngine.Transform localSpace)
        {
            if (localSpace == null)
                return localDirection;

            return localSpace.TransformDirection(localDirection);
        }

        /// <summary>
        /// 座標系に応じてTransformDataを変換
        /// </summary>
        public static TransformData ConvertTransformData(
            TransformData data, 
            CoordinateReference fromCoordinate, 
            CoordinateReference toCoordinate,
            UnityEngine.Transform referenceTransform = null)
        {
            if (fromCoordinate == toCoordinate || referenceTransform == null)
                return data;

            if (fromCoordinate == CoordinateReference.Global && toCoordinate == CoordinateReference.Local)
            {
                return new TransformData(
                    GlobalToLocal(data.Position, referenceTransform),
                    Quaternion.Inverse(referenceTransform.rotation) * data.Rotation,
                    data.LocalScale
                );
            }
            else if (fromCoordinate == CoordinateReference.Local && toCoordinate == CoordinateReference.Global)
            {
                return new TransformData(
                    LocalToGlobal(data.Position, referenceTransform),
                    referenceTransform.rotation * data.Rotation,
                    data.LocalScale
                );
            }
            else
            {
                return data;
            }
        }
    }

    /// <summary>
    /// 軸フラグのヘルパーメソッド
    /// </summary>
    public static class AxisFlagsExtensions
    {
        /// <summary>
        /// 指定した軸が含まれているかチェック
        /// </summary>
        public static bool HasAxis(this AxisFlags flags, Axis axis)
        {
            switch (axis)
            {
                case Axis.X:
                    return flags.HasFlag(AxisFlags.X);
                case Axis.Y:
                    return flags.HasFlag(AxisFlags.Y);
                case Axis.Z:
                    return flags.HasFlag(AxisFlags.Z);
                default:
                    return false;
            }
        }

        /// <summary>
        /// Vector3に軸フラグを適用
        /// </summary>
        public static Vector3 ApplyToVector(this AxisFlags flags, Vector3 original, Vector3 modified)
        {
            return new Vector3(
                flags.HasFlag(AxisFlags.X) ? modified.x : original.x,
                flags.HasFlag(AxisFlags.Y) ? modified.y : original.y,
                flags.HasFlag(AxisFlags.Z) ? modified.z : original.z
            );
        }

        /// <summary>
        /// 軸フラグに基づいてベクトル成分をマスク
        /// </summary>
        public static Vector3 MaskVector(this AxisFlags flags, Vector3 vector)
        {
            return new Vector3(
                flags.HasFlag(AxisFlags.X) ? vector.x : 0f,
                flags.HasFlag(AxisFlags.Y) ? vector.y : 0f,
                flags.HasFlag(AxisFlags.Z) ? vector.z : 0f
            );
        }
    }
}