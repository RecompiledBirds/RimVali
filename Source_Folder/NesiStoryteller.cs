using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Verse;

namespace AvaliMod
{
    public class NesiStoryTellerProps : StorytellerCompProperties
    {
        public float onDays;

        public float offDays;

        public float minSpacingDays;

        public int minIncidents = 1;

        public int maxIncidents = 5;

        public SimpleCurve acceptFractionByDaysPassedCurve;

        public SimpleCurve acceptPercentFactorPerThreatPointsCurve;

        public SimpleCurve acceptPercentFactorPerProgressScoreCurve;
        public NesiStoryTellerProps()
        {
            compClass = typeof(NesiStoryTeller);
        }
    }


    public enum NesiState
    {
        HyperAggressive = -3,
        Aggressive = -2,
        Hunting = -1,
        Neutral = 0,
        Calm = 1,
        Friendly = 2,
    }



    public class NesiStoryTeller : StorytellerComp
    {
        protected NesiStoryTellerProps Props => (NesiStoryTellerProps)props;


        #region Incident gen
        public FiringIncident GenAgressiveIncident(IIncidentTarget targ)
        {
            List<IncidentDef> defs = new List<IncidentDef> { IncidentDefOf.ManhunterPack, IncidentDefOf.MechCluster, IncidentDefOf.RaidEnemy, IncidentDefOf.Infestation };
            defs.AddRange(DefDatabase<IncidentDef>.AllDefs.Where(x => x.category == IncidentCategoryDefOf.ThreatBig));
            IncidentDef def = new List<IncidentDef> { IncidentDefOf.ManhunterPack, IncidentDefOf.MechCluster, IncidentDefOf.RaidEnemy, IncidentDefOf.Infestation }.RandomElement();
            IncidentParms parms = GenerateParms(def.category, targ);
            IncidentParms parms2 = GenerateParms(def.category, targ);
            if (!def.Worker.CanFireNow(parms2)) { return null; }
            parms.faction = HasPawnsNotAvali ? Find.FactionManager.FirstFactionOfDef(AvaliDefs.NesiSpecOps) : parms.faction;


            parms.points *= new Random(Find.World.ConstantRandSeed).Next(1, (int)StorytellerUtilityPopulation.AdjustedPopulation * new Random(Find.World.ConstantRandSeed).Next(1, 3)) * (HasPawnsNotAvali ? ratioNonAvaliToAvali * 2 : 1);

            if (HasPawnsNotAvali && ratioNonAvaliToAvali > 5)
            {
                int multiplier = 5;
                MessageBoxResult res = System.Windows.MessageBox.Show("Are you watching? \n -Nesi", "Nesi", button: MessageBoxButton.YesNo);
                if (res == MessageBoxResult.Yes)
                {
                    System.Windows.MessageBox.Show("I hope you enjoy the show! \n -Nesi");

                }
                else
                {
                    System.Windows.MessageBox.Show("Oh.. alright. I hope you understand I don't like to be ignored. \n -Nesi");
                    multiplier = 10;
                }
                parms.points *= ratioAvaliToNonAvali * multiplier;
            }

            return new FiringIncident(def, this, parms) { parms = parms };


        }

        public FiringIncident GenHunting(IIncidentTarget targ)
        {

            List<IncidentDef> defs = new List<IncidentDef> { IncidentDefOf.Eclipse, IncidentDefOf.SolarFlare, IncidentDefOf.ToxicFallout, IncidentDefOf.ToxicFallout, AvaliDefs.VolcanicWinter, AvaliDefs.Alphabeavers, AvaliDefs.HeatWave, AvaliDefs.Flashstorm, AvaliDefs.PsychicDrone, AvaliDefs.ShortCircuit };
            defs.AddRange(DefDatabase<IncidentDef>.AllDefs.Where(x => x.category == IncidentCategoryDefOf.DiseaseHuman));
            IncidentDef def = defs.RandomElement();
            IncidentParms parms = GenerateParms(def.category, targ);

            parms.points = parms.points * new Random(Find.World.ConstantRandSeed).Next(1, (int)StorytellerUtilityPopulation.AdjustedPopulation * new Random(Find.World.ConstantRandSeed).Next(1, 3));
            IncidentParms parms2 = GenerateParms(def.category, targ);
            if (!def.Worker.CanFireNow(parms2)) { return null; }
            if (new Random(Find.World.ConstantRandSeed).Next(1, 2) == 2) { NesiStorytellerData.state = NesiState.Aggressive; }
            return new FiringIncident(def, this, parms) { parms = parms };
        }

