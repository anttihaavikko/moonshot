// -----------------------------------------------------------------------
// <copyright file="BadTriangle.cs" company="">
// Original Triangle code by Jonathan Richard Shewchuk, http://www.cs.cmu.edu/~quake/triangle.html
// Triangle.NET code by Christian Woltering, http://triangle.codeplex.com/
// </copyright>
// -----------------------------------------------------------------------

namespace TriangleNet.Data
{
    /// <summary>
    ///     A queue used to store bad triangles.
    /// </summary>
    /// <remarks>
    ///     The key is the square of the cosine of the smallest angle of the triangle.
    ///     Each triangle's vertices are stored so that one can check whether a
    ///     triangle is still the same.
    /// </remarks>
    internal class BadTriangle
    {
        public static int OTID;
        public int ID;
        public double key; // cos^2 of smallest (apical) angle.
        public BadTriangle nexttriang; // Pointer to next bad triangle.

        public Otri poortri; // A skinny or too-large triangle.
        public Vertex triangorg, triangdest, triangapex; // Its three vertices.

        public BadTriangle()
        {
            ID = OTID++;
        }

        public override string ToString()
        {
            return string.Format("B-TID {0}", poortri.triangle.hash);
        }
    }
}