﻿// -----------------------------------------------------------------------
// <copyright file="Edge.cs" company="">
// Triangle.NET code by Christian Woltering, http://triangle.codeplex.com/
// </copyright>
// -----------------------------------------------------------------------

namespace TriangleNet.Geometry
{
    /// <summary>
    ///     Represents a straight line segment in 2D space.
    /// </summary>
    public class Edge
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="Edge" /> class.
        /// </summary>
        public Edge(int p0, int p1)
            : this(p0, p1, 0)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="Edge" /> class.
        /// </summary>
        public Edge(int p0, int p1, int boundary)
        {
            P0 = p0;
            P1 = p1;
            Boundary = boundary;
        }

        /// <summary>
        ///     Gets the first endpoints index.
        /// </summary>
        public int P0 { get; }

        /// <summary>
        ///     Gets the second endpoints index.
        /// </summary>
        public int P1 { get; }

        /// <summary>
        ///     Gets the segments boundary mark.
        /// </summary>
        public int Boundary { get; }
    }
}