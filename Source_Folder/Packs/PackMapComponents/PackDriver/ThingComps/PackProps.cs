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
        public ThoughtDef deathThought = null;
        public SimpleCurve packGenChanceOverAge;
        public ThoughtDef togetherThought;
        public int packLossStages;

        public PackProps()
        {
            compClass = typeof(PackComp);
        }
    }
}