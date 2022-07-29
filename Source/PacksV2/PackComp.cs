using RimWorld;
using System.Collections.Generic;
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

    public class PackComp : ThingComp
    {
        public bool inPack = false;
        public int lastDay;
        public int ticksSinceLastInPack;
        public int timeAlone;

        public PackProps Props => (PackProps)props;

        public override void CompTick()
        {
            if (!inPack)
            {
                ticksSinceLastInPack++;
            }
            else if (ticksSinceLastInPack > 0)
            {
                ticksSinceLastInPack--;
            }
        }

        public override void PostExposeData()
        {
            Scribe_Values.Look(ref timeAlone, "timeAlone");
            Scribe_Values.Look(ref ticksSinceLastInPack, "ticksSinceLastInPack");
            Scribe_Values.Look(ref lastDay, "lastDay");
            base.PostExposeData();
        }
    }
}