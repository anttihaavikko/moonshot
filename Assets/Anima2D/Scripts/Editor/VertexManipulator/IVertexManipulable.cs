using UnityEngine;

namespace Anima2D
{
    public interface IVertexManipulable
    {
        int GetManipulableVertexCount();
        Vector3 GetManipulableVertex(int index);
        void SetManipulatedVertex(int index, Vector3 vertex);
    }
}