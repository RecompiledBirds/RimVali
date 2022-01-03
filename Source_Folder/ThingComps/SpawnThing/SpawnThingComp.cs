using Verse;
namespace AvaliMod
{
    public class spawnThingComp : ThingComp
    {
        private float thingProgress;

        private thingSpawnerProps Props
        {
            get
            {
                return (thingSpawnerProps)this.props;
            }
        }
        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look<float>(ref thingProgress, "thingProgress", 0.0f, true);
        }
        public override void CompTick()
        {
            thingProgress += (float)(1.0 / ((double)this.Props.daysToSpawn * 60000.0));
            if ((double)thingProgress <= 1.0)
                return;
            spawnThing(this.Props.thingToSpawn, this.parent.Position, this.parent.Map);
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
                if (!this.parent.Destroyed)
                {
                    this.parent.Destroy(DestroyMode.Vanish);
                }
            }
        }
        public override string CompInspectStringExtra()
        {
            return (string)("Progress: " + thingProgress.ToStringPercent());
        }
    }
}