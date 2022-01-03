using System.Linq;
using RimWorld;
using Verse;

namespace AvaliMod
{
    public class DeathActionWorker_Test : DeathActionWorker
    {
        private readonly bool enableDebug =
            LoadedModManager.GetMod<RimValiMod>().GetSettings<RimValiModSettings>().enableDebugMode;

        public override void PawnDied(Corpse corpse)
        {
            if (enableDebug)
            {
                Log.Message("Death detected");
            }

            if (!corpse.InnerPawn.def.HasComp(typeof(PackComp)) || RimValiUtility.Driver.Packs == null ||
                RimValiUtility.Driver.Packs.Count <= 0)
            {
                return;
            }

            if (enableDebug)
            {
                Log.Message("I got here, packs exist.");
            }

            Pawn pawn = corpse.InnerPawn;
            AvaliPack pack = RimValiUtility.Driver.GetCurrentPack(pawn);
            if (pack == null)
            {
                return;
            }

            if (enableDebug)
            {
                Log.Message("The pawn had a pack");
            }

            var deathDate = new DeathDate(pawn)
            {
                day = GenDate.DayOfYear(1, Find.WorldGrid.LongLatOf(corpse.Map.Tile).x),
            };
            if (corpse.InnerPawn != null && enableDebug)
            {
                Log.Message("Pawn is not null");
            }

            deathDate.deadPawn = pawn;
            if (enableDebug)
            {
                Log.Message($"I got the death date '{deathDate.day}'");
            }

            pack.pawns.Remove(pawn);
            if (pawn == pack.leaderPawn && pack.pawns.Count > 0)
            {
                pack.leaderPawn = pack.pawns.First();
            }

            pack.deathDates.Add(deathDate);

            //pack.size--;
            if (enableDebug)
            {
                Log.Message(pack.GetAllNonNullPawns.Count.ToString());
            }

            foreach (Pawn packMate in pack.GetAllNonNullPawns)
            {
                Log.Message("On: " + packMate.Name);
                if (pawn == packMate || packMate.Dead)
                {
                    continue;
                }

                Log.Message(packMate.Name + " is not dead, adding thought");
                var comp = packMate.TryGetComp<PackComp>();
                if (comp.Props.deathThought != null)
                {
                    packMate.needs.mood.thoughts.memories.TryGainMemory(comp.Props.deathThought, pawn);
                }
            }
        }
    }
}
