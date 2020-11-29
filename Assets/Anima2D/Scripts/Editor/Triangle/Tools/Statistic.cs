// -----------------------------------------------------------------------
// <copyright file="Statistic.cs">
// Original Triangle code by Jonathan Richard Shewchuk, http://www.cs.cmu.edu/~quake/triangle.html
// Triangle.NET code by Christian Woltering, http://triangle.codeplex.com/
// </copyright>
// -----------------------------------------------------------------------

using System;
using TriangleNet.Data;
using TriangleNet.Geometry;

namespace TriangleNet.Tools
{
    /// <summary>
    ///     Gather mesh statistics.
    /// </summary>
    public class Statistic
    {
        private static readonly int[] plus1Mod3 = {1, 2, 0};
        private static readonly int[] minus1Mod3 = {2, 0, 1};

        #region Private methods

        private void GetAspectHistogram(Mesh mesh)
        {
            int[] aspecttable;
            double[] ratiotable;

            aspecttable = new int[16];
            ratiotable = new[]
            {
                1.5, 2.0, 2.5, 3.0, 4.0, 6.0, 10.0, 15.0, 25.0, 50.0,
                100.0, 300.0, 1000.0, 10000.0, 100000.0, 0.0
            };


            Otri tri = default;
            var p = new Vertex[3];
            double[] dx = new double[3], dy = new double[3];
            var edgelength = new double[3];
            double triarea;
            double trilongest2;
            double triminaltitude2;
            double triaspect2;

            int aspectindex;
            int i, j, k;

            tri.orient = 0;
            foreach (var t in mesh.triangles.Values)
            {
                tri.triangle = t;
                p[0] = tri.Org();
                p[1] = tri.Dest();
                p[2] = tri.Apex();
                trilongest2 = 0.0;

                for (i = 0; i < 3; i++)
                {
                    j = plus1Mod3[i];
                    k = minus1Mod3[i];
                    dx[i] = p[j].x - p[k].x;
                    dy[i] = p[j].y - p[k].y;
                    edgelength[i] = dx[i] * dx[i] + dy[i] * dy[i];
                    if (edgelength[i] > trilongest2) trilongest2 = edgelength[i];
                }

                //triarea = Primitives.CounterClockwise(p[0], p[1], p[2]);
                triarea = Math.Abs((p[2].x - p[0].x) * (p[1].y - p[0].y) -
                                   (p[1].x - p[0].x) * (p[2].y - p[0].y)) / 2.0;

                triminaltitude2 = triarea * triarea / trilongest2;

                triaspect2 = trilongest2 / triminaltitude2;

                aspectindex = 0;
                while (triaspect2 > ratiotable[aspectindex] * ratiotable[aspectindex] && aspectindex < 15)
                    aspectindex++;
                aspecttable[aspectindex]++;
            }
        }

        #endregion

