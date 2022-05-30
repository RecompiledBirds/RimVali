using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimValiCore;
using AvaliMod;
using Verse;

namespace RimVali.Storytellers
{
    public class JhinStoryTellerProps : StorytellerCompProperties
    {
        public SimpleCurve acceptFractionByDaysPassedCurve;

        public SimpleCurve acceptPercentFactorPerProgressScoreCurve;

        public SimpleCurve acceptPercentFactorPerThreatPointsCurve;

        public int maxIncidents = 5;

        public int minIncidents = 1;

        public float minSpacingDays;

        public float offDays;
        public float onDays;

        public JhinStoryTellerProps()
        {
            compClass = typeof(JhinStoryteller);
        }
    }
    public class JhinStoryteller : StorytellerComp
    {
        protected NesiStorytellerProps Props => (NesiStorytellerProps)props;
        public override IEnumerable<FiringIncident> MakeIntervalIncidents(IIncidentTarget target)
        {
            Update();
            var num = 1f;
            if (Props.acceptFractionByDaysPassedCurve != null)
            {
                num *= Props.acceptFractionByDaysPassedCurve.Evaluate(GenDate.DaysPassedFloat);
            }

            if (Props.acceptPercentFactorPerThreatPointsCurve != null)
            {
                num *= Props.acceptPercentFactorPerThreatPointsCurve.Evaluate(
                    StorytellerUtility.DefaultThreatPointsNow(target));
            }

            if (Props.acceptPercentFactorPerProgressScoreCurve != null)
            {
                num *= Props.acceptPercentFactorPerProgressScoreCurve.Evaluate(
                    StorytellerUtility.GetProgressScore(target));
            }

            int incCount = IncidentCycleUtility.IncidentCountThisInterval(target,
                Find.Storyteller.storytellerComps.IndexOf(this), Props.minDaysPassed, Props.onDays, Props.offDays,
                Props.minSpacingDays, Props.minIncidents, Props.maxIncidents, num);

            incCount += HasSlaves ? random.Next(1,2) : 0;

            for(int a = 0; a < incCount; a++)
            {
                FiringIncident inc = null;
                switch (StorytellerData.state)
                {
                    case StorytellerState.Neutral:
                        inc = GenNeutral(target);
                        break;
                    case StorytellerState.HyperAggressive:
                        inc =GenHyperAggressive(target);
                        break;
                    case StorytellerState.Aggressive:
                        inc = GenAggressive(target);
                        break;
                    case StorytellerState.Hunting:
                        inc = GenHunting(target);
                        break;
                    case StorytellerState.Calm:
                        break;
                    case StorytellerState.Friendly:
                        break;

                }

                if(inc!=null)
                    yield return inc;
            }
        }
        private static Random random = new Random();
        int countSlaves;
        int countFree;
        public float RatioSlavesToNonSlaves=>countSlaves/countFree;
        public float RatioNonSlavesToSlaves=>countFree/countSlaves;

        public bool HasSlaves => countSlaves > 0;

        int daysSinceLastupdated;

        private bool started = false;


