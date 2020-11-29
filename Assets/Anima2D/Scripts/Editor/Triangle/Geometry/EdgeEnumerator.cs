// -----------------------------------------------------------------------
// <copyright file="EdgeEnumerator.cs" company="">
// Triangle.NET code by Christian Woltering, http://triangle.codeplex.com/
// </copyright>
// -----------------------------------------------------------------------

using System.Collections;
using System.Collections.Generic;
using TriangleNet.Data;

namespace TriangleNet.Geometry
{
    /// <summary>
    ///     Enumerates the edges of a triangulation.
    /// </summary>
    public class EdgeEnumerator : IEnumerator<Edge>
    {
        private Otri neighbor = default;
        private Vertex p1, p2;
        private Osub sub = default;
        private Otri tri = default;
        private readonly IEnumerator<Triangle> triangles;

        /// <summary>
        ///     Initializes a new instance of the <see cref="EdgeEnumerator" /> class.
        /// </summary>
        public EdgeEnumerator(Mesh mesh)
        {
            triangles = mesh.triangles.Values.GetEnumerator();
            triangles.MoveNext();

            tri.triangle = triangles.Current;
            tri.orient = 0;
        }

        public Edge Current { get; private set; }

        public void Dispose()
        {
            triangles.Dispose();
        }

        object IEnumerator.Current => Current;

        public bool MoveNext()
        {
            if (tri.triangle == null) return false;

            Current = null;

            while (Current == null)
            {
                if (tri.orient == 3)
                {
                    if (triangles.MoveNext())
                    {
                        tri.triangle = triangles.Current;
                        tri.orient = 0;
                    }
                    else
                    {
                        // Finally no more triangles
                        return false;
                    }
                }

                tri.Sym(ref neighbor);

                if (tri.triangle.id < neighbor.triangle.id || neighbor.triangle == Mesh.dummytri)
                {
                    p1 = tri.Org();
                    p2 = tri.Dest();

                    tri.SegPivot(ref sub);

                    // Boundary mark of dummysub is 0, so we don't need to worry about that.
                    Current = new Edge(p1.id, p2.id, sub.seg.boundary);
                }

                tri.orient++;
            }

            return true;
        }

        public void Reset()
        {
            triangles.Reset();
        }
    }
}