using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace AvaliMod
{
    #region settings stuff

    [StaticConstructorOnStartup]
    public static class AirdropResearchManager
    {
        static AirdropResearchManager()
        {
            if (!LoadedModManager.GetMod<RimValiMod>().GetSettings<RimValiModSettings>().enableAirdrops)
            {
                try
                {
                    foreach (ResearchProjectDef def in DefDatabase<ResearchProjectDef>.AllDefs.Where(proj => proj.requiredResearchFacilities.Contains(AvaliDefs.AvaliNexus)))
                    {
                        def.requiredResearchFacilities.Remove(AvaliDefs.AvaliNexus);
                    }
                }
                catch (Exception e)
                {
                    Log.Message(e.Message);
                }
            }
        }
    }

    #endregion settings stuff

    public class AirdropAlert : Alert
    {
        public override AlertReport GetReport()
        {
            defaultLabel = "IlluminateAirdropSend".Translate(AirDropHandler.GetTimeToDropInHours.Named("TIME"))/* "AirdropInStart".Translate() + " " + (AirDropHandler.timeToDrop / AirDropHandler.ticksInAnHour).ToString() + " " + "AirdropInEnd".Translate()*/;

            return AirDropHandler.HasMessaged && !AirDropHandler.hasDropped ? AlertReport.Active : AlertReport.Inactive;
        }
    }

    public class AirDropHandler : WorldComponent
    {
        private readonly bool airdrops = LoadedModManager.GetMod<RimValiMod>().GetSettings<RimValiModSettings>().enableAirdrops;
        private readonly int avaliReq = LoadedModManager.GetMod<RimValiMod>().GetSettings<RimValiModSettings>().avaliRequiredForDrop;
        private int ticks = 0;
        public static int timeToDrop;

        public static int GetTimeToDropInHours => timeToDrop / ticksInAnHour;

        private static readonly int ticksInAnHour = 25;
        private static bool hasMessaged;
        public static bool HasMessaged => hasMessaged;

        public AirDropHandler(World world) : base(world)
        {
            timeToDrop = 0;
            hasMessaged = false;
            hasDropped = false;
        }

        public static bool hasDropped = false;

        private IntVec3 targetPos;
        private Map map;

        public override void ExposeData()
        {
            Scribe_Values.Look(ref timeToDrop, "timeToDrop", 0);
            Scribe_Values.Look(ref hasDropped, "hasDropped", false);
            Scribe_Values.Look(ref hasMessaged, "hasMessaged", false);
            Scribe_Values.Look(ref targetPos, "targetPos");
            Scribe_References.Look(ref map, "map");
            base.ExposeData();
        }

        private void SendDrop()
        {
            map = Current.Game.CurrentMap;
            GenDate.DayOfYear(ticks, Find.WorldGrid.LongLatOf(map.Tile).x);
            List<Thing> thingList = new List<Thing>
            {
                ThingMaker.MakeThing(AvaliDefs.AvaliNexus)
            };

            Scribe_Values.Look(ref hasDropped, "hasDropped", false);

            GlobalTargetInfo targ = new GlobalTargetInfo(targetPos, map);

            if (map.IsPlayerHome)
            {
                hasDropped = true;
                //TODO: Figure out why this doesn't seem to be working.
                //Assembly outdated?
                foreach (Faction faction in Find.FactionManager.AllFactions.Where(x => x.def == AvaliDefs.AvaliFaction))
                {
                    faction.TryAffectGoodwillWith(Faction.OfPlayer, 200, true, false);
                }
                DropPodUtility.DropThingsNear(targetPos, map, thingList);
                ChoiceLetter choiceLetter = LetterMaker.MakeLetter("IlluminateAirdrop".Translate(), "AirdropEventDesc".Translate(), AvaliDefs.IlluminateAirdrop, lookTargets: new LookTargets() { targets = new List<GlobalTargetInfo>() { targ } });
                Find.LetterStack.ReceiveLetter(choiceLetter, null);
            }
            else
            {
                hasMessaged = false;
                SetupDrop();
                ChoiceLetter choiceLetter = LetterMaker.MakeLetter(def: AvaliDefs.IlluminateAirdrop, label: "Cannotsend", text: "test");
                Find.LetterStack.ReceiveLetter(choiceLetter, null);
            }
        }

        private void SetupDrop()
        {
            if (!hasMessaged)
            {
                map = Current.Game.CurrentMap;
                timeToDrop = UnityEngine.Random.Range(1 * ticksInAnHour, 48 * ticksInAnHour);
                targetPos = DropCellFinder.TradeDropSpot(map);
                GlobalTargetInfo targ = new GlobalTargetInfo(targetPos, map);
                ChoiceLetter choiceLetter = LetterMaker.MakeLetter("AirdropSendMsg".Translate(), "IlluminateAirdropSend".Translate((GetTimeToDropInHours).Named("TIME")), AvaliDefs.IlluminateAirdrop, lookTargets: new LookTargets() { targets = new List<GlobalTargetInfo>() { targ } });
                Find.LetterStack.ReceiveLetter(choiceLetter, null);
                hasMessaged = true;
            }
        }

        private bool GetReady()
        {
            map = Current.Game.CurrentMap;
            if (RimValiCore.RimValiUtility.PawnOfRaceCount(Faction.OfPlayer, AvaliDefs.RimVali) >= avaliReq && !hasDropped && map.IsPlayerHome)
            {
                SetupDrop();
                return true;
            }
            return false;
        }

        public override void WorldComponentTick()
        {
            ticks++;
            if (ticks == 120)
            {
                if (airdrops)
                {
                    if (GetReady())
                    {
                        timeToDrop--;
                    }
                    map = Current.Game.CurrentMap;
                    if (timeToDrop <= 0 && RimValiCore.RimValiUtility.PawnOfRaceCount(Faction.OfPlayer, AvaliDefs.RimVali) >= avaliReq && !hasDropped && map.IsPlayerHome)
                    {
                        SendDrop();
                    }
                }
                ticks = 0;
            }
        }
    }
}