        public void Update()
        {
            if ((GenDate.DaysPassed % 2 == 0 && GenDate.DaysPassed - daysSinceLastupdated != 0) || !started)
            {
                countSlaves = RimValiCore.RimValiUtility.AllPawnsOfFactionSpawned(Faction.OfPlayer).Where(pawn => pawn.IsSlave).Count();
                countFree = RimValiCore.RimValiUtility.AllPawnsOfFactionSpawned(Faction.OfPlayer).Where(pawn => pawn.IsFreeNonSlaveColonist).Count();
                daysSinceLastupdated = GenDate.DaysPassed;
                started = true;
                CalculateState();
            }

        }
        public void SetDaysPassed(int days)
        {
            StorytellerData.daysSpentNice=days;
        }
        public void CalculateState()
        {
            int daysPassed = GenDate.DaysPassed;
            if (daysPassed > StorytellerData.dayLastUpdated)
            {
                bool shouldChange = HasSlaves && Rand.Chance(0.2f);
                if (shouldChange && StorytellerData.state<=StorytellerState.Neutral && countSlaves >= 2)
                {
                    StorytellerData.daysPassedSinceLastHunt=daysPassed;
                    StorytellerData.state=StorytellerState.Hunting;
                    SetDaysPassed(daysPassed);
                    return;
                    
                }else if(shouldChange)
                {
                    StorytellerData.state -= random.Next(-2,0);
                }

                if ((daysPassed > StorytellerData.daysPassedSinceLastHunt +
                    random.Next(2, 9) && StorytellerData.state <= 0 &&
                    Rand.Chance(0.1f)))
                {
                    StorytellerData.daysPassedSinceLastHunt = daysPassed;
                    StorytellerData.state = StorytellerState.Hunting;
                    SetDaysPassed(daysPassed);
                    return;
                }

                if (daysPassed > StorytellerData.daysSpentNice + random.Next(0, 8) && StorytellerData.state > 0)
                {
                    StorytellerData.state -= 1;
                    if (StorytellerData.state >= 0)
                    {
                        StorytellerData.daysSpentNice = daysPassed;
                    }
                    SetDaysPassed(daysPassed);
                    return;
                }

                if (!HasSlaves && StorytellerData.state == 0 && random.Next(1, 10) == 2)
                {
                    StorytellerData.state += random.Next(-1, 2);
                    SetDaysPassed(daysPassed);
                    return;
                }

                if (HasSlaves)
                {
                    SetDaysPassed(daysPassed);
                    StorytellerData.state--;
                }
            }
        }

        #region incident gen

        public FiringIncident GenAggressive(IIncidentTarget targ)
        {
            IncidentDef def= new List<IncidentDef>() { IncidentDefOf.RaidEnemy,IncidentDefOf.ManhunterPack,IncidentDefOf.ToxicFallout,IncidentDefOf.MechCluster,IncidentDefOf.SolarFlare}.RandomElement();
            float pointsMultiplier = HasSlaves ? RatioSlavesToNonSlaves : 1;
            IncidentParms parms = new IncidentParms();
            parms.attackTargets = (List<Thing>)RimValiCore.RimValiUtility.AllPawnsOfFactionSpawned(Faction.OfPlayer).Where(pawn => pawn.IsFreeNonSlaveColonist);
            parms.points = StorytellerUtilityPopulation.AdjustedPopulation * pointsMultiplier;

            return new FiringIncident(def,this,parms);
        }

        public FiringIncident GenHyperAggressive(IIncidentTarget targ)
        {
            IncidentDef def = new List<IncidentDef>() { IncidentDefOf.RaidEnemy,IncidentDefOf.MechCluster }.RandomElement();
            float pointsMultiplier = HasSlaves ? RatioSlavesToNonSlaves*3 : 1;
            IncidentParms parms = new IncidentParms();
            parms.attackTargets = (List<Thing>)RimValiCore.RimValiUtility.AllPawnsOfFactionSpawned(Faction.OfPlayer).Where(pawn => pawn.IsFreeNonSlaveColonist);
            parms.points = StorytellerUtilityPopulation.AdjustedPopulation * pointsMultiplier;

            return new FiringIncident(IncidentDefOf.RaidEnemy, this, parms);
        }

