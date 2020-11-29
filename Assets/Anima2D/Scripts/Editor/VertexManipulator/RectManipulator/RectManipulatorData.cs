using System.Collections.Generic;
using UnityEngine;

namespace Anima2D
{
    public class RectManipulatorData : IRectManipulatorData
    {
        public List<Vector3> normalizedVertices { get; set; } = new List<Vector3>();
    }
}