        /// <summary>
        ///     Update statistics about the quality of the mesh.
        /// </summary>
        /// <param name="mesh"></param>
        public void Update(Mesh mesh, int sampleDegrees)
        {
            InputVertices = mesh.invertices;
            InputTriangles = mesh.inelements;
            InputSegments = mesh.insegments;
            InputHoles = mesh.holes.Count;
            Vertices = mesh.vertices.Count - mesh.undeads;
            Triangles = mesh.triangles.Count;
            Edges = mesh.edges;
            BoundaryEdges = mesh.hullsize;
            InteriorBoundaryEdges = mesh.subsegs.Count - mesh.hullsize;
            ConstrainedEdges = mesh.subsegs.Count;

            var p = new Point[3];

            int k1, k2;
            int degreeStep;

            //sampleDegrees = 36; // sample every 5 degrees
            //sampleDegrees = 45; // sample every 4 degrees
            sampleDegrees = 60; // sample every 3 degrees

            var cosSquareTable = new double[sampleDegrees / 2 - 1];
            var dx = new double[3];
            var dy = new double[3];
            var edgeLength = new double[3];
            double dotProduct;
            double cosSquare;
            double triArea;
            double triLongest2;
            double triMinAltitude2;
            double triAspect2;

            var radconst = Math.PI / sampleDegrees;
            var degconst = 180.0 / Math.PI;

            // New angle table
            AngleHistogram = new int[sampleDegrees];
            MinAngleHistogram = new int[sampleDegrees];
            MaxAngleHistogram = new int[sampleDegrees];

            for (var i = 0; i < sampleDegrees / 2 - 1; i++)
            {
                cosSquareTable[i] = Math.Cos(radconst * (i + 1));
                cosSquareTable[i] = cosSquareTable[i] * cosSquareTable[i];
            }

            for (var i = 0; i < sampleDegrees; i++) AngleHistogram[i] = 0;

            ShortestAltitude = mesh.bounds.Width + mesh.bounds.Height;
            ShortestAltitude = ShortestAltitude * ShortestAltitude;
            LargestAspectRatio = 0.0;
            ShortestEdge = ShortestAltitude;
            LongestEdge = 0.0;
            SmallestArea = ShortestAltitude;
            LargestArea = 0.0;
            SmallestAngle = 0.0;
            LargestAngle = 2.0;

            var acuteBiggest = true;
            var acuteBiggestTri = true;

            double triMinAngle, triMaxAngle = 1;

            foreach (var tri in mesh.triangles.Values)
            {
                triMinAngle = 0; // Min angle:  0 < a <  60 degress
                triMaxAngle = 1; // Max angle: 60 < a < 180 degress

                p[0] = tri.vertices[0];
                p[1] = tri.vertices[1];
                p[2] = tri.vertices[2];

                triLongest2 = 0.0;

                for (var i = 0; i < 3; i++)
                {
                    k1 = plus1Mod3[i];
                    k2 = minus1Mod3[i];

                    dx[i] = p[k1].X - p[k2].X;
                    dy[i] = p[k1].Y - p[k2].Y;

                    edgeLength[i] = dx[i] * dx[i] + dy[i] * dy[i];

                    if (edgeLength[i] > triLongest2) triLongest2 = edgeLength[i];

                    if (edgeLength[i] > LongestEdge) LongestEdge = edgeLength[i];

                    if (edgeLength[i] < ShortestEdge) ShortestEdge = edgeLength[i];
                }

                //triarea = Primitives.CounterClockwise(p[0], p[1], p[2]);
                triArea = Math.Abs((p[2].X - p[0].X) * (p[1].Y - p[0].Y) -
                                   (p[1].X - p[0].X) * (p[2].Y - p[0].Y));

                if (triArea < SmallestArea) SmallestArea = triArea;

                if (triArea > LargestArea) LargestArea = triArea;

                triMinAltitude2 = triArea * triArea / triLongest2;
                if (triMinAltitude2 < ShortestAltitude) ShortestAltitude = triMinAltitude2;

                triAspect2 = triLongest2 / triMinAltitude2;
                if (triAspect2 > LargestAspectRatio) LargestAspectRatio = triAspect2;

                for (var i = 0; i < 3; i++)
                {
                    k1 = plus1Mod3[i];
                    k2 = minus1Mod3[i];

                    dotProduct = dx[k1] * dx[k2] + dy[k1] * dy[k2];
                    cosSquare = dotProduct * dotProduct / (edgeLength[k1] * edgeLength[k2]);
                    degreeStep = sampleDegrees / 2 - 1;

                    for (var j = degreeStep - 1; j >= 0; j--)
                        if (cosSquare > cosSquareTable[j])
                            degreeStep = j;

                    if (dotProduct <= 0.0)
                    {
                        AngleHistogram[degreeStep]++;
                        if (cosSquare > SmallestAngle) SmallestAngle = cosSquare;
                        if (acuteBiggest && cosSquare < LargestAngle) LargestAngle = cosSquare;

                        // Update min/max angle per triangle
                        if (cosSquare > triMinAngle) triMinAngle = cosSquare;
                        if (acuteBiggestTri && cosSquare < triMaxAngle) triMaxAngle = cosSquare;
                    }
                    else
                    {
                        AngleHistogram[sampleDegrees - degreeStep - 1]++;
                        if (acuteBiggest || cosSquare > LargestAngle)
                        {
                            LargestAngle = cosSquare;
                            acuteBiggest = false;
                        }

                        // Update max angle for (possibly non-acute) triangle
                        if (acuteBiggestTri || cosSquare > triMaxAngle)
                        {
                            triMaxAngle = cosSquare;
                            acuteBiggestTri = false;
                        }
                    }
                }

                // Update min angle histogram
                degreeStep = sampleDegrees / 2 - 1;

                for (var j = degreeStep - 1; j >= 0; j--)
                    if (triMinAngle > cosSquareTable[j])
                        degreeStep = j;
                MinAngleHistogram[degreeStep]++;

                // Update max angle histogram
                degreeStep = sampleDegrees / 2 - 1;

                for (var j = degreeStep - 1; j >= 0; j--)
                    if (triMaxAngle > cosSquareTable[j])
                        degreeStep = j;

                if (acuteBiggestTri)
                    MaxAngleHistogram[degreeStep]++;
                else
                    MaxAngleHistogram[sampleDegrees - degreeStep - 1]++;

                acuteBiggestTri = true;
            }

            ShortestEdge = Math.Sqrt(ShortestEdge);
            LongestEdge = Math.Sqrt(LongestEdge);
            ShortestAltitude = Math.Sqrt(ShortestAltitude);
            LargestAspectRatio = Math.Sqrt(LargestAspectRatio);
            SmallestArea *= 0.5;
            LargestArea *= 0.5;
            if (SmallestAngle >= 1.0)
                SmallestAngle = 0.0;
            else
                SmallestAngle = degconst * Math.Acos(Math.Sqrt(SmallestAngle));

            if (LargestAngle >= 1.0)
            {
                LargestAngle = 180.0;
            }
            else
            {
                if (acuteBiggest)
                    LargestAngle = degconst * Math.Acos(Math.Sqrt(LargestAngle));
                else
                    LargestAngle = 180.0 - degconst * Math.Acos(Math.Sqrt(LargestAngle));
            }
        }

