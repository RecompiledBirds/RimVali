using Verse;

namespace AvaliMod
{
    public class EggLayerProps : CompProperties
    {
        public ThingDef eggFertilizedDef;
        public EggLayerProps()
        {
            compClass = typeof(EggLayerComp);
        }
    }
}
