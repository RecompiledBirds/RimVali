using Verse;
namespace AvaliMod
{
    public class spawnThingComp : ThingComp
    {
        private float thingProgress;

        private thingSpawnerProps Props => (thingSpawnerProps)props;
        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look<float>(ref thingProgress, "thingProgress", 0.0f, true);
        }
        public override void CompTick()
        {
            thingProgress += (float)(1.0 / (Props.daysToSpawn * 60000.0));
            if (thingProgress <= 1.0)
            {
                return;
            }

            spawnThing(Props.thingToSpawn, parent.Position, parent.Map);
        }
        public void spawnThing(ThingDef thingDef, IntVec3 loc, Map map)
        {
            try
            {
                Thing thing = ThingMaker.MakeThing(thingDef);
                GenPlace.TryPlaceThing(thing, loc, map, ThingPlaceMode.Direct);
            }
            finally
            {
                if (!parent.Destroyed)
                {
                    parent.Destroy(DestroyMode.Vanish);
                }
            }
        }
        public override string CompInspectStringExtra()
        {
            return "Progress: " + thingProgress.ToStringPercent();
        }
    }
}