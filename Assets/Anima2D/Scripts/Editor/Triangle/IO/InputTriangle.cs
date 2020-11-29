// -----------------------------------------------------------------------
// <copyright file="Triangle.cs" company="">
// Triangle.NET code by Christian Woltering, http://triangle.codeplex.com/
// </copyright>
// -----------------------------------------------------------------------

using TriangleNet.Data;
using TriangleNet.Geometry;

namespace TriangleNet.IO
{
    /// <summary>
    ///     Simple triangle class for input.
    /// </summary>
    public class InputTriangle : ITriangle
    {
        internal double area;
        internal int region;
        internal int[] vertices;

        public InputTriangle(int p0, int p1, int p2)
        {
            vertices = new[] {p0, p1, p2};
        }

        #region Public properties

        /// <summary>
        ///     Gets the triangle id.
        /// </summary>
        public int ID => 0;

        /// <summary>
        ///     Gets the first corners vertex id.
        /// </summary>
        public int P0 => vertices[0];

        /// <summary>
        ///     Gets the seconds corners vertex id.
        /// </summary>
        public int P1 => vertices[1];

        /// <summary>
        ///     Gets the third corners vertex id.
        /// </summary>
        public int P2 => vertices[2];

        /// <summary>
        ///     Gets the specified corners vertex.
        /// </summary>
        public Vertex GetVertex(int index)
        {
            return null; // TODO: throw NotSupportedException?
        }

        public bool SupportsNeighbors => false;

        public int N0 => -1;

        public int N1 => -1;

        public int N2 => -1;

        public ITriangle GetNeighbor(int index)
        {
            return null;
        }

        public ISegment GetSegment(int index)
        {
            return null;
        }

        /// <summary>
        ///     Gets the triangle area constraint.
        /// </summary>
        public double Area
        {
            get => area;
            set => area = value;
        }

        /// <summary>
        ///     Region ID the triangle belongs to.
        /// </summary>
        public int Region
        {
            get => region;
            set => region = value;
        }

        #endregion
    }
}