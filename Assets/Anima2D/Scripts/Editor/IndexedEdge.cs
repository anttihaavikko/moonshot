using System;

namespace Anima2D
{
    [Serializable]
    public struct IndexedEdge
    {
        public int index1;
        public int index2;

        public IndexedEdge(int index1, int index2)
        {
            this.index1 = index1;
            this.index2 = index2;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
                return false;

            var p = (IndexedEdge) obj;

            return index1 == p.index1 && index2 == p.index2 || index1 == p.index2 && index2 == p.index1;
        }

        public override int GetHashCode()
        {
            return index1.GetHashCode() ^ index2.GetHashCode();
        }
    }
}