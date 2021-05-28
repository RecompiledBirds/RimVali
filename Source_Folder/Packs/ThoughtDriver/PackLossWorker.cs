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
            AvaliPackDriver driver = Current.Game.GetComponent<AvaliPackDriver>();
            
            if (packComp.ticksSinceLastInpack == 0 && (pawn.GetPackWithoutSelf() == null || pawn.GetPackWithoutSelf().pawns.Count == 0))
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

            if (p.story.traits.HasTrait(AvaliDefs.AvaliPackBroken)|| thoughtComp == null||packComp == null|| p.health.hediffSet.hediffs.Any(x => thoughtComp.Props.packLossPreventers.Contains(x.def)) || !LoadedModManager.GetMod<RimValiMod>().GetSettings<RimValiModSettings>().packLossEnabled)
            {
                return ThoughtState.Inactive; 
            }
            UpdatePackLoss(p);
            
            int timeAlone = packComp.ticksSinceLastInpack;
            if (timeAlone == Find.TickManager.TicksGame || timeAlone == 0)
            { return ThoughtState.Inactive; }
            if (timeAlone >= dayLen*stageThree)
            {
                if (packComp.lastDay+3 < GenDate.DaysPassed)
                {
                    if (new Random().Next(0, 100) < RimValiMod.settings.packBrokenChance && RimValiMod.settings.canGetPackBroken && !p.story.traits.HasTrait(AvaliDefs.AvaliPackBroken))
                    {
                        p.story.traits.GainTrait(new Trait(AvaliDefs.AvaliPackBroken));
                    }
                    packComp.lastDay = GenDate.DaysPassed;
                }
              

                return ThoughtState.ActiveAtStage(2);
            }
            if (timeAlone >= dayLen*stageTwo)
            {

                if (packComp.lastDay + 3 < GenDate.DaysPassed)
                {
                    if (new Random().Next(0, 100) < RimValiMod.settings.packBrokenChance && RimValiMod.settings.canGetPackBroken && !p.story.traits.HasTrait(AvaliDefs.AvaliPackBroken))
                    {
                        p.story.traits.GainTrait(new Trait(AvaliDefs.AvaliPackBroken));
                    }
                    packComp.lastDay = GenDate.DaysPassed;
                }
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