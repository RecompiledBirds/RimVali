using RimWorld;
using Verse;
using System.Collections.Generic;
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
            return ((object)obj).GetHashCode();
        }
    }
}