using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace AvaliMod
{
    public class NesiStoryTeller : StorytellerComp
    {
        protected NesiStorytellerProps Props => (NesiStorytellerProps)props;

        private static bool HasPawnsNotAvali
        {
            get
            {
                return RimValiCore.RimValiUtility.AllPawnsOfFactionSpawned(Faction.OfPlayer)
                    .Any(pawn => pawn.RaceProps.Humanlike && pawn.def != AvaliDefs.RimVali);
            }
        }

        private static float RatioAvaliToNonAvali => (float)CountPawnsAvali / CountPawnsNotAvali;
        private static float RatioNonAvaliToAvali => (float)CountPawnsNotAvali / CountPawnsAvali;

        private static int CountPawnsAvali
        {
            get
            {
                return RimValiCore.RimValiUtility
                    .AllPawnsOfFactionSpawned(Faction.OfPlayer).Count(pawn => pawn.def == AvaliDefs.RimVali);
            }
        }

        private static int CountPawnsNotAvali
        {
            get
            {
                return RimValiCore.RimValiUtility
                    .AllPawnsOfFactionSpawned(Faction.OfPlayer).Count(pawn => pawn.def != AvaliDefs.RimVali);
            }
        }

        public override IEnumerable<FiringIncident> MakeIntervalIncidents(IIncidentTarget target)
        {
            int incidentCount = GetIncidentCount(target);

            foreach (int _ in Enumerable.Range(0,incidentCount))
            {
                FiringIncident incident = null;
                switch (StorytellerData.state)
                {
                    case StorytellerState.Aggressive:
                        incident = GenAgressiveIncident(target);
                        break;
                    case StorytellerState.Hunting:
                        incident = GenHunting(target);
                        break;
                    case StorytellerState.Neutral:
                        incident = GenNeutral(target);
                        break;
                    case StorytellerState.Friendly:
                        incident = GenFriendly(target);
                        break;
                }

                if (incident == null)
                {
                    continue;
                }

                if (new Random(Find.World.ConstantRandSeed).Next(1, 5) == 3)
                {
                    UpdateState();
                }

                yield return incident;
            }

            //return base.MakeIntervalIncidents(target);
        }

        private int GetIncidentCount(IIncidentTarget target)
        {
            float acceptFraction = GenerateAcceptFraction(target);
            return IncidentCycleUtility.IncidentCountThisInterval(
                target,
                Find.Storyteller.storytellerComps.IndexOf(this),
                Props.minDaysPassed,
                Props.onDays,
                Props.offDays,
                Props.minSpacingDays,
                Props.minIncidents,
                Props.maxIncidents,
                acceptFraction
                );
        }

        private float GenerateAcceptFraction(IIncidentTarget target)
        {
            var acceptFraction = 1f;

            if (Props.acceptFractionByDaysPassedCurve != null)
            {
                acceptFraction *= Props.acceptFractionByDaysPassedCurve.Evaluate(GenDate.DaysPassedFloat);
            }

            if (Props.acceptPercentFactorPerThreatPointsCurve != null)
            {
                acceptFraction *= Props.acceptPercentFactorPerThreatPointsCurve.Evaluate(
                    StorytellerUtility.DefaultThreatPointsNow(target));
            }

            if (Props.acceptPercentFactorPerProgressScoreCurve != null)
            {
                acceptFraction *= Props.acceptPercentFactorPerProgressScoreCurve.Evaluate(
                    StorytellerUtility.GetProgressScore(target));
            }

            return acceptFraction;
        }

        private void UpdateState()
        {
            StorytellerState currentState = StorytellerData.state;
            int currentDaysPassed = GenDate.DaysPassed;
            int daysSinceLastStateUpdate = StorytellerData.dayLastUpdated;

            //Make sure we're not on the hunting->aggressive route, and we've given the current state time to do it's job.
            if (currentState != StorytellerState.Hunting &&
                currentDaysPassed > daysSinceLastStateUpdate + 5)
            {
                int daysBeingNice = StorytellerData.daysSpentNice;
                int daysSinceLastHunt = StorytellerData.daysPassedSinceLastHunt;
                Random worldRandomGenerator = new Random(Find.World.ConstantRandSeed);

                if (HasPawnsNotAvali &&
                worldRandomGenerator.Next(0, 2) == 1 &&
                currentState <= StorytellerState.Neutral)
                {
                    UpdateDaysAndStateToHunting();
                    return;
                }

                if (currentDaysPassed > (daysSinceLastHunt +
                   worldRandomGenerator.Next(2, 4)) && 
                    currentState <= StorytellerState.Neutral &&
                    worldRandomGenerator.Next(1, 10) == 2)
                {
                    UpdateDaysAndStateToHunting();
                    return;
                }

                if (currentDaysPassed > daysBeingNice +
                    worldRandomGenerator.Next(0, 4) && 
                    currentState > StorytellerState.Neutral)
                {
                    StorytellerData.state -= 1;
                    if (StorytellerData.state == StorytellerState.Neutral)
                    {
                        StorytellerData.daysSpentNice = GenDate.DaysPassed;
                    }

                    StorytellerData.dayLastUpdated = GenDate.DaysPassed;
                    return;
                }

                if (
                currentState == StorytellerState.Neutral &&
                worldRandomGenerator.Next(1, 10) == 2)
                {
                    StorytellerData.state += worldRandomGenerator.Next(-1, 2);
                    StorytellerData.dayLastUpdated = GenDate.DaysPassed;
                    return;
                }

                // Currently checks if state is -1/Hunting
                if (currentState == StorytellerState.Hunting &&
                    worldRandomGenerator.Next(1, 10) == 2)
                {
                    StorytellerData.state += worldRandomGenerator.Next(0, 1);
                    StorytellerData.dayLastUpdated = GenDate.DaysPassed;
                }
            }
        }

        private static void UpdateDaysAndStateToHunting()
        {
            StorytellerData.daysPassedSinceLastHunt = GenDate.DaysPassed;
            StorytellerData.state = StorytellerState.Hunting;
            StorytellerData.dayLastUpdated = GenDate.DaysPassed;
        }

        #region Incident gen

        public FiringIncident GenAgressiveIncident(IIncidentTarget targ)
        {
            var defs = new List<IncidentDef>
            {
                IncidentDefOf.ManhunterPack, IncidentDefOf.MechCluster, IncidentDefOf.RaidEnemy,
                IncidentDefOf.Infestation,
            };
            defs.AddRange(DefDatabase<IncidentDef>.AllDefs.Where(x => x.category == IncidentCategoryDefOf.ThreatBig));
            IncidentDef def = new List<IncidentDef>
            {
                IncidentDefOf.ManhunterPack, IncidentDefOf.MechCluster, IncidentDefOf.RaidEnemy,
                IncidentDefOf.Infestation,
            }.RandomElement();
            IncidentParms parms = GenerateParms(def.category, targ);
            IncidentParms parms2 = GenerateParms(def.category, targ);
            if (!def.Worker.CanFireNow(parms2))
            {
                return null;
            }

            parms.faction = HasPawnsNotAvali
                ? Find.FactionManager.FirstFactionOfDef(AvaliDefs.NesiSpecOps)
                : parms.faction;


            parms.points *=
                new Random(Find.World.ConstantRandSeed).Next(1,
                    (int)StorytellerUtilityPopulation.AdjustedPopulation *
                    new Random(Find.World.ConstantRandSeed).Next(1, 3)) *
                (HasPawnsNotAvali ? RatioNonAvaliToAvali * 2 : 1);

            if (HasPawnsNotAvali && RatioNonAvaliToAvali > 6)
            {
                var multiplier = 25;

                parms.points *= RatioAvaliToNonAvali * multiplier;
            }
            else if (HasPawnsNotAvali && RatioNonAvaliToAvali > 5)
            {
                var multiplier = 15;
                parms.points *= RatioAvaliToNonAvali * multiplier;
            }

            return new FiringIncident(def, this, parms) { parms = parms };
        }

        public FiringIncident GenHunting(IIncidentTarget targ)
        {
            var defs = new List<IncidentDef>
            {
                IncidentDefOf.Eclipse, IncidentDefOf.SolarFlare, IncidentDefOf.ToxicFallout, IncidentDefOf.ToxicFallout,
                AvaliDefs.VolcanicWinter, AvaliDefs.Alphabeavers, AvaliDefs.HeatWave, AvaliDefs.Flashstorm,
                AvaliDefs.PsychicDrone, AvaliDefs.ShortCircuit,
            };
            defs.AddRange(
                DefDatabase<IncidentDef>.AllDefs.Where(x => x.category == IncidentCategoryDefOf.DiseaseHuman));
            IncidentDef def = defs.RandomElement();
            IncidentParms parms = GenerateParms(def.category, targ);

            parms.points *= new Random(Find.World.ConstantRandSeed).Next(1,
                (int)StorytellerUtilityPopulation.AdjustedPopulation *
                new Random(Find.World.ConstantRandSeed).Next(1, 3));
            IncidentParms parms2 = GenerateParms(def.category, targ);
            if (!def.Worker.CanFireNow(parms2))
            {
                return null;
            }

            if (new Random(Find.World.ConstantRandSeed).Next(1, 2) == 2)
            {
                StorytellerData.state = StorytellerState.Aggressive;
            }

            return new FiringIncident(def, this, parms) { parms = parms };
        }

        public FiringIncident GenNeutral(IIncidentTarget targ)
        {
            var defs = new List<IncidentDef>
            {
                AvaliDefs.HerdMigration, AvaliDefs.MeteoriteImpact, AvaliDefs.RansomDemand, AvaliDefs.RefugeePodCrash,
                AvaliDefs.ResourcePodCrash, AvaliDefs.SelfTame, AvaliDefs.ThrumboPasses, AvaliDefs.WildManWandersIn,
                AvaliDefs.AmbrosiaSprout,
            };
            defs.AddRange(DefDatabase<IncidentDef>.AllDefs.Where(x => x.category == IncidentCategoryDefOf.Misc));
            IncidentDef def = defs.RandomElement();
            IncidentParms parms = GenerateParms(def.category, targ);
            parms.points *= new Random(Find.World.ConstantRandSeed).Next(1,
                (int)StorytellerUtilityPopulation.AdjustedPopulation *
                new Random(Find.World.ConstantRandSeed).Next(1, 3));
            IncidentParms parms2 = GenerateParms(def.category, targ);
            if (def == AvaliDefs.RefugeePodCrash || def == AvaliDefs.WildManWandersIn ||
                new Random(Find.World.ConstantRandSeed).Next(1, 5) == 2)
            {
                parms.faction = Find.FactionManager.FirstFactionOfDef(AvaliDefs.AvaliFaction);
                parms.pawnKind = AvaliDefs.RimValiColonist;
            }

            if (!def.Worker.CanFireNow(parms2))
            {
                return null;
            }

            return new FiringIncident(def, this, parms) { parms = parms };
        }

        public FiringIncident GenFriendly(IIncidentTarget targ)
        {
            var defs = new List<IncidentDef>
            {
                IncidentDefOf.TraderCaravanArrival, IncidentDefOf.TraderCaravanArrival, IncidentDefOf.TravelerGroup,
                IncidentDefOf.VisitorGroup, IncidentDefOf.WandererJoin,
            };
            defs.AddRange(DefDatabase<IncidentDef>.AllDefs.Where(x =>
                x.category == IncidentCategoryDefOf.GiveQuest || x.category == IncidentCategoryDefOf.OrbitalVisitor ||
                x.category == IncidentCategoryDefOf.AllyAssistance));
            IncidentDef def = defs.RandomElement();
            IncidentParms parms = GenerateParms(def.category, targ);
            parms.points *= new Random(Find.World.ConstantRandSeed).Next(1,
                (int)StorytellerUtilityPopulation.AdjustedPopulation *
                new Random(Find.World.ConstantRandSeed).Next(1, 3));
            IncidentParms parms2 = GenerateParms(def.category, targ);
            if (def.category == IncidentCategoryDefOf.OrbitalVisitor ||
                new Random(Find.World.ConstantRandSeed).Next(1, 5) == 2)
            {
                parms.faction = Find.FactionManager.FirstFactionOfDef(AvaliDefs.AvaliFaction);
                parms.pawnKind = AvaliDefs.RimValiColonist;
            }

            parms.points = RatioAvaliToNonAvali > 2
                ? parms.points * (RatioAvaliToNonAvali * StorytellerUtilityPopulation.AdjustedPopulation)
                : parms.points;
            if (!def.Worker.CanFireNow(parms2))
            {
                return null;
            }

            return new FiringIncident(def, this, parms) { parms = parms };
        }

        #endregion
    }
}
