using RimWorld;
using System;
using System.Collections.Generic;
using Verse;
namespace AvaliMod
{
    public class AirDropHandler : MapComponent
    {
        private int ticks = 0;
        public AirDropHandler(Map map)
            : base(map)
        {

        }

        public bool hasDropped;

        public override void ExposeData()
        {
            Scribe_Values.Look(ref hasDropped, "hasDropped", false);
            base.ExposeData();
        }
        private void SendDrop()
        {
            List<Thing> thingList = new List<Thing>();
            thingList.Add(ThingMaker.MakeThing(AvaliDefs.AvaliNanoForge));
            Scribe_Values.Look(ref hasDropped, "hasDropped", false);
            Map target = map;
            IntVec3 intVec3 = DropCellFinder.TradeDropSpot(target);
            if (RimValiUtility.PawnOfRaceCount(Faction.OfPlayer, AvaliDefs.RimVali) >= 5 && !hasDropped)
            {
                hasDropped = true;
                DropPodUtility.DropThingsNear(intVec3, target, (IEnumerable<Thing>)thingList);
       
            }
        }
        public override void MapComponentTick()
        {

            if (ticks == 120)
            {
                SendDrop();
                ticks = 0;
            }
        }
    }
}