using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Rimvali.Rewrite.Packs;
using RimValiCore;
using RimValiCore.RVR;
using RimWorld;
using Verse;
using Enumerable = System.Linq.Enumerable;

namespace AvaliMod
{
    public static class RimValiUtility
    {
        public static string build = "Eisu 1.3.0";

        public static IEnumerable<ModuleDef> foundModules = new List<ModuleDef>();

        public static string FoundModulesString =>
            "RimValiModules".TranslateSimple() + "\n" +
            string.Join("\n", Enumerable.Select(foundModules, module => module.name));

        public static void LogAnaylitics(object message,bool otherEvaluation = true)
        {
            if (RimValiMod.settings.advancedAnaylitics && otherEvaluation)
            {
                Log.Message($"[RIMVALI ANAYLITICS]: {message}");
                StackTrace trace = new StackTrace();
                string tracedFrames="";

                for(int a = 1; a<trace.GetFrames().Length-1&& a<8; a++)
                {
                    StackFrame frame = trace.GetFrame(a);
                    tracedFrames += ($"At  MethodName: {frame.GetMethod()}, Line: {frame.GetFileLineNumber()}\n");
                }

                Log.Message($"[RVA Trace]:{tracedFrames}");
            }
            
        }
        public static HashSet<Pawn> PawnsInWorld
        {
            get
            {
                return Enumerable.ToHashSet(Enumerable.Where(
                    RimValiCore.RimValiUtility.AllPawnsOfRaceInWorld(RimValiDefChecks.PotentialPackRaces),
                    x => !x.story.traits.HasTrait(AvaliDefs.AvaliPackBroken) && x.Spawned));
            }
        }

        
        public static bool IsPackBroken(this Pawn pawn)
        {
            return pawn.story.traits.HasTrait(AvaliDefs.AvaliPackBroken);
        }

        public static bool PackMatesInRoom(this Pawn pawn)
        {
            Pack pack = Find.World.GetComponent<PacksV2WorldComponent>().GetPack(pawn);
            if (pawn == null)
                return false;
            return pack.GetAllPawns.Any(x => x != pawn && x.GetRoom() == pawn.GetRoom())&&!pawn.GetRoom().UsesOutdoorTemperature;
        }



        [DebugAction("RimVali", allowedGameStates = AllowedGameStates.PlayingOnMap)]
        public static void SubtractAirdropTime()
        {
            AirDropHandler.timeToDrop -= 10 * 25;
        }

        [DebugAction("RimVali", "Airdrop now", allowedGameStates = AllowedGameStates.PlayingOnMap)]
        public static void DropNow()
        {
            AirDropHandler.timeToDrop = 0;
        }


    }
}
