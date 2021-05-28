using System;
using Verse;
using RimWorld;
namespace AvaliMod
{
    public class Shared_workRoom : ThoughtWorker
    {
        protected override ThoughtState CurrentStateInternal(Pawn pawn)
        {
            AvaliThoughtDriver avaliThoughtDriver = pawn.TryGetComp<AvaliThoughtDriver>();
            PackComp packComp = pawn.TryGetComp<PackComp>();
            if (!(avaliThoughtDriver == null) && pawn.Awake() && pawn.CheckIfPackmatesInRoom()) { 
                return ThoughtState.ActiveDefault;
            }

            return ThoughtState.Inactive;
        }
    }
}