        public FiringIncident GenNeutral(IIncidentTarget targ)
        {
            List<IncidentDef> defs = new List<IncidentDef> { AvaliDefs.HerdMigration, AvaliDefs.MeteoriteImpact, AvaliDefs.RansomDemand, AvaliDefs.RefugeePodCrash, AvaliDefs.ResourcePodCrash, AvaliDefs.SelfTame, AvaliDefs.ThrumboPasses, AvaliDefs.WildManWandersIn, AvaliDefs.AmbrosiaSprout };
            defs.AddRange(DefDatabase<IncidentDef>.AllDefs.Where(x => x.category == IncidentCategoryDefOf.Misc));
            IncidentDef def = defs.RandomElement();
            IncidentParms parms = GenerateParms(def.category, targ);
            parms.points = parms.points * new Random(Find.World.ConstantRandSeed).Next(1, (int)StorytellerUtilityPopulation.AdjustedPopulation * new Random(Find.World.ConstantRandSeed).Next(1, 3));
            IncidentParms parms2 = GenerateParms(def.category, targ);
            if (def == AvaliDefs.RefugeePodCrash || def == AvaliDefs.WildManWandersIn || new Random(Find.World.ConstantRandSeed).Next(1, 5) == 2)
            {
                parms.faction = Find.FactionManager.FirstFactionOfDef(AvaliDefs.AvaliFaction);
                parms.pawnKind = AvaliDefs.RimValiColonist;
            }
            if (!def.Worker.CanFireNow(parms2)) { return null; }
            return new FiringIncident(def, this, parms) { parms = parms };
        }

        public FiringIncident GenFriendly(IIncidentTarget targ)
        {
            List<IncidentDef> defs = new List<IncidentDef> { IncidentDefOf.TraderCaravanArrival, IncidentDefOf.TraderCaravanArrival, IncidentDefOf.TravelerGroup, IncidentDefOf.VisitorGroup, IncidentDefOf.WandererJoin };
            defs.AddRange(DefDatabase<IncidentDef>.AllDefs.Where(x => x.category == IncidentCategoryDefOf.GiveQuest || x.category == IncidentCategoryDefOf.OrbitalVisitor || x.category == IncidentCategoryDefOf.AllyAssistance));
            IncidentDef def = defs.RandomElement();
            IncidentParms parms = GenerateParms(def.category, targ);
            parms.points = parms.points * new Random(Find.World.ConstantRandSeed).Next(1, (int)StorytellerUtilityPopulation.AdjustedPopulation * new Random(Find.World.ConstantRandSeed).Next(1, 3));
            IncidentParms parms2 = GenerateParms(def.category, targ);
            if (def.category == IncidentCategoryDefOf.OrbitalVisitor || new Random(Find.World.ConstantRandSeed).Next(1, 5) == 2)
            {
                parms.faction = Find.FactionManager.FirstFactionOfDef(AvaliDefs.AvaliFaction);
                parms.pawnKind = AvaliDefs.RimValiColonist;
            }
            parms.points = ratioAvaliToNonAvali > 2 ? parms.points * (ratioAvaliToNonAvali * StorytellerUtilityPopulation.AdjustedPopulation) : parms.points;
            if (!def.Worker.CanFireNow(parms2)) { return null; }
            return new FiringIncident(def, this, parms) { parms = parms };
        }
        #endregion

        public override IEnumerable<FiringIncident> MakeIntervalIncidents(IIncidentTarget target)
        {
            float num = 1f;
            if (Props.acceptFractionByDaysPassedCurve != null) { num *= Props.acceptFractionByDaysPassedCurve.Evaluate(GenDate.DaysPassedFloat); }
            if (Props.acceptPercentFactorPerThreatPointsCurve != null) { num *= Props.acceptPercentFactorPerThreatPointsCurve.Evaluate(StorytellerUtility.DefaultThreatPointsNow(target)); }
            if (Props.acceptPercentFactorPerProgressScoreCurve != null) { num *= Props.acceptPercentFactorPerProgressScoreCurve.Evaluate(StorytellerUtility.GetProgressScore(target)); }
            int incCount = IncidentCycleUtility.IncidentCountThisInterval(target, Find.Storyteller.storytellerComps.IndexOf(this), Props.minDaysPassed, Props.onDays, Props.offDays, Props.minSpacingDays, Props.minIncidents, Props.maxIncidents, num);
            for (int i = 0; i < incCount; i++)
            {
                FiringIncident inc = null;
                switch (NesiStorytellerData.state)
                {
                    case NesiState.Aggressive:
                        inc = GenAgressiveIncident(target);
                        break;
                    case NesiState.Hunting:
                        inc = GenHunting(target);
                        break;
                    case NesiState.Neutral:
                        inc = GenNeutral(target);
                        break;
                    case NesiState.Friendly:
                        inc = GenFriendly(target);
                        break;
                    case NesiState.Calm:
                        break;

                }
                if (inc != null)
                {
                    if (new Random(Find.World.ConstantRandSeed).Next(1, 5) == 3) { UpdateState(); }
                    yield return inc;
                }
            }

            yield break;
            //return base.MakeIntervalIncidents(target);
        }

