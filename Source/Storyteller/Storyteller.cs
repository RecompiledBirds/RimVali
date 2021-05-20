using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace AvaliMod
{
    
    public class NesiStoryTellerProps : StorytellerCompProperties
    {
        public float onDays;

        // Token: 0x040043AB RID: 17323
        public float offDays;

        // Token: 0x040043AC RID: 17324
        public float minSpacingDays;

        // Token: 0x040043AD RID: 17325


        // Token: 0x040043AE RID: 17326
        public SimpleCurve acceptFractionByDaysPassedCurve;

        // Token: 0x040043AF RID: 17327
        public SimpleCurve acceptPercentFactorPerThreatPointsCurve;

        // Token: 0x040043B0 RID: 17328
        public SimpleCurve acceptPercentFactorPerProgressScoreCurve;
        public NesiStoryTellerProps()
        {
            this.compClass = typeof(NesiStoryTeller);
        }
    }


    public enum NesiState
    {
        Aggressive = -2,
        Hunting = -1,
        Neutral = 0,
        Calm = 1,
        Friendly = 2,
    }



    public class NesiStoryTeller : StorytellerComp
    {
        protected NesiStoryTellerProps Props
        {
            get
            {
                return (NesiStoryTellerProps)this.props;
            }
        }
      
        public FiringIncident GenAgressiveIncident(IIncidentTarget targ)
        {
            List<IncidentDef> defs = new List<IncidentDef> { IncidentDefOf.ManhunterPack, IncidentDefOf.MechCluster, IncidentDefOf.RaidEnemy, IncidentDefOf.Infestation };
            defs.AddRange(DefDatabase<IncidentDef>.AllDefs.Where(x => x.category == IncidentCategoryDefOf.ThreatBig));
            IncidentDef def = new List<IncidentDef> { IncidentDefOf.ManhunterPack, IncidentDefOf.MechCluster, IncidentDefOf.RaidEnemy, IncidentDefOf.Infestation}.RandomElement();
            IncidentParms parms = this.GenerateParms(def.category, targ);
            IncidentParms parms2 = this.GenerateParms(def.category, targ);
            if (!def.Worker.CanFireNow(parms2))
            {
                return null;
            }
            parms.points = parms.points * new Random(Find.World.ConstantRandSeed).Next(1, (int)StorytellerUtilityPopulation.AdjustedPopulation* new Random(Find.World.ConstantRandSeed).Next(1,3));
            return new FiringIncident(def, this, parms) { parms=parms};


        }

        public FiringIncident GenHunting(IIncidentTarget targ)
        {
            List<IncidentDef> defs = new List<IncidentDef> { IncidentDefOf.Eclipse, IncidentDefOf.SolarFlare, IncidentDefOf.ToxicFallout, IncidentDefOf.ToxicFallout, AvaliDefs.VolcanicWinter, AvaliDefs.Alphabeavers, AvaliDefs.HeatWave, AvaliDefs.Flashstorm, AvaliDefs.PsychicDrone, AvaliDefs.ShortCircuit };
            defs.AddRange(DefDatabase<IncidentDef>.AllDefs.Where(x => x.category == IncidentCategoryDefOf.DiseaseHuman|| x.category == IncidentCategoryDefOf.DiseaseHuman));
            IncidentDef def = defs.RandomElement();
            IncidentParms parms = this.GenerateParms(def.category, targ);
            
            parms.points = parms.points * new Random(Find.World.ConstantRandSeed).Next(1, (int)StorytellerUtilityPopulation.AdjustedPopulation * new Random(Find.World.ConstantRandSeed).Next(1,3));
            IncidentParms parms2 = this.GenerateParms(def.category, targ);
            if (!def.Worker.CanFireNow(parms2))
            {
                return null;
            }
            if (new Random(Find.World.ConstantRandSeed).Next(1, 2) == 2)
            {
                NesiStorytellerData.state = NesiState.Aggressive;
            }
            return new FiringIncident(def, this, parms) { parms = parms };


        }
        public FiringIncident GenNeutral(IIncidentTarget targ)
        {
            List<IncidentDef> defs = new List<IncidentDef> { AvaliDefs.HerdMigration, AvaliDefs.MeteoriteImpact, AvaliDefs.RansomDemand, AvaliDefs.RefugeePodCrash, AvaliDefs.ResourcePodCrash, AvaliDefs.SelfTame, AvaliDefs.ThrumboPasses, AvaliDefs.WildManWandersIn, AvaliDefs.AmbrosiaSprout };
            defs.AddRange(DefDatabase<IncidentDef>.AllDefs.Where(x => x.category == IncidentCategoryDefOf.Misc));
            IncidentDef def = defs.RandomElement();
            IncidentParms parms = this.GenerateParms(def.category, targ);
            parms.points = parms.points * new Random(Find.World.ConstantRandSeed).Next(1, (int)StorytellerUtilityPopulation.AdjustedPopulation * new Random(Find.World.ConstantRandSeed).Next(1,3));
            IncidentParms parms2 = this.GenerateParms(def.category, targ);
            if (!def.Worker.CanFireNow(parms2))
            {
                return null;
            }
            return new FiringIncident(def, this, parms) { parms = parms };
        }

        public FiringIncident GenFriendly(IIncidentTarget targ)
        {
            List<IncidentDef> defs = new List<IncidentDef> { IncidentDefOf.TraderCaravanArrival, IncidentDefOf.TraderCaravanArrival, IncidentDefOf.TravelerGroup, IncidentDefOf.VisitorGroup, IncidentDefOf.WandererJoin };
            defs.AddRange(DefDatabase<IncidentDef>.AllDefs.Where(x => x.category == IncidentCategoryDefOf.GiveQuest || x.category == IncidentCategoryDefOf.OrbitalVisitor||x.category==IncidentCategoryDefOf.AllyAssistance));
            IncidentDef def = defs.RandomElement();
            IncidentParms parms = this.GenerateParms(def.category, targ);
            parms.points = parms.points * new Random(Find.World.ConstantRandSeed).Next(1, (int)StorytellerUtilityPopulation.AdjustedPopulation * new Random(Find.World.ConstantRandSeed).Next(1, 3));
            IncidentParms parms2 = this.GenerateParms(def.category, targ);
            if (!def.Worker.CanFireNow(parms2))
            {
                return null;
            }
            return new FiringIncident(def, this, parms) { parms = parms };
        }

        public override IEnumerable<FiringIncident> MakeIntervalIncidents(IIncidentTarget target)
        {
            float num = 1f;
            if (this.Props.acceptFractionByDaysPassedCurve != null)
            {
                num *= this.Props.acceptFractionByDaysPassedCurve.Evaluate(GenDate.DaysPassedFloat);
            }
            if (this.Props.acceptPercentFactorPerThreatPointsCurve != null)
            {
                num *= this.Props.acceptPercentFactorPerThreatPointsCurve.Evaluate(StorytellerUtility.DefaultThreatPointsNow(target));
            }
            if (this.Props.acceptPercentFactorPerProgressScoreCurve != null)
            {
                num *= this.Props.acceptPercentFactorPerProgressScoreCurve.Evaluate(StorytellerUtility.GetProgressScore(target));
            }
            Random rand = new Random(Find.World.ConstantRandSeed);
            int min = rand.Next(1, 4);
            int max = rand.Next(min, min + 4);
            UpdateState();
            int incCount = IncidentCycleUtility.IncidentCountThisInterval(target, Find.Storyteller.storytellerComps.IndexOf(this), this.Props.minDaysPassed, this.Props.onDays, this.Props.offDays, this.Props.minSpacingDays,min, max, num);
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
                    yield return inc;
                }
            }
            if(new Random(Find.World.ConstantRandSeed).Next(1,12) == 6)
            {
                UpdateState();
            }
            yield break;
            //return base.MakeIntervalIncidents(target);
        }


        void UpdateState()
        {
            //Make sure we're not on the hunting->aggressive route, and we've given the current state time to do it's job.
            if (NesiStorytellerData.state != NesiState.Hunting && GenDate.DaysPassed > NesiStorytellerData.dayLastUpdated + 5)
            {
                if(GenDate.DaysPassed> NesiStorytellerData.daysPassedSinceLastHunt + new Random(Find.World.ConstantRandSeed).Next(2,4)&& NesiStorytellerData.state<= 0&& new Random(Find.World.ConstantRandSeed).Next(1, 10)==2)
                {
                    NesiStorytellerData.daysPassedSinceLastHunt = GenDate.DaysPassed;
                    NesiStorytellerData.state = NesiState.Hunting;
                    NesiStorytellerData.dayLastUpdated = GenDate.DaysPassed;
                    return;
                }
                if(GenDate.DaysPassed> NesiStorytellerData.daysSpentNice + new Random(Find.World.ConstantRandSeed).Next(0, 4) && NesiStorytellerData .state> 0)
                {
                    NesiStorytellerData.state = NesiStorytellerData.state - 1;
                    if (NesiStorytellerData.state == 0)
                    {
                        NesiStorytellerData.daysSpentNice = GenDate.DaysPassed;
                    }
                    NesiStorytellerData.dayLastUpdated = GenDate.DaysPassed;
                    return;
                }
                if(NesiStorytellerData.state== 0 && new Random(Find.World.ConstantRandSeed).Next(1, 10) == 2)
                {
                    NesiStorytellerData.state += new Random(Find.World.ConstantRandSeed).Next(-1, 2);
                    NesiStorytellerData.dayLastUpdated = GenDate.DaysPassed;
                    return;
                }
                if(NesiStorytellerData.state==NesiState.Aggressive && new Random(Find.World.ConstantRandSeed).Next(1, 10) == 2)
                {
                    NesiStorytellerData.state += new Random(Find.World.ConstantRandSeed).Next(0, 1);
                    NesiStorytellerData.dayLastUpdated = GenDate.DaysPassed;
                    return;
                }

            }   

        }

    }
}
