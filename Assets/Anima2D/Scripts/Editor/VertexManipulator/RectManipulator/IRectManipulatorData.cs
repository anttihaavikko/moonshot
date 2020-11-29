using System.Collections.Generic;
using UnityEngine;

namespace Anima2D
{
    public interface IRectManipulatorData
    {
        List<Vector3> normalizedVertices { get; set; }
    }
}