        private bool HasPawnsNotAvali => RimValiCore.RimValiUtility.AllPawnsOfFactionSpawned(Faction.OfPlayer).Any(pawn => pawn.RaceProps.Humanlike && pawn.def != AvaliDefs.RimVali || pawn.def != AvaliDefs.IWAvaliRace);

        private float ratioAvaliToNonAvali => countPawnsavali / countPawnsNotavali;

        private float ratioNonAvaliToAvali => countPawnsNotavali / countPawnsavali;

        private int countPawnsavali => RimValiCore.RimValiUtility.AllPawnsOfFactionSpawned(Faction.OfPlayer).Where(pawn => pawn.def == AvaliDefs.RimVali || pawn.def == AvaliDefs.IWAvaliRace).Count();

        private int countPawnsNotavali => RimValiCore.RimValiUtility.AllPawnsOfFactionSpawned(Faction.OfPlayer).Where(pawn => pawn.def != AvaliDefs.RimVali || pawn.def != AvaliDefs.IWAvaliRace).Count();

        private void UpdateState()
        {

            //Make sure we're not on the hunting->aggressive route, and we've given the current state time to do it's job.
            if (NesiStorytellerData.state != NesiState.Hunting && GenDate.DaysPassed > NesiStorytellerData.dayLastUpdated + 5)
            {
                if (HasPawnsNotAvali && new Random(Find.World.ConstantRandSeed).Next(0, 2) == 1 && NesiStorytellerData.state <= 0)
                {
                    NesiStorytellerData.daysPassedSinceLastHunt = GenDate.DaysPassed;
                    NesiStorytellerData.state = NesiState.Hunting;
                    NesiStorytellerData.dayLastUpdated = GenDate.DaysPassed;
                    return;
                }
                if (GenDate.DaysPassed > NesiStorytellerData.daysPassedSinceLastHunt + new Random(Find.World.ConstantRandSeed).Next(2, 4) && NesiStorytellerData.state <= 0 && new Random(Find.World.ConstantRandSeed).Next(1, 10) == 2)
                {
                    NesiStorytellerData.daysPassedSinceLastHunt = GenDate.DaysPassed;
                    NesiStorytellerData.state = NesiState.Hunting;
                    NesiStorytellerData.dayLastUpdated = GenDate.DaysPassed;
                    return;
                }
                if (GenDate.DaysPassed > NesiStorytellerData.daysSpentNice + new Random(Find.World.ConstantRandSeed).Next(0, 4) && NesiStorytellerData.state > 0)
                {
                    NesiStorytellerData.state = NesiStorytellerData.state - 1;
                    if (NesiStorytellerData.state == 0)
                    {
                        NesiStorytellerData.daysSpentNice = GenDate.DaysPassed;
                    }
                    NesiStorytellerData.dayLastUpdated = GenDate.DaysPassed;
                    return;
                }
                if (NesiStorytellerData.state == 0 && new Random(Find.World.ConstantRandSeed).Next(1, 10) == 2)
                {
                    NesiStorytellerData.state += new Random(Find.World.ConstantRandSeed).Next(-1, 2);
                    NesiStorytellerData.dayLastUpdated = GenDate.DaysPassed;
                    return;
                }
                if (NesiStorytellerData.state == (NesiState.Aggressive | NesiState.HyperAggressive) && new Random(Find.World.ConstantRandSeed).Next(1, 10) == 2)
                {
                    NesiStorytellerData.state += new Random(Find.World.ConstantRandSeed).Next(0, 1);
                    NesiStorytellerData.dayLastUpdated = GenDate.DaysPassed;
                    return;
                }

            }

        }

    }
}