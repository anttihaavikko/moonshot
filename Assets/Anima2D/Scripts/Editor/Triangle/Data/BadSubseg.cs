﻿// -----------------------------------------------------------------------
// <copyright file="BadSubseg.cs" company="">
// Original Triangle code by Jonathan Richard Shewchuk, http://www.cs.cmu.edu/~quake/triangle.html
// Triangle.NET code by Christian Woltering, http://triangle.codeplex.com/
// </copyright>
// -----------------------------------------------------------------------

namespace TriangleNet.Data
{
    /// <summary>
    ///     A queue used to store encroached subsegments.
    /// </summary>
    /// <remarks>
    ///     Each subsegment's vertices are stored so that we can check whether a
    ///     subsegment is still the same.
    /// </remarks>
    internal class BadSubseg
    {
        private static int hashSeed;

        public Osub encsubseg; // An encroached subsegment.
        internal int Hash;
        public Vertex subsegorg, subsegdest; // Its two vertices.

        public BadSubseg()
        {
            Hash = hashSeed++;
        }

        public override int GetHashCode()
        {
            return Hash;
        }

        public override string ToString()
        {
            return string.Format("B-SID {0}", encsubseg.seg.hash);
        }
    }
}