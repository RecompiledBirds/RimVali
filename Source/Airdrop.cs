using RimWorld;
using System;
using System.Collections.Generic;
using Verse;
using System.Linq;
using RimWorld.Planet;

namespace AvaliMod
{
    public class FactionResearchManager
    {

    }
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
    public class AirdropAlert : Alert
    {
        public override AlertReport GetReport()
        {
            
            defaultLabel = "IlluminateAirdropSend".Translate((AirDropHandler.timeToDrop / AirDropHandler.ticksInAnHour).Named("TIME"))/* "AirdropInStart".Translate() + " " + (AirDropHandler.timeToDrop / AirDropHandler.ticksInAnHour).ToString() + " " + "AirdropInEnd".Translate()*/;
            if (AirDropHandler.hasMessaged && !AirDropHandler.hasDropped)
            {
                return AlertReport.Active;
            }
            else
            {
                return AlertReport.Inactive;
            }
        }
    }
    public class AirDropHandler : WorldComponent
    {
        private readonly bool airdrops = LoadedModManager.GetMod<RimValiMod>().GetSettings<RimValiModSettings>().enableAirdrops;
        private readonly int avaliReq = LoadedModManager.GetMod<RimValiMod>().GetSettings<RimValiModSettings>().avaliRequiredForDrop;
        private int ticks = 0;
        public static int timeToDrop;
        public static int ticksInAnHour = 25;
        public static bool hasMessaged;
        public AirDropHandler(World world) : base(world) {
            timeToDrop = 0;
            hasMessaged = false;
            hasDropped = false; 
        }

        public static bool hasDropped = false;
        
        public override void ExposeData()
        {
            Scribe_Values.Look(ref timeToDrop, "timeToDrop", 0);
            Scribe_Values.Look(ref hasDropped, "hasDropped", false);
            Scribe_Values.Look(ref hasMessaged, "hasMessaged", false);
            base.ExposeData();
        }
        private void SendDrop()
        {
            Map map = Current.Game.CurrentMap;
            GenDate.DayOfYear(ticks, Find.WorldGrid.LongLatOf(map.Tile).x);
            List<Thing> thingList = new List<Thing>
            {
                ThingMaker.MakeThing(AvaliDefs.AvaliNexus)
            };
            Scribe_Values.Look(ref hasDropped, "hasDropped", false);
            Map target = map;
            List<Faction> newFactions = new List<Faction>();
            IntVec3 intVec3 = DropCellFinder.TradeDropSpot(target);
            if (map.IsPlayerHome)
            {
                hasDropped = true;
                foreach(Faction faction in Find.FactionManager.AllFactions.Where(x => x.def == AvaliDefs.AvaliFaction))
                {
                    faction.SetRelationDirect(Faction.OfPlayer, FactionRelationKind.Ally);
                    
                    newFactions.Add(faction);
                }
                DropPodUtility.DropThingsNear(intVec3, target, (IEnumerable<Thing>)thingList);
                ChoiceLetter choiceLetter = LetterMaker.MakeLetter("IlluminateAirdrop".Translate(), "AirdropEventDesc".Translate(), AvaliMod.AvaliDefs.IlluminateAirdrop);
                Find.LetterStack.ReceiveLetter(choiceLetter, null);
            }
            else
            {
                SetupDrop();
            }
        }

        private void SetupDrop()
        {
            if (!hasMessaged)
            {
                timeToDrop = UnityEngine.Random.Range(1 * ticksInAnHour, 48 * ticksInAnHour);

                ChoiceLetter choiceLetter = LetterMaker.MakeLetter("AirdropSendMsg".Translate(), "IlluminateAirdropSend".Translate((AirDropHandler.timeToDrop / AirDropHandler.ticksInAnHour).Named("TIME")), AvaliMod.AvaliDefs.IlluminateAirdrop);
                Find.LetterStack.ReceiveLetter(choiceLetter, null);
                hasMessaged = true;
            }
        }

        private bool GetReady()
        {
            Map map = Current.Game.CurrentMap;
            if (RimValiUtility.PawnOfRaceCount(Faction.OfPlayer, AvaliDefs.RimVali) >= avaliReq && !hasDropped && map.IsPlayerHome)
            {
                if (!hasDropped)
                {
                    SetupDrop();
                    
                }
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
                    if (GetReady() && !hasDropped)
                    {
                        
                        timeToDrop--;
                    }
                    Map map = Current.Game.CurrentMap;
                    if (timeToDrop <= 0 && RimValiUtility.PawnOfRaceCount(Faction.OfPlayer, AvaliDefs.RimVali) >= avaliReq && !hasDropped && map.IsPlayerHome)
                    {
                        SendDrop();
                    }
                }
                ticks = 0;
            }
        }
    }
   
}