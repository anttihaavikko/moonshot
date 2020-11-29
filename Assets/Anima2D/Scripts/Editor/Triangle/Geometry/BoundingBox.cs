// -----------------------------------------------------------------------
// <copyright file="BoundingBox.cs" company="">
// Triangle.NET code by Christian Woltering, http://triangle.codeplex.com/
// </copyright>
// -----------------------------------------------------------------------

using System;

namespace TriangleNet.Geometry
{
    /// <summary>
    ///     A simple bounding box class.
    /// </summary>
    public class BoundingBox
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="BoundingBox" /> class.
        /// </summary>
        public BoundingBox()
        {
            Xmin = double.MaxValue;
            Ymin = double.MaxValue;
            Xmax = -double.MaxValue;
            Ymax = -double.MaxValue;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="BoundingBox" /> class
        ///     with predefined bounds.
        /// </summary>
        /// <param name="xmin">Minimum x value.</param>
        /// <param name="ymin">Minimum y value.</param>
        /// <param name="xmax">Maximum x value.</param>
        /// <param name="ymax">Maximum y value.</param>
        public BoundingBox(double xmin, double ymin, double xmax, double ymax)
        {
            this.Xmin = xmin;
            this.Ymin = ymin;
            this.Xmax = xmax;
            this.Ymax = ymax;
        }

        /// <summary>
        ///     Gets the minimum x value (left boundary).
        /// </summary>
        public double Xmin { get; private set; }

        /// <summary>
        ///     Gets the minimum y value (bottom boundary).
        /// </summary>
        public double Ymin { get; private set; }

        /// <summary>
        ///     Gets the maximum x value (right boundary).
        /// </summary>
        public double Xmax { get; private set; }

        /// <summary>
        ///     Gets the maximum y value (top boundary).
        /// </summary>
        public double Ymax { get; private set; }

        /// <summary>
        ///     Gets the width of the bounding box.
        /// </summary>
        public double Width => Xmax - Xmin;

        /// <summary>
        ///     Gets the height of the bounding box.
        /// </summary>
        public double Height => Ymax - Ymin;

        /// <summary>
        ///     Update bounds.
        /// </summary>
        /// <param name="x">X coordinate.</param>
        /// <param name="y">Y coordinate.</param>
        public void Update(double x, double y)
        {
            Xmin = Math.Min(Xmin, x);
            Ymin = Math.Min(Ymin, y);
            Xmax = Math.Max(Xmax, x);
            Ymax = Math.Max(Ymax, y);
        }

        /// <summary>
        ///     Scale bounds.
        /// </summary>
        /// <param name="dx">Add dx to left and right bounds.</param>
        /// <param name="dy">Add dy to top and bottom bounds.</param>
        public void Scale(double dx, double dy)
        {
            Xmin -= dx;
            Xmax += dx;
            Ymin -= dy;
            Ymax += dy;
        }

        /// <summary>
        ///     Check if given point is inside bounding box.
        /// </summary>
        /// <param name="pt">Point to check.</param>
        /// <returns>Return true, if bounding box contains given point.</returns>
        public bool Contains(Point pt)
        {
            return pt.x >= Xmin && pt.x <= Xmax && pt.y >= Ymin && pt.y <= Ymax;
        }
    }
}