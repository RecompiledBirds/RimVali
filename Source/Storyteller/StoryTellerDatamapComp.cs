using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using RimWorld.Planet;
using Verse;
namespace AvaliMod
{
    public class NesiStorytellerData : WorldComponent
    {
        public static NesiState state = NesiState.Neutral;
        public static int daysPassedSinceLastHunt;
        public static int daysSpentNice;
        public static int dayLastUpdated;
        public override void ExposeData()
        {
            Scribe_Values.Look(ref state, "state");
            Scribe_Values.Look(ref daysPassedSinceLastHunt, "daysPassedSinceLastHunt");
            Scribe_Values.Look(ref daysSpentNice, "daysSpentNice");
            Scribe_Values.Look(ref dayLastUpdated, "dayLastUpdated");
            base.ExposeData();
        }
        public NesiStorytellerData(World world) : base(world)
        {
            state = NesiState.Neutral;
            dayLastUpdated = 0;
            daysPassedSinceLastHunt = 0;
            daysSpentNice = 0;
        }
    }
}
