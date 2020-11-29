// -----------------------------------------------------------------------
// <copyright file="VoronoiRegion.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

using System.Collections.Generic;
using TriangleNet.Data;
using TriangleNet.Geometry;

namespace TriangleNet.Tools
{
    /// <summary>
    ///     Represents a region in the Voronoi diagram.
    /// </summary>
    public class VoronoiRegion
    {
        private readonly List<Point> vertices;

        public VoronoiRegion(Vertex generator)
        {
            ID = generator.id;
            Generator = generator;
            vertices = new List<Point>();
            Bounded = true;
        }

        /// <summary>
        ///     Gets the Voronoi region id (which is the same as the generators vertex id).
        /// </summary>
        public int ID { get; }

        /// <summary>
        ///     Gets the Voronoi regions generator.
        /// </summary>
        public Point Generator { get; }

        /// <summary>
        ///     Gets the Voronoi vertices on the regions boundary.
        /// </summary>
        public ICollection<Point> Vertices => vertices;

        /// <summary>
        ///     Gets or sets whether the Voronoi region is bounded.
        /// </summary>
        public bool Bounded { get; set; }

        public void Add(Point point)
        {
            vertices.Add(point);
        }

        public void Add(List<Point> points)
        {
            vertices.AddRange(points);
        }

        public override string ToString()
        {
            return string.Format("R-ID {0}", ID);
        }
    }
}