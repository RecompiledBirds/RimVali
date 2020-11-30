using System;
using Verse;
using RimWorld;
namespace AvaliMod
{
    public class PackLossThoughtWorker : ThoughtWorker
    {
        public void UpdatePackLoss(Pawn pawn)
        {
            AvaliThoughtDriver thoughtComp = pawn.TryGetComp<AvaliThoughtDriver>();
            PackComp packComp = pawn.TryGetComp<PackComp>();

            
            if (packComp.ticksSinceLastInpack == 0 && (RimValiUtility.GetPackWithoutSelf(pawn) == null || RimValiUtility.GetPackWithoutSelf(pawn).size < 2))
            {
                packComp.ticksSinceLastInpack = Find.TickManager.TicksGame;
            }
            else if (RimValiUtility.GetPackWithoutSelf(pawn) != null && RimValiUtility.GetPackWithoutSelf(pawn).size > 1)
            {
                packComp.ticksSinceLastInpack = 0;
            }
        }
        protected override ThoughtState CurrentStateInternal(Pawn p)
        {
            
            PackComp packComp = p.TryGetComp<PackComp>();
            AvaliThoughtDriver thoughtComp = p.TryGetComp<AvaliThoughtDriver>();
            if(thoughtComp == null)
            {
                return ThoughtState.Inactive;
            }
            foreach (Hediff hediff in p.health.hediffSet.hediffs)
            {
                if (thoughtComp.Props.packLossPreventers.Contains(hediff.def)){
                    return ThoughtState.Inactive;
                }
            }
            if (!LoadedModManager.GetMod<RimValiMod>().GetSettings<RimValiModSettings>().packLossEnabled)
            {
                return ThoughtState.Inactive;
            }
            if (packComp == null)
            {
                return ThoughtState.Inactive;
            }
            UpdatePackLoss(p);
            int timeAlone = Find.TickManager.TicksGame - packComp.ticksSinceLastInpack;
            if (timeAlone == Find.TickManager.TicksGame)
            { return ThoughtState.Inactive; }
            if (timeAlone >= 240000)
            {
                return ThoughtState.ActiveAtStage(2);
            }
            if (timeAlone >= 120000)
            {
                return ThoughtState.ActiveAtStage(1);
            }
            if (timeAlone >= 60000)
            {
                return ThoughtState.ActiveAtStage(0);
            }
          
            return ThoughtState.Inactive;
        }
    }
}