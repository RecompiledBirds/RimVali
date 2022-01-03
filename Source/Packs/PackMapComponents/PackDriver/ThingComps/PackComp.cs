using Verse;

namespace AvaliMod
{
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
