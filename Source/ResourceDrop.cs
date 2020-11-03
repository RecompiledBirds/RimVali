using RimWorld;
using System;
using System.Collections.Generic;
using Verse;
namespace AvaliMod
{
    public class IlluminateAirdrop : IncidentWorker
    {
        public bool hasDropped;
        public override void ExposeData()
        {
            Scribe_Values.Look(ref hasDropped, “hasDropped", false);
            base.ExposeData();
        }
        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            List<Thing> thingList = new List<Thing>();
            thingList.Add(ThingMaker.MakeThing(AvaliDefs.AvaliNanoForge));
            Map target = (Map)parms.target;
            IntVec3 intVec3 = DropCellFinder.TradeDropSpot(target);
            if (RimValiUtility.PawnOfRaceCount(Faction.OfPlayer, AvaliDefs.RimVali) >= 5 && !hasDropped)
            {
                hasDropped = true;
                DropPodUtility.DropThingsNear(intVec3, target, (IEnumerable<Thing>)thingList);
                this.SendStandardLetter("Illuminate Airdrop", "Illuminate Airdrop", AvaliDefs.IlluminateAirdrop,parms, (LookTargets)new TargetInfo(intVec3, target, false), (NamedArgument[])Array.Empty<NamedArgument>());
            }
            return true;
        }
    }
}
