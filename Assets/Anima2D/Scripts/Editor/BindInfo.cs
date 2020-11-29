using System;
using UnityEngine;

namespace Anima2D
{
    [Serializable]
    public class BindInfo : ICloneable
    {
        public Matrix4x4 bindPose;
        public float boneLength;

        public string path;
        public string name;

        public Color color;
        public int zOrder;

        public Vector3 position => bindPose.inverse * new Vector4(0f, 0f, 0f, 1f);
        public Vector3 endPoint => bindPose.inverse * new Vector4(boneLength, 0f, 0f, 1f);

        public object Clone()
        {
            return MemberwiseClone();
        }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
                return false;

            var p = (BindInfo) obj;

            return Mathf.Approximately((position - p.position).sqrMagnitude, 0f) &&
                   Mathf.Approximately((endPoint - p.endPoint).sqrMagnitude, 0f);
        }

        public override int GetHashCode()
        {
            return position.GetHashCode() ^ endPoint.GetHashCode();
        }

        public static implicit operator bool(BindInfo b)
        {
            return b != null;
        }
    }
}