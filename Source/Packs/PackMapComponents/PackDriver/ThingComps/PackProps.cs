using RimWorld;
using System.Collections.Generic;
using Verse;
namespace AvaliMod
{
    public class PackProps : CompProperties
    {
        public List<ThingDef> racesInPacks;
        public bool canHaveAloneThought;
        public ThoughtDef aloneThought;
        public PawnRelationDef relation;
        public SimpleCurve packGenChanceOverAge;
        public PackProps()
        {
            this.compClass = typeof(PackComp);
        }
    }
}