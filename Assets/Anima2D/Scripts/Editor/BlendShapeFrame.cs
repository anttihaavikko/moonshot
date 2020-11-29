using System;
using UnityEngine;

namespace Anima2D
{
    [Serializable]
    public class BlendShapeFrame : ScriptableObject
    {
        public float weight;
        public Vector3[] vertices;

        public static BlendShapeFrame Create(float weight, Vector3[] vertices)
        {
            var frame = CreateInstance<BlendShapeFrame>();

            frame.vertices = vertices;

            frame.weight = weight;

            return frame;
        }
    }
}