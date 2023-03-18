using UnityEngine;

namespace PlacemenAssistance
{
    public struct TransformData
    {
        public Vector3 position;
        public Quaternion rotation;
        public Vector3 localScale;

        public TransformData(Vector3 position, Quaternion rotation, Vector3 localScale)
        {
            this.position = position;
            this.rotation = rotation;
            this.localScale = localScale;
        }
    }

    public struct AxisFlags
    {
        public bool X;
        public bool Y;
        public bool Z;
    }

    public enum AlignType
    {
        Maximum,
        Center,
        Minimum
    }

    public enum Axis
    {
        X,
        Y,
        Z
    }
}