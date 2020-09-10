using Verse;
namespace AvaliMod
{
    public class thingSpawnerProps : CompProperties
    {
        public ThingDef thingToSpawn;
        public float daysToSpawn;

        public thingSpawnerProps()
        {
            this.compClass = typeof(spawnThingComp);
        }
    }
}