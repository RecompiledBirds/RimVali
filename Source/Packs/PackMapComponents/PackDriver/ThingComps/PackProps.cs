using System.Collections.Generic;
using RimWorld;
using Verse;

namespace AvaliMod
{
    public class PackProps : CompProperties
    {
        public ThoughtDef aloneThought;
        public bool canHaveAloneThought;
        public ThoughtDef deathThought = null;
        public SimpleCurve packGenChanceOverAge;
        public int packLossStages;
        public List<ThingDef> racesInPacks;
        public ThoughtDef togetherThought;

        public PackProps()
        {
            compClass = typeof(PackComp);
        }
    }
}
