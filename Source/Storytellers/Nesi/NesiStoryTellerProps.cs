using RimWorld;
using Verse;

namespace AvaliMod
{
    public class NesiStoryTellerProps : StorytellerCompProperties
    {
        public SimpleCurve acceptFractionByDaysPassedCurve;

        public SimpleCurve acceptPercentFactorPerProgressScoreCurve;

        public SimpleCurve acceptPercentFactorPerThreatPointsCurve;

        public int maxIncidents = 5;

        public int minIncidents = 1;

        public float minSpacingDays;

        public float offDays;
        public float onDays;

        public NesiStoryTellerProps()
        {
            compClass = typeof(NesiStoryTeller);
        }
    }
}
