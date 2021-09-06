using RimWorld;
using Verse;

namespace AvaliMod
{
    public class SharedRoomWorker : ThoughtWorker
    {
        protected override ThoughtState CurrentStateInternal(Pawn pawn)
        {
            AvaliThoughtDriver avaliThoughtDriver = pawn.TryGetComp<AvaliThoughtDriver>();
            if ((RimValiUtility.Driver?.HasPack(pawn) == true) && avaliThoughtDriver != null && pawn.Awake() && pawn.CheckIfPackmatesInRoom())
            {
                return ThoughtState.ActiveDefault;
            }

            return ThoughtState.Inactive;
        }
    }
}