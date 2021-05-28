using RimWorld;
using Verse;
using System.Linq;
using System.Collections.Generic;
namespace AvaliMod
{
    public class PackComp : ThingComp
    {
        public PackProps Props
        {
            get
            {
                return (PackProps)this.props;
            }
        }
        private ThoughtDef deathThought
        {
            get
            {
                return Props.deathThought;
            }
        }
        private ThoughtDef togetherThought
        {
            get
            {
                return Props.togetherThought;
            }
        }

        private int packLossStages
        {
            get
            {
                return Props.packLossStages;
            }
        }
        private List<ThingDef> RacesInPacks
        {
            get
            {
                return Props.racesInPacks;
            }
        }

        //can they have a thought for being alone? (in xml)
        private bool CanHaveAloneThought
        {
            get
            {
                return Props.canHaveAloneThought;
            }
        }
        //what is the thought referred to as? (xml)
        private ThoughtDef AloneThought
        {
            get
            {
                return Props.aloneThought;
            }
        }

        //A curve of age gen over time. (xml)
        private SimpleCurve AgeCurve
        {
            get
            {
                return Props.packGenChanceOverAge;
            }
        }

        //Pull a list from the races rimvali found during loading
        private List<ThingDef> OtherRaces
        {
            get
            {
                return RimValiDefChecks.potentialPackRaces.ToList<ThingDef>();
            }
        }
        public bool inPack = false;
        public int ticksSinceLastInpack = 0;
        public int timeAlone;
        public int ticks;
        public int lastDay;
        public override void CompTick()
        {
            if (!inPack)
            {
                ticksSinceLastInpack++;
            }
            else if(ticksSinceLastInpack>0)
            {
                ticksSinceLastInpack--;
            }
        }
        public override void PostExposeData()
        {
            Scribe_Values.Look(ref timeAlone, "timeAlone", 0);
            Scribe_Values.Look(ref ticksSinceLastInpack, "ticksSinceLastInPack", 0);
            Scribe_Values.Look(ref lastDay, "lastDay", 0);
            base.PostExposeData();
        }
    }
}