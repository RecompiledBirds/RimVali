using Rimvali.Rewrite.Packs;
using RimWorld;
using Verse;

namespace AvaliMod
{
    public class SharedRoomWorker : ThoughtWorker
    {
        protected override ThoughtState CurrentStateInternal(Pawn pawn)
        {
            if (!PacksV2WorldComponent.EnhancedMode)
            {
                var avaliThoughtDriver = pawn.TryGetComp<AvaliThoughtDriver>();
                if (RimValiUtility.Driver != null && RimValiUtility.Driver.HasPack(pawn) && avaliThoughtDriver != null &&
                    pawn.Awake() && pawn.CheckIfPackmatesInRoom())
                {
                    return ThoughtState.ActiveDefault;
                }
            }
            else
            {
                var avaliThoughtDriver = pawn.TryGetComp<AvaliThoughtDriver>();
                PacksV2WorldComponent packsComp = Find.World.GetComponent<PacksV2WorldComponent>();
                Pack pack = packsComp.GetPack(pawn);

                if (pack != null && avaliThoughtDriver != null && pawn.Awake() && pack.CheckIfPackmatesInRoom(pawn))
                {
                    return ThoughtState.ActiveDefault;
                }

            }
            return ThoughtState.Inactive;
        }
    }
}
