// -----------------------------------------------------------------------
// <copyright file="IVoronoi.cs" company="">
// Triangle.NET code by Christian Woltering, http://triangle.codeplex.com/
// </copyright>
// -----------------------------------------------------------------------

using System.Collections.Generic;
using TriangleNet.Geometry;

namespace TriangleNet.Tools
{
    /// <summary>
    ///     TODO: Update summary.
    /// </summary>
    public interface IVoronoi
    {
        /// <summary>
        ///     Gets the list of Voronoi vertices.
        /// </summary>
        Point[] Points { get; }

        /// <summary>
        ///     Gets the list of Voronoi regions.
        /// </summary>
        List<VoronoiRegion> Regions { get; }
    }
}