using Verse;

namespace AvaliMod
{
    public class SpawnThingProperties : CompProperties
    {
        public ThingDef thingToSpawn;
        public float daysToSpawn;

        public SpawnThingProperties()
        {
            compClass = typeof(SpawnThingComp);
        }
    }
}