        #region Static members

        /// <summary>
        ///     Number of incircle tests performed.
        /// </summary>
        public static long InCircleCount = 0;

        public static long InCircleCountDecimal = 0;

        /// <summary>
        ///     Number of counterclockwise tests performed.
        /// </summary>
        public static long CounterClockwiseCount = 0;

        public static long CounterClockwiseCountDecimal = 0;

        /// <summary>
        ///     Number of 3D orientation tests performed.
        /// </summary>
        public static long Orient3dCount = 0;

        /// <summary>
        ///     Number of right-of-hyperbola tests performed.
        /// </summary>
        public static long HyperbolaCount = 0;

        /// <summary>
        ///     // Number of circumcenter calculations performed.
        /// </summary>
        public static long CircumcenterCount = 0;

        /// <summary>
        ///     Number of circle top calculations performed.
        /// </summary>
        public static long CircleTopCount = 0;

        /// <summary>
        ///     Number of vertex relocations.
        /// </summary>
        public static long RelocationCount = 0;

        #endregion

        #region Properties

        /// <summary>
        ///     Gets the shortest edge.
        /// </summary>
        public double ShortestEdge { get; private set; }

        /// <summary>
        ///     Gets the longest edge.
        /// </summary>
        public double LongestEdge { get; private set; }

        //

        /// <summary>
        ///     Gets the shortest altitude.
        /// </summary>
        public double ShortestAltitude { get; private set; }

        /// <summary>
        ///     Gets the largest aspect ratio.
        /// </summary>
        public double LargestAspectRatio { get; private set; }

        /// <summary>
        ///     Gets the smallest area.
        /// </summary>
        public double SmallestArea { get; private set; }

        /// <summary>
        ///     Gets the largest area.
        /// </summary>
        public double LargestArea { get; private set; }

        /// <summary>
        ///     Gets the smallest angle.
        /// </summary>
        public double SmallestAngle { get; private set; }

        /// <summary>
        ///     Gets the largest angle.
        /// </summary>
        public double LargestAngle { get; private set; }

        /// <summary>
        ///     Gets the number of input vertices.
        /// </summary>
        public int InputVertices { get; private set; }

        /// <summary>
        ///     Gets the number of input triangles.
        /// </summary>
        public int InputTriangles { get; private set; }

        /// <summary>
        ///     Gets the number of input segments.
        /// </summary>
        public int InputSegments { get; private set; }

        /// <summary>
        ///     Gets the number of input holes.
        /// </summary>
        public int InputHoles { get; private set; }

        /// <summary>
        ///     Gets the number of mesh vertices.
        /// </summary>
        public int Vertices { get; private set; }

        /// <summary>
        ///     Gets the number of mesh triangles.
        /// </summary>
        public int Triangles { get; private set; }

        /// <summary>
        ///     Gets the number of mesh edges.
        /// </summary>
        public int Edges { get; private set; }

        /// <summary>
        ///     Gets the number of exterior boundary edges.
        /// </summary>
        public int BoundaryEdges { get; private set; }

        /// <summary>
        ///     Gets the number of interior boundary edges.
        /// </summary>
        public int InteriorBoundaryEdges { get; private set; }

        /// <summary>
        ///     Gets the number of constrained edges.
        /// </summary>
        public int ConstrainedEdges { get; private set; }

        /// <summary>
        ///     Gets the angle histogram.
        /// </summary>
        public int[] AngleHistogram { get; private set; }

        /// <summary>
        ///     Gets the min angles histogram.
        /// </summary>
        public int[] MinAngleHistogram { get; private set; }

        /// <summary>
        ///     Gets the max angles histogram.
        /// </summary>
        public int[] MaxAngleHistogram { get; private set; }

        #endregion
    }
}