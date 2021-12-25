using RimWorld.Planet;
using Verse;

namespace AvaliMod
{
    public class StorytellerData : WorldComponent
    {
        public static StorytellerState state = StorytellerState.Neutral;
        public static int daysPassedSinceLastHunt;
        public static int daysSpentNice;
        public static int dayLastUpdated;

        public StorytellerData(World world) : base(world)
        {
            state = StorytellerState.Neutral;
            dayLastUpdated = 0;
            daysPassedSinceLastHunt = 0;
            daysSpentNice = 0;
        }

        public override void ExposeData()
        {
            if (Current.Game.storyteller.def.defName == "Nesi")
            {
                Log.Message("Can't wait to see what happens..");
            }

            Log.Message("HEY");
            Scribe_Values.Look(ref state, "state");
            Scribe_Values.Look(ref daysPassedSinceLastHunt, "daysPassedSinceLastHunt");
            Scribe_Values.Look(ref daysSpentNice, "daysSpentNice");
            Scribe_Values.Look(ref dayLastUpdated, "dayLastUpdated");
            base.ExposeData();
        }
    }
}
