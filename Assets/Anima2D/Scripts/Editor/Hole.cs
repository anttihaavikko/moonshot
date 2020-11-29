using System;
using UnityEngine;

namespace Anima2D
{
    [Serializable]
    public class Hole : ICloneable
    {
        public Vector2 vertex = Vector2.zero;

        public Hole(Vector2 vertex)
        {
            this.vertex = vertex;
        }

        public object Clone()
        {
            return MemberwiseClone();
        }

        public static implicit operator bool(Hole h)
        {
            return h != null;
        }
    }
}