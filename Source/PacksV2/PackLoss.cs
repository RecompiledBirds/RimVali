using System;
using Rimvali.Rewrite.Packs;
using RimWorld;
using Verse;

namespace AvaliMod
{
    public class PackLossThoughtWorker : ThoughtWorker
    {
        private readonly int stageOne = LoadedModManager.GetMod<RimValiMod>().GetSettings<RimValiModSettings>()
            .stageOneDaysPackloss;

        private readonly int stageThree = LoadedModManager.GetMod<RimValiMod>().GetSettings<RimValiModSettings>()
            .stageThreeDaysPackloss;

        private readonly int stageTwo = LoadedModManager.GetMod<RimValiMod>().GetSettings<RimValiModSettings>()
            .stageTwoDaysPackloss;


        public void UpdatePackLossv2(Pawn pawn, PackComp packComp, PacksV2WorldComponent packsV2WorldComponent)
        {
            if (packComp.ticksSinceLastInPack == 0 && !packsV2WorldComponent.PawnHasPackWithMembers(pawn, false))
            {
                packComp.inPack = false;
            }
            else if (pawn.IsPackBroken() || packsV2WorldComponent.PawnHasPackWithMembers(pawn, false))
            {
                packComp.inPack = true;
            }
        }
        protected override ThoughtState CurrentStateInternal(Pawn p)
        {

            if (!AvaliDefs.IsAvali(p) || !RimValiMod.settings.packLossEnabled || p.IsPackBroken())
                return ThoughtState.Inactive;
            PacksV2WorldComponent packsComp = Find.World.GetComponent<PacksV2WorldComponent>();
            var packComp = p.TryGetComp<PackComp>();
            var thoughtComp = p.TryGetComp<AvaliThoughtDriver>();

            if (packsComp.PawnHasPackWithMembers(p, false) || p.health.hediffSet.hediffs.Any(x => thoughtComp.Props.packLossPreventers.Contains(x.def)))
                return ThoughtState.Inactive;

            UpdatePackLossv2(p, packComp, packsComp);

            int timeAlone = packComp.ticksSinceLastInPack;
            if (timeAlone == Find.TickManager.TicksGame || timeAlone == 0)
            {
                return ThoughtState.Inactive;
            }

            bool stageTwoHasPassed = timeAlone >= GenDate.TicksPerDay * stageTwo;

            if (timeAlone >= GenDate.TicksPerDay * stageThree)
            {
                if (packComp.lastDay + 3 < GenDate.DaysPassed)
                {
                    if (new Random().Next(0, 100) < RimValiMod.settings.packBrokenChance && RimValiMod.settings.canGetPackBroken &&
                        !p.story.traits.HasTrait(AvaliDefs.AvaliPackBroken))
                    {
                        p.story.traits.GainTrait(new Trait(AvaliDefs.AvaliPackBroken));
                    }

                    packComp.lastDay = GenDate.DaysPassed;
                }


                return ThoughtState.ActiveAtStage(2);
            }

            if (stageTwoHasPassed)
            {
                if (packComp.lastDay + 3 >= GenDate.DaysPassed)
                {
                    return ThoughtState.ActiveAtStage(1);
                }

                if (new Random().Next(0, 100) < RimValiMod.settings.packBrokenChance && RimValiMod.settings.canGetPackBroken &&
                    !p.story.traits.HasTrait(AvaliDefs.AvaliPackBroken))
                {
                    p.story.traits.GainTrait(new Trait(AvaliDefs.AvaliPackBroken));
                }

                packComp.lastDay = GenDate.DaysPassed;

                return ThoughtState.ActiveAtStage(1);
            }

            if (timeAlone >= GenDate.TicksPerDay * stageThree)
            {
                return ThoughtState.ActiveAtStage(2);
            }

            if (stageTwoHasPassed)
            {
                return ThoughtState.ActiveAtStage(1);
            }

            if (timeAlone >= GenDate.TicksPerDay * stageOne)
            {
                return ThoughtState.ActiveAtStage(0);
            }
            return ThoughtState.Inactive;
        }
    }
}