using RimWorld;
using System.Collections.Generic;
using Verse;
namespace AvaliMod
{
    public class PackProps : HediffCompProperties
    {
        public List<ThingDef> racesInPacks;
        public int maxMembers;
        public bool canHaveAloneThought;
        public ThoughtDef aloneThought;
        public PawnRelationDef relation;
        public SimpleCurve packGenChanceOverAge;
        public bool allowOverride = true;
        public PackProps()
        {
            this.compClass = typeof(PackMateComp);
        }
    }
}