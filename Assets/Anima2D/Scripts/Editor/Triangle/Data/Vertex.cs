﻿// -----------------------------------------------------------------------
// <copyright file="Vertex.cs" company="">
// Original Triangle code by Jonathan Richard Shewchuk, http://www.cs.cmu.edu/~quake/triangle.html
// Triangle.NET code by Christian Woltering, http://triangle.codeplex.com/
// </copyright>
// -----------------------------------------------------------------------

using System;
using TriangleNet.Geometry;

namespace TriangleNet.Data
{
    //using System;
    /// <summary>
    ///     The vertex data structure.
    /// </summary>
    public class Vertex : Point
    {
        // Hash for dictionary. Will be set by mesh instance.
        internal int hash;
        internal Otri tri;

        internal VertexType type;

        /// <summary>
        ///     Initializes a new instance of the <see cref="Vertex" /> class.
        /// </summary>
        public Vertex()
            : this(0, 0, 0, 0)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="Vertex" /> class.
        /// </summary>
        /// <param name="x">The x coordinate of the vertex.</param>
        /// <param name="y">The y coordinate of the vertex.</param>
        public Vertex(double x, double y)
            : this(x, y, 0, 0)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="Vertex" /> class.
        /// </summary>
        /// <param name="x">The x coordinate of the vertex.</param>
        /// <param name="y">The y coordinate of the vertex.</param>
        /// <param name="mark">The boundary mark.</param>
        public Vertex(double x, double y, int mark)
            : this(x, y, mark, 0)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="Vertex" /> class.
        /// </summary>
        /// <param name="x">The x coordinate of the vertex.</param>
        /// <param name="y">The y coordinate of the vertex.</param>
        /// <param name="mark">The boundary mark.</param>
        /// <param name="attribs">The number of point attributes.</param>
        public Vertex(double x, double y, int mark, int attribs)
            : base(x, y, mark)
        {
            type = VertexType.InputVertex;

            if (attribs > 0) attributes = new double[attribs];
        }

        public override int GetHashCode()
        {
            return hash;
        }

        #region Public properties

        /// <summary>
        ///     Gets the vertex type.
        /// </summary>
        public VertexType Type => type;

        /// <summary>
        ///     Gets the specified coordinate of the vertex.
        /// </summary>
        /// <param name="i">Coordinate index.</param>
        /// <returns>X coordinate, if index is 0, Y coordinate, if index is 1.</returns>
        public double this[int i]
        {
            get
            {
                if (i == 0) return x;

                if (i == 1) return y;

                throw new ArgumentOutOfRangeException("Index must be 0 or 1.");
            }
        }

        #endregion
    }
}