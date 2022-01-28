using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using Verse;
using Random = UnityEngine.Random;

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
                    foreach (ResearchProjectDef def in DefDatabase<ResearchProjectDef>.AllDefs.Where(proj =>
                                 proj.requiredResearchFacilities.Contains(AvaliDefs.AvaliNexus)))
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
            // $"{"AirdropInStart".Translate()} {AirDropHandler.timeToDrop / AirDropHandler.ticksInAnHour} {"AirdropInEnd".Translate()}";

            defaultLabel = "IlluminateAirdropSend".Translate(AirDropHandler.GetTimeToDropInHours.Named("TIME"));

            return AirDropHandler.HasMessaged && !AirDropHandler.hasDropped ? AlertReport.Active : AlertReport.Inactive;
        }
    }

    public class AirDropHandler : WorldComponent
    {
        private const int TicksPerHour = GenDate.TicksPerHour / 100;
        public static int timeToDrop;
        private static bool hasMessaged;

        internal static bool hasDropped;

        private readonly bool airdrops =
            LoadedModManager.GetMod<RimValiMod>().GetSettings<RimValiModSettings>().enableAirdrops;

        private readonly int avaliReq = LoadedModManager.GetMod<RimValiMod>().GetSettings<RimValiModSettings>()
            .avaliRequiredForDrop;

        private Map map;

        private IntVec3 targetPos;
        private int ticks;

        public AirDropHandler(World world) : base(world)
        {
            timeToDrop = 0;
            hasMessaged = false;
            hasDropped = false;
        }

        public static int GetTimeToDropInHours => timeToDrop / TicksPerHour;
        public static bool HasMessaged => hasMessaged;

        public override void ExposeData()
        {
            Scribe_Values.Look(ref timeToDrop, "timeToDrop");
            Scribe_Values.Look(ref hasDropped, "hasDropped");
            Scribe_Values.Look(ref hasMessaged, "hasMessaged");
            Scribe_Values.Look(ref targetPos, "targetPos");
            Scribe_References.Look(ref map, "map");
            base.ExposeData();
        }

        private void SendDrop()
        {
            map = Current.Game.CurrentMap;
            GenDate.DayOfYear(ticks, Find.WorldGrid.LongLatOf(map.Tile).x);
            var thingList = new List<Thing>
            {
                ThingMaker.MakeThing(AvaliDefs.AvaliNexus),
            };

            Scribe_Values.Look(ref hasDropped, "hasDropped");

            var target = new GlobalTargetInfo(targetPos, map);

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
                ChoiceLetter choiceLetter = LetterMaker.MakeLetter("IlluminateAirdrop".Translate(),
                    "AirdropEventDesc".Translate(), AvaliDefs.IlluminateAirdrop,
                    new LookTargets { targets = new List<GlobalTargetInfo> { target } });
                Find.LetterStack.ReceiveLetter(choiceLetter);
            }
            else
            {
                hasMessaged = false;
                SetupDrop();
                ChoiceLetter choiceLetter =
                    LetterMaker.MakeLetter("Cannotsend", "test", AvaliDefs.IlluminateAirdrop);
                Find.LetterStack.ReceiveLetter(choiceLetter);
            }
        }

        private void SetupDrop()
        {
            if (!hasMessaged)
            {
                map = Current.Game.CurrentMap;
                timeToDrop = Random.Range(1 * TicksPerHour, 48 * TicksPerHour);
                targetPos = DropCellFinder.TradeDropSpot(map);
                var targ = new GlobalTargetInfo(targetPos, map);
                ChoiceLetter choiceLetter = LetterMaker.MakeLetter("AirdropSendMsg".Translate(),
                    "IlluminateAirdropSend".Translate(GetTimeToDropInHours.Named("TIME")), AvaliDefs.IlluminateAirdrop,
                    new LookTargets { targets = new List<GlobalTargetInfo> { targ } });
                Find.LetterStack.ReceiveLetter(choiceLetter);
                hasMessaged = true;
            }
        }

        private bool GetReady()
        {
            map = Current.Game.CurrentMap;
            if (RimValiCore.RimValiUtility.PawnOfRaceCount(Faction.OfPlayer, AvaliDefs.RimVali) < avaliReq ||
                hasDropped || !map.IsPlayerHome)
            {
                return false;
            }

            SetupDrop();
            return true;
        }

        public override void WorldComponentTick()
        {
            if (!airdrops)
            {
                return;
            }

            ticks++;
            if (ticks < 120)
            {
                return;
            }

            if (GetReady())
            {
                timeToDrop--;
            }

            map = Current.Game.CurrentMap;
            if (timeToDrop <= 0 &&
                RimValiCore.RimValiUtility.PawnOfRaceCount(Faction.OfPlayer, AvaliDefs.RimVali) >= avaliReq &&
                !hasDropped && map.IsPlayerHome)
            {
                SendDrop();
            }

            ticks = 0;
        }
    }
}
