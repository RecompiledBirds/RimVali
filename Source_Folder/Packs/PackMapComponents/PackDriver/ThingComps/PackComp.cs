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
