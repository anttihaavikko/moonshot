// -----------------------------------------------------------------------
// <copyright file="InputGeometry.cs" company="">
// Triangle.NET code by Christian Woltering, http://triangle.codeplex.com/
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using TriangleNet.Data;

namespace TriangleNet.Geometry
{
    /// <summary>
    ///     The input geometry which will be triangulated. May represent a
    ///     pointset or a planar straight line graph.
    /// </summary>
    public class InputGeometry
    {
        internal List<Point> holes;

        // Used to check consitent use of point attributes.
        private int pointAttributes = -1;
        internal List<Vertex> points;
        internal List<RegionPointer> regions;
        internal List<Edge> segments;

        /// <summary>
        ///     Initializes a new instance of the <see cref="InputGeometry" /> class.
        /// </summary>
        public InputGeometry()
            : this(3)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="InputGeometry" /> class.
        ///     The point list will be initialized with a given capacity.
        /// </summary>
        /// <param name="capacity">Point list capacity.</param>
        public InputGeometry(int capacity)
        {
            points = new List<Vertex>(capacity);
            segments = new List<Edge>();
            holes = new List<Point>();
            regions = new List<RegionPointer>();

            Bounds = new BoundingBox();

            pointAttributes = -1;
        }

        /// <summary>
        ///     Gets the bounding box of the input geometry.
        /// </summary>
        public BoundingBox Bounds { get; }

        /// <summary>
        ///     Indicates, whether the geometry should be treated as a PSLG.
        /// </summary>
        public bool HasSegments => segments.Count > 0;

        /// <summary>
        ///     Gets the number of points.
        /// </summary>
        public int Count => points.Count;

        /// <summary>
        ///     Gets the list of input points.
        /// </summary>
        public IEnumerable<Point> Points => points;

        /// <summary>
        ///     Gets the list of input segments.
        /// </summary>
        public ICollection<Edge> Segments => segments;

        /// <summary>
        ///     Gets the list of input holes.
        /// </summary>
        public ICollection<Point> Holes => holes;

        /// <summary>
        ///     Gets the list of regions.
        /// </summary>
        public ICollection<RegionPointer> Regions => regions;

        /// <summary>
        ///     Clear input geometry.
        /// </summary>
        public void Clear()
        {
            points.Clear();
            segments.Clear();
            holes.Clear();
            regions.Clear();

            pointAttributes = -1;
        }

        /// <summary>
        ///     Adds a point to the geometry.
        /// </summary>
        /// <param name="x">X coordinate.</param>
        /// <param name="y">Y coordinate.</param>
        public void AddPoint(double x, double y)
        {
            AddPoint(x, y, 0);
        }

        /// <summary>
        ///     Adds a point to the geometry.
        /// </summary>
        /// <param name="x">X coordinate.</param>
        /// <param name="y">Y coordinate.</param>
        /// <param name="boundary">Boundary marker.</param>
        public void AddPoint(double x, double y, int boundary)
        {
            points.Add(new Vertex(x, y, boundary));

            Bounds.Update(x, y);
        }

        /// <summary>
        ///     Adds a point to the geometry.
        /// </summary>
        /// <param name="x">X coordinate.</param>
        /// <param name="y">Y coordinate.</param>
        /// <param name="boundary">Boundary marker.</param>
        /// <param name="attribute">Point attribute.</param>
        public void AddPoint(double x, double y, int boundary, double attribute)
        {
            AddPoint(x, y, 0, new[] {attribute});
        }

        /// <summary>
        ///     Adds a point to the geometry.
        /// </summary>
        /// <param name="x">X coordinate.</param>
        /// <param name="y">Y coordinate.</param>
        /// <param name="boundary">Boundary marker.</param>
        /// <param name="attribs">Point attributes.</param>
        public void AddPoint(double x, double y, int boundary, double[] attribs)
        {
            if (pointAttributes < 0)
                pointAttributes = attribs == null ? 0 : attribs.Length;
            else if (attribs == null && pointAttributes > 0)
                throw new ArgumentException("Inconsitent use of point attributes.");
            else if (attribs != null && pointAttributes != attribs.Length)
                throw new ArgumentException("Inconsitent use of point attributes.");

            points.Add(new Vertex(x, y, boundary) {attributes = attribs});

            Bounds.Update(x, y);
        }

        /// <summary>
        ///     Adds a hole location to the geometry.
        /// </summary>
        /// <param name="x">X coordinate of the hole.</param>
        /// <param name="y">Y coordinate of the hole.</param>
        public void AddHole(double x, double y)
        {
            holes.Add(new Point(x, y));
        }

        /// <summary>
        ///     Adds a hole location to the geometry.
        /// </summary>
        /// <param name="x">X coordinate of the hole.</param>
        /// <param name="y">Y coordinate of the hole.</param>
        /// <param name="id">The region id.</param>
        public void AddRegion(double x, double y, int id)
        {
            regions.Add(new RegionPointer(x, y, id));
        }

        /// <summary>
        ///     Adds a segment to the geometry.
        /// </summary>
        /// <param name="p0">First endpoint.</param>
        /// <param name="p1">Second endpoint.</param>
        public void AddSegment(int p0, int p1)
        {
            AddSegment(p0, p1, 0);
        }

        /// <summary>
        ///     Adds a segment to the geometry.
        /// </summary>
        /// <param name="p0">First endpoint.</param>
        /// <param name="p1">Second endpoint.</param>
        /// <param name="boundary">Segment marker.</param>
        public void AddSegment(int p0, int p1, int boundary)
        {
            if (p0 == p1 || p0 < 0 || p1 < 0) throw new NotSupportedException("Invalid endpoints.");

            segments.Add(new Edge(p0, p1, boundary));
        }
    }
}