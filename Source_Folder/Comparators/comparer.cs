using System.Collections.Generic;
using Verse;
namespace AvaliMod
{

    public class PawnEqaulityComparer : IEqualityComparer<Pawn>
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