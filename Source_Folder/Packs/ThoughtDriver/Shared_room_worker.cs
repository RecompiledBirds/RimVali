using RimWorld;
using Verse;
namespace AvaliMod
{
    public class Shared_workRoom : ThoughtWorker
    {
        protected override ThoughtState CurrentStateInternal(Pawn pawn)
        {
            AvaliThoughtDriver avaliThoughtDriver = pawn.TryGetComp<AvaliThoughtDriver>();
            if (RimValiUtility.Driver != null && RimValiUtility.Driver.HasPack(pawn) && !(avaliThoughtDriver == null) && pawn.Awake() && pawn.CheckIfPackmatesInRoom())
            {
                return ThoughtState.ActiveDefault;
            }

            return ThoughtState.Inactive;
        }
    }
}