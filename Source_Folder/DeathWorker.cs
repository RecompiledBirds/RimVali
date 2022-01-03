using System;
using Verse;
using RimWorld;
using System.Linq;

namespace AvaliMod
{
    // Token: 0x020000DC RID: 220
    public class DeathActionWorker_Test : DeathActionWorker
    {
        private readonly bool enableDebug = LoadedModManager.GetMod<RimValiMod>().GetSettings<RimValiModSettings>().enableDebugMode;
        public override void PawnDied(Corpse corpse)
        {
            if (enableDebug)
            {
                Log.Message("Death detected");
            }
            if (corpse.InnerPawn.def.HasComp(typeof(PackComp)) && !(RimValiUtility.Driver.Packs == null) && RimValiUtility.Driver.Packs.Count > 0)
            {

                    if (enableDebug)
                    {
                        Log.Message("I got here, packs exist.");
                    }
                    Pawn pawn = corpse.InnerPawn;
                    AvaliPack pack= RimValiUtility.Driver.GetCurrentPack(pawn);
                if (pack != null)
                {
                    if (enableDebug)
                    {
                        Log.Message("The pawn had a pack");
                    }
                    DeathDate deathDate = new DeathDate(pawn);
                    //Log.Message(GenDate.DayOfYear(1, Find.WorldGrid.LongLatOf(corpse.Map.Tile).x).ToString());
                    deathDate.day = GenDate.DayOfYear(1, Find.WorldGrid.LongLatOf(corpse.Map.Tile).x);
                    if (corpse.InnerPawn != null && enableDebug)
                    {
                        Log.Message("Pawn is not null");
                    }

                    deathDate.deadPawn = pawn;
                    if (enableDebug)
                    {
                        Log.Message("I got the deathdate");
                        Log.Message(deathDate.day.ToString());
                    }
                    pack.pawns.Remove(pawn);
                    if (pawn == pack.leaderPawn)
                    {
                        if (pack.pawns.Count > 0)
                        {
                            pack.leaderPawn = pack.pawns.First();
                        }
                    }
                    pack.deathDates.Add(deathDate);

                    //pack.size--;
                    if (enableDebug)
                    {
                        Log.Message(pack.GetAllNonNullPawns.Count.ToString());
                    }
                    foreach (Pawn packmate in pack.GetAllNonNullPawns)
                    {
                        Log.Message("On: " + packmate.Name.ToString());
                        if ((pawn != packmate) && !packmate.Dead)
                        {
                            Log.Message(packmate.Name.ToString() + " is not dead, adding thought");
                            PackComp comp = packmate.TryGetComp<PackComp>();
                            if (comp.Props.deathThought != null)
                            {
                                packmate.needs.mood.thoughts.memories.TryGainMemory(comp.Props.deathThought, pawn);
                            }
                        }
                    }

                }
            }
        }
    }
}