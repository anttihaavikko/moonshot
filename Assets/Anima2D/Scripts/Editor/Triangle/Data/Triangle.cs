// -----------------------------------------------------------------------
// <copyright file="Triangle.cs" company="">
// Original Triangle code by Jonathan Richard Shewchuk, http://www.cs.cmu.edu/~quake/triangle.html
// Triangle.NET code by Christian Woltering, http://triangle.codeplex.com/
// </copyright>
// -----------------------------------------------------------------------

using TriangleNet.Geometry;

namespace TriangleNet.Data
{
    /// <summary>
    ///     The triangle data structure.
    /// </summary>
    /// <remarks>
    ///     Each triangle contains three pointers to adjoining triangles, plus three
    ///     pointers to vertices, plus three pointers to subsegments (declared below;
    ///     these pointers are usually 'dummysub'). It may or may not also contain
    ///     user-defined attributes and/or a floating-point "area constraint".
    /// </remarks>
    public class Triangle : ITriangle
    {
        internal double area;

        // Hash for dictionary. Will be set by mesh instance.
        internal int hash;

        // The ID is only used for mesh output.
        internal int id;
        internal bool infected;

        internal Otri[] neighbors;
        internal int region;
        internal Osub[] subsegs;
        internal Vertex[] vertices;

        public Triangle()
        {
            // Initialize the three adjoining triangles to be "outer space".
            neighbors = new Otri[3];
            neighbors[0].triangle = Mesh.dummytri;
            neighbors[1].triangle = Mesh.dummytri;
            neighbors[2].triangle = Mesh.dummytri;

            // Three NULL vertices.
            vertices = new Vertex[3];

            // TODO: if (Behavior.UseSegments)
            {
                // Initialize the three adjoining subsegments to be the
                // omnipresent subsegment.
                subsegs = new Osub[3];
                subsegs[0].seg = Mesh.dummysub;
                subsegs[1].seg = Mesh.dummysub;
                subsegs[2].seg = Mesh.dummysub;
            }

            // TODO:
            //if (Behavior.VarArea)
            //{
            //    area = -1.0;
            //}
        }

        public override int GetHashCode()
        {
            return hash;
        }

        public override string ToString()
        {
            return string.Format("TID {0}", hash);
        }

        #region Public properties

        /// <summary>
        ///     Gets the triangle id.
        /// </summary>
        public int ID => id;

        /// <summary>
        ///     Gets the first corners vertex id.
        /// </summary>
        public int P0 => vertices[0] == null ? -1 : vertices[0].id;

        /// <summary>
        ///     Gets the seconds corners vertex id.
        /// </summary>
        public int P1 => vertices[1] == null ? -1 : vertices[1].id;

        /// <summary>
        ///     Gets the specified corners vertex.
        /// </summary>
        public Vertex GetVertex(int index)
        {
            return vertices[index]; // TODO: Check range?
        }

        /// <summary>
        ///     Gets the third corners vertex id.
        /// </summary>
        public int P2 => vertices[2] == null ? -1 : vertices[2].id;

        public bool SupportsNeighbors => true;

        /// <summary>
        ///     Gets the first neighbors id.
        /// </summary>
        public int N0 => neighbors[0].triangle.id;

        /// <summary>
        ///     Gets the second neighbors id.
        /// </summary>
        public int N1 => neighbors[1].triangle.id;

        /// <summary>
        ///     Gets the third neighbors id.
        /// </summary>
        public int N2 => neighbors[2].triangle.id;

        /// <summary>
        ///     Gets a triangles' neighbor.
        /// </summary>
        /// <param name="index">The neighbor index (0, 1 or 2).</param>
        /// <returns>The neigbbor opposite of vertex with given index.</returns>
        public ITriangle GetNeighbor(int index)
        {
            return neighbors[index].triangle == Mesh.dummytri ? null : neighbors[index].triangle;
        }

        /// <summary>
        ///     Gets a triangles segment.
        /// </summary>
        /// <param name="index">The vertex index (0, 1 or 2).</param>
        /// <returns>The segment opposite of vertex with given index.</returns>
        public ISegment GetSegment(int index)
        {
            return subsegs[index].seg == Mesh.dummysub ? null : subsegs[index].seg;
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
        public int Region => region;

        #endregion
    }
}