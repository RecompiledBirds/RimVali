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

        public void UpdatePackLoss(Pawn pawn)
        {
            var packComp = pawn.TryGetComp<PackComp>();

            if (packComp.ticksSinceLastInPack == 0 && (pawn.GetPackWithoutSelf() == null ||
                                                       pawn.GetPackWithoutSelf().GetAllNonNullPawns
                                                           .EnumerableNullOrEmpty() || pawn.GetPackWithoutSelf()
                                                           .GetAllNonNullPawns.Count == 0))
            {
                packComp.inPack = false;
            }
            else if (pawn.story.traits.HasTrait(AvaliDefs.AvaliPackBroken) || pawn.GetPackWithoutSelf() != null &&
                     pawn.GetPackWithoutSelf().GetAllNonNullPawns.Count > 1)
            {
                packComp.inPack = true;
            }
        }
        public void UpdatePackLossv2(Pawn pawn, PackComp packComp, PacksV2WorldComponent packsV2WorldComponent)
        {
            Pack pack = packsV2WorldComponent.GetPack(pawn);
            if (packComp.ticksSinceLastInPack == 0 && (pack == null || pack.GetAllPawns.Count > 1))
            {
                packComp.inPack = false;
            }
            else if (pawn.IsPackBroken() || pack != null && pack.GetAllPawns.Count > 1)
            {
                packComp.inPack = true;
            }
        }
        protected override ThoughtState CurrentStateInternal(Pawn p)
        {
            if (!RimValiMod.settings.unstable)
            {
                if (RimValiUtility.Driver == null || !RimValiUtility.Driver.PawnHasPack(p))
                {
                    return ThoughtState.Inactive;
                }

                var packComp = p.TryGetComp<PackComp>();
                var thoughtComp = p.TryGetComp<AvaliThoughtDriver>();

                if (thoughtComp == null || packComp == null ||
                    p.health.hediffSet.hediffs.Any(x => thoughtComp.Props.packLossPreventers.Contains(x.def)) ||
                    !LoadedModManager.GetMod<RimValiMod>().GetSettings<RimValiModSettings>().packLossEnabled)
                {
                    return ThoughtState.Inactive;
                }

                UpdatePackLoss(p);

                int timeAlone = packComp.ticksSinceLastInPack;
                if (timeAlone == Find.TickManager.TicksGame || timeAlone == 0)
                {
                    return ThoughtState.Inactive;
                }

                if (timeAlone >= GenDate.TicksPerDay * stageThree)
                {
                    if (packComp.lastDay + 3 < GenDate.DaysPassed)
                    {
                        if (new Random().Next(0, 100) < RimValiMod.settings.packBrokenChance *
                            (RimValiUtility.Driver.GetPackCount(p) / 2) && RimValiMod.settings.canGetPackBroken &&
                            !p.story.traits.HasTrait(AvaliDefs.AvaliPackBroken))
                        {
                            p.story.traits.GainTrait(new Trait(AvaliDefs.AvaliPackBroken));
                        }

                        packComp.lastDay = GenDate.DaysPassed;
                    }


                    return ThoughtState.ActiveAtStage(2);
                }

                if (timeAlone >= GenDate.TicksPerDay * stageTwo)
                {
                    if (packComp.lastDay + 3 >= GenDate.DaysPassed)
                    {
                        return ThoughtState.ActiveAtStage(1);
                    }

                    if (new Random().Next(0, 100) < RimValiMod.settings.packBrokenChance *
                        (RimValiUtility.Driver.GetPackCount(p) / 3) && RimValiMod.settings.canGetPackBroken &&
                        !p.story.traits.HasTrait(AvaliDefs.AvaliPackBroken))
                    {
                        p.story.traits.GainTrait(new Trait(AvaliDefs.AvaliPackBroken));
                    }

                    packComp.lastDay = GenDate.DaysPassed;

                    return ThoughtState.ActiveAtStage(1);
                }

                if (timeAlone >= GenDate.TicksPerDay * stageOne)
                {
                    return ThoughtState.ActiveAtStage(0);
                }

                return ThoughtState.Inactive;
            }
            else
            {
                PacksV2WorldComponent packsComp = Find.World.GetComponent<PacksV2WorldComponent>();
                var packComp = p.TryGetComp<PackComp>();
                var thoughtComp = p.TryGetComp<AvaliThoughtDriver>();

                if (packsComp.PawnHasPackWithMembers(p) || !RimValiMod.settings.packLossEnabled || p.health.hediffSet.hediffs.Any(x => thoughtComp.Props.packLossPreventers.Contains(x.def)))
                    return ThoughtState.Inactive;

                UpdatePackLossv2(p, packComp, packsComp);

                int timeAlone = packComp.ticksSinceLastInPack;
                if (timeAlone == Find.TickManager.TicksGame || timeAlone == 0)
                {
                    return ThoughtState.Inactive;
                }

                bool stageTwoHasPassed = timeAlone >= GenDate.TicksPerDay * stageTwo;

                if (stageTwoHasPassed)
                {
                    if (new Random().Next(0, 100) < RimValiMod.settings.packBrokenChance && RimValiMod.settings.canGetPackBroken && !p.IsPackBroken())
                    {
                        p.story.traits.GainTrait(new Trait(AvaliDefs.AvaliPackBroken));
                    }
                    packComp.lastDay = GenDate.DaysPassed;
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
            }
            return ThoughtState.Inactive;
        }
    }
}
