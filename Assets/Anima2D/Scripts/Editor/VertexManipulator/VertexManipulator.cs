using System.Collections.Generic;

namespace Anima2D
{
    public class VertexManipulator : IVertexManipulator
    {
        public List<IVertexManipulable> manipulables { get; } = new List<IVertexManipulable>();

        public void AddVertexManipulable(IVertexManipulable vertexManipulable)
        {
            if (vertexManipulable != null) manipulables.Add(vertexManipulable);
        }

        public void Clear()
        {
            manipulables.Clear();
        }

        public virtual void DoManipulate()
        {
        }
    }
}