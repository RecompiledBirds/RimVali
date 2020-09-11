using RimWorld;
using Verse;
namespace AvaliMod
{
    public class AvaliDriverThoughtProps : CompProperties
    {
        public PawnRelationDef relationDef;
        public ThoughtDef inSameRoomThought;
        public ThoughtDef sharedBedroomThought;
        public ThoughtDef sleptApartThought;

        public AvaliDriverThoughtProps()
        {
            this.compClass = typeof(AvaliThoughtDriver);
        }
    }
}