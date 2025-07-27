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
    /// 整列タイプを表現する値オブジェクト
    /// </summary>
    public enum AlignmentType
    {
        Minimum,
        Center,
        Maximum
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
    /// 座標系を表現する値オブジェクト
    /// </summary>
    public readonly struct CoordinateSystem : IEquatable<CoordinateSystem>
    {
        public readonly CoordinateReference Reference;

        public CoordinateSystem(CoordinateReference reference)
        {
            Reference = reference;
        }

        public bool IsGlobal => Reference == CoordinateReference.Global;
        public bool IsLocal => Reference == CoordinateReference.Local;

        public static CoordinateSystem Global => new CoordinateSystem(CoordinateReference.Global);
        public static CoordinateSystem Local => new CoordinateSystem(CoordinateReference.Local);

        public static implicit operator CoordinateSystem(CoordinateReference reference)
        {
            return new CoordinateSystem(reference);
        }

        public static implicit operator CoordinateReference(CoordinateSystem system)
        {
            return system.Reference;
        }

        public bool Equals(CoordinateSystem other)
        {
            return Reference == other.Reference;
        }

        public override bool Equals(object obj)
        {
            return obj is CoordinateSystem other && Equals(other);
        }

        public override int GetHashCode()
        {
            return (int)Reference;
        }

        public override string ToString()
        {
            return Reference.ToString();
        }
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
    /// 軸フラグの構造体版
    /// </summary>
    public readonly struct AxisFlags : IEquatable<AxisFlags>
    {
        public readonly bool X;
        public readonly bool Y;
        public readonly bool Z;

        public AxisFlags(bool x, bool y, bool z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public static AxisFlags All => new AxisFlags(true, true, true);
        public static AxisFlags None => new AxisFlags(false, false, false);
        public static AxisFlags XOnly => new AxisFlags(true, false, false);
        public static AxisFlags YOnly => new AxisFlags(false, true, false);
        public static AxisFlags ZOnly => new AxisFlags(false, false, true);

        public bool HasAnyAxis => X || Y || Z;
        public bool HasAllAxes => X && Y && Z;

        public Vector3 ApplyToVector(Vector3 original, Vector3 modified)
        {
            return new Vector3(
                X ? modified.x : original.x,
                Y ? modified.y : original.y,
                Z ? modified.z : original.z
            );
        }

        public Vector3 MaskVector(Vector3 vector)
        {
            return new Vector3(
                X ? vector.x : 0f,
                Y ? vector.y : 0f,
                Z ? vector.z : 0f
            );
        }

        public bool Equals(AxisFlags other)
        {
            return X == other.X && Y == other.Y && Z == other.Z;
        }

        public override bool Equals(object obj)
        {
            return obj is AxisFlags other && Equals(other);
        }

        public override int GetHashCode()
        {
            return (X ? 1 : 0) | (Y ? 2 : 0) | (Z ? 4 : 0);
        }

        public override string ToString()
        {
            var axes = new System.Collections.Generic.List<string>();
            if (X) axes.Add("X");
            if (Y) axes.Add("Y");
            if (Z) axes.Add("Z");
            return axes.Count > 0 ? string.Join(", ", axes) : "None";
        }
    }

}