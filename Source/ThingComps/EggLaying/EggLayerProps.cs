using Verse;

namespace AvaliMod
{
    public class EggLayerProps : CompProperties
    {
        public IntRange eggCountRange = IntRange.one;
        public int eggFertilizationCountMax = 1;
        public ThingDef eggFertilizedDef;
        public bool eggLayFemaleOnly = true;
        public float eggLayIntervalDays = 1f;

        public EggLayerProps()
        {
            compClass = typeof(EggLayerComp);
        }
    }
}
