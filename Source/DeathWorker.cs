using System.Linq;
using RimWorld;
using Verse;
using Rimvali.Rewrite.Packs;
namespace AvaliMod
{
    public class DeathActionWorker_Test : DeathActionWorker
    {
        public override void PawnDied(Corpse corpse)
        {

            PacksV2WorldComponent packsV2WorldComponent = Find.World.GetComponent<PacksV2WorldComponent>();

            if(!corpse.InnerPawn.def.HasComp(typeof(PackComp)) || packsV2WorldComponent.PacksReadOnly.NullOrEmpty())
            {
                return;
            }


            Pawn pawn = corpse.InnerPawn;


            Pack pack = packsV2WorldComponent.GetPack(pawn);

            if (pack == null)
            {
                return;
            }


            var deathDate = new DeathDate(pawn)
            {
                day = GenDate.DayOfYear(1, Find.WorldGrid.LongLatOf(corpse.Map.Tile).x),
            };

            deathDate.deadPawn = pawn;

            pack.RemovePawn(pawn);

            foreach (Pawn packMate in pack.GetAllPawns)
            {
                if (packMate.Dead || !packMate.Spawned)
                {
                    continue;
                }
                var comp = packMate.TryGetComp<PackComp>();
                if (comp.Props.deathThought != null)
                {
                    packMate.needs.mood.thoughts.memories.TryGainMemory(comp.Props.deathThought, pawn);
                }
            }
        }
    }
}