        public FiringIncident GenHunting(IIncidentTarget targ)
        {
            IncidentDef def = new List<IncidentDef>() { IncidentDefOf.SolarFlare, IncidentDefOf.ToxicFallout, IncidentDefOf.ToxicFallout, IncidentDefOf.Infestation,AvaliDefs.MeteoriteImpact, IncidentDefOf.SolarFlare,AvaliDefs.VolcanicWinter, AvaliDefs.ShortCircuit, AvaliDefs.VolcanicWinter, AvaliDefs.ShortCircuit }.RandomElement();
            float pointsMultiplier = HasSlaves ? RatioSlavesToNonSlaves*2 : 1;
            IncidentParms parms = new IncidentParms();
            parms.attackTargets = (List<Thing>)RimValiCore.RimValiUtility.AllPawnsOfFactionSpawned(Faction.OfPlayer).Where(pawn => pawn.IsFreeNonSlaveColonist);
            parms.points = StorytellerUtilityPopulation.AdjustedPopulation * pointsMultiplier;
           
            return new FiringIncident(def, this, parms);
        }

        public FiringIncident GenNeutral(IIncidentTarget targ)
        {
            IncidentDef def = new List<IncidentDef>() { IncidentDefOf.FarmAnimalsWanderIn, IncidentDefOf.Eclipse,IncidentDefOf.RaidEnemy,IncidentDefOf.ShipChunkDrop,IncidentDefOf.TravelerGroup,IncidentDefOf.VisitorGroup,IncidentDefOf.WandererJoin, AvaliDefs.VolcanicWinter,AvaliDefs.ShortCircuit,AvaliDefs.Alphabeavers, IncidentDefOf.GiveQuest_Random }.RandomElement();
            float pointsMultiplier = HasSlaves ? RatioNonSlavesToSlaves : 1;
            IncidentParms parms = new IncidentParms();
            //parms.attackTargets = (List<Thing>)RimValiCore.RimValiUtility.AllPawnsOfFactionSpawned(Faction.OfPlayer).Where(pawn => pawn.IsFreeNonSlaveColonist);
            parms.points = StorytellerUtilityPopulation.AdjustedPopulation * pointsMultiplier;

            return new FiringIncident(def, this, parms);
        }

        public FiringIncident GenCalm(IIncidentTarget targ)
        {
            IncidentDef def = new List<IncidentDef>() { IncidentDefOf.FarmAnimalsWanderIn, IncidentDefOf.VisitorGroup, IncidentDefOf.TravelerGroup, IncidentDefOf.TraderCaravanArrival, IncidentDefOf.TravelerGroup, IncidentDefOf.VisitorGroup, IncidentDefOf.WandererJoin, IncidentDefOf.ShipChunkDrop, IncidentDefOf.GiveQuest_Random }.RandomElement();
            float pointsMultiplier = HasSlaves ? RatioNonSlavesToSlaves: 2;
            IncidentParms parms = new IncidentParms();
            //parms.attackTargets = (List<Thing>)RimValiCore.RimValiUtility.AllPawnsOfFactionSpawned(Faction.OfPlayer).Where(pawn => pawn.IsFreeNonSlaveColonist);
            parms.points = StorytellerUtilityPopulation.AdjustedPopulation * pointsMultiplier;

            return new FiringIncident(def, this, parms);
        }

        public FiringIncident GenFriendly(IIncidentTarget targ)
        {
            IncidentDef def = new List<IncidentDef>() { IncidentDefOf.FarmAnimalsWanderIn, IncidentDefOf.VisitorGroup, IncidentDefOf.TravelerGroup, IncidentDefOf.TraderCaravanArrival, IncidentDefOf.OrbitalTraderArrival, IncidentDefOf.VisitorGroup, IncidentDefOf.WandererJoin, IncidentDefOf.GauranlenPodSpawn,IncidentDefOf.GiveQuest_Random }.RandomElement();
            float pointsMultiplier = HasSlaves ? RatioNonSlavesToSlaves : 3;
            IncidentParms parms = new IncidentParms();
            //parms.attackTargets = (List<Thing>)RimValiCore.RimValiUtility.AllPawnsOfFactionSpawned(Faction.OfPlayer).Where(pawn => pawn.IsFreeNonSlaveColonist);
            parms.points = StorytellerUtilityPopulation.AdjustedPopulation * pointsMultiplier;

            return new FiringIncident(def, this, parms);
        }
        #endregion
    }
}
