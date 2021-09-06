using System.Collections.Generic;
using Verse;

namespace AvaliMod
{
    public class PawnEqualityComparer : IEqualityComparer<Pawn>
    {
        public bool Equals(Pawn x, Pawn y)
        {
            if (!(x == y))
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public int GetHashCode(Pawn obj)
        {
            return obj.GetHashCode();
        }
    }
}