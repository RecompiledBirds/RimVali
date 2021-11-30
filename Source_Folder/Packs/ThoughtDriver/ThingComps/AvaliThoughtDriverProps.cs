using RimWorld;
using System.Collections.Generic;
using Verse;

namespace AvaliMod
{
    public class AvaliDriverThoughtProps : CompProperties
    { 
        public ThoughtDef inSameRoomThought;
        public ThoughtDef sharedBedroomThought;
        public ThoughtDef sleptApartThought;
        public List<HediffDef> packLossPreventers;

        public AvaliDriverThoughtProps()
        {
            this.compClass = typeof(AvaliThoughtDriver);
        }
    }
}