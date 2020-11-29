// -----------------------------------------------------------------------
// <copyright file="RegionPointer.cs" company="">
// Triangle.NET code by Christian Woltering, http://triangle.codeplex.com/
// </copyright>
// -----------------------------------------------------------------------

namespace TriangleNet.Geometry
{
    /// <summary>
    ///     Pointer to a region in the mesh geometry. A region is a well-defined
    ///     subset of the geomerty (enclosed by subsegments).
    /// </summary>
    public class RegionPointer
    {
        internal int id;
        internal Point point;

        /// <summary>
        ///     Initializes a new instance of the <see cref="RegionPointer" /> class.
        /// </summary>
        /// <param name="x">X coordinate of the region.</param>
        /// <param name="y">Y coordinate of the region.</param>
        /// <param name="id">Region id.</param>
        public RegionPointer(double x, double y, int id)
        {
            point = new Point(x, y);
            this.id = id;
        }
    }
}