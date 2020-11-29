// -----------------------------------------------------------------------
// <copyright file="Segment.cs" company="">
// Original Triangle code by Jonathan Richard Shewchuk, http://www.cs.cmu.edu/~quake/triangle.html
// Triangle.NET code by Christian Woltering, http://triangle.codeplex.com/
// </copyright>
// -----------------------------------------------------------------------

using TriangleNet.Geometry;

namespace TriangleNet.Data
{
    /// <summary>
    ///     The subsegment data structure.
    /// </summary>
    /// <remarks>
    ///     Each subsegment contains two pointers to adjoining subsegments, plus
    ///     four pointers to vertices, plus two pointers to adjoining triangles,
    ///     plus one boundary marker.
    /// </remarks>
    public class Segment : ISegment
    {
        internal int boundary;

        // Hash for dictionary. Will be set by mesh instance.
        internal int hash;

        internal Osub[] subsegs;
        internal Otri[] triangles;
        internal Vertex[] vertices;

        public Segment()
        {
            // Initialize the two adjoining subsegments to be the omnipresent
            // subsegment.
            subsegs = new Osub[2];
            subsegs[0].seg = Mesh.dummysub;
            subsegs[1].seg = Mesh.dummysub;

            // Four NULL vertices.
            vertices = new Vertex[4];

            // Initialize the two adjoining triangles to be "outer space."
            triangles = new Otri[2];
            triangles[0].triangle = Mesh.dummytri;
            triangles[1].triangle = Mesh.dummytri;

            // Set the boundary marker to zero.
            boundary = 0;
        }

        /// <summary>
        ///     Gets the segments endpoint.
        /// </summary>
        public Vertex GetVertex(int index)
        {
            return vertices[index]; // TODO: Check range?
        }

        /// <summary>
        ///     Gets an adjoining triangle.
        /// </summary>
        public ITriangle GetTriangle(int index)
        {
            return triangles[index].triangle == Mesh.dummytri ? null : triangles[index].triangle;
        }

        public override int GetHashCode()
        {
            return hash;
        }

        public override string ToString()
        {
            return string.Format("SID {0}", hash);
        }

        #region Public properties

        /// <summary>
        ///     Gets the first endpoints vertex id.
        /// </summary>
        public int P0 => vertices[0].id;

        /// <summary>
        ///     Gets the seconds endpoints vertex id.
        /// </summary>
        public int P1 => vertices[1].id;

        /// <summary>
        ///     Gets the segment boundary mark.
        /// </summary>
        public int Boundary => boundary;

        #endregion
    }
}