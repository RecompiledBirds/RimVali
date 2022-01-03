using Verse;
using RimWorld;

namespace AvaliMod
{
    public class AvaliEggLayer_Props : CompProperties
    {
        public float eggLayIntervalDays = 1f;
        public IntRange eggCountRange = IntRange.one;
        public int eggFertilizationCountMax = 1;
        public bool eggLayFemaleOnly = true;
        public ThingDef eggFertilizedDef;

        public AvaliEggLayer_Props()
        {
            this.compClass = typeof(AvaliEggLayer);
        }
    }
}