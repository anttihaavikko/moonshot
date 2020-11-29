using System;
using UnityEngine;

namespace Anima2D
{
    [Serializable]
    public class BlendShape : ScriptableObject
    {
        public BlendShapeFrame[] frames = new BlendShapeFrame[0];

        public static BlendShape Create(string name)
        {
            var blendShape = CreateInstance<BlendShape>();
            blendShape.name = name;
            return blendShape;
        }
    }
}