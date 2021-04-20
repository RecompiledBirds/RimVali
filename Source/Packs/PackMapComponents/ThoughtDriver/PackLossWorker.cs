using System;
using Verse;
using RimWorld;
namespace AvaliMod
{
    public class PackLossThoughtWorker : ThoughtWorker
    {
        int stageOne = LoadedModManager.GetMod<RimValiMod>().GetSettings<RimValiModSettings>().stageOneDaysPackloss;
        int stageTwo = LoadedModManager.GetMod<RimValiMod>().GetSettings<RimValiModSettings>().stageTwoDaysPackloss;
        int stageThree = LoadedModManager.GetMod<RimValiMod>().GetSettings<RimValiModSettings>().stageThreeDaysPackloss;
        int dayLen = 60000;
        public void UpdatePackLoss(Pawn pawn)
        {
            AvaliThoughtDriver thoughtComp = pawn.TryGetComp<AvaliThoughtDriver>();
            PackComp packComp = pawn.TryGetComp<PackComp>();

            
            if (packComp.ticksSinceLastInpack == 0 && (pawn.GetPackWithoutSelf() == null || pawn.GetPackWithoutSelf().pawns.Count < 2))
            {
                packComp.inPack= false;
            }
            else if (pawn.GetPackWithoutSelf() != null && pawn.GetPackWithoutSelf().pawns.Count > 1)
            {
                packComp.inPack =true;
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
            int timeAlone = packComp.ticksSinceLastInpack;
            if (timeAlone == Find.TickManager.TicksGame || timeAlone == 0)
            { return ThoughtState.Inactive; }
            if (timeAlone >= dayLen*stageThree)
            {
                return ThoughtState.ActiveAtStage(2);
            }
            if (timeAlone >= dayLen*stageTwo)
            {
                return ThoughtState.ActiveAtStage(1);
            }
            if (timeAlone >= dayLen*stageOne)
            {
                return ThoughtState.ActiveAtStage(0);
            }
          
            return ThoughtState.Inactive;
        }
    }
}