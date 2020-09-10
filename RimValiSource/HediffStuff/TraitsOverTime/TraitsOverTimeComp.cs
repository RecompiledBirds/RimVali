using RimWorld;
using System;
using System.Collections.Generic;
using Verse;
namespace AvaliMod
{
    public class TraitsOverTime : HediffComp
    {
        public bool warned = false;
        Random rand = new Random();
        private TraitsOverTimeProps Props
        {
            get
            {
                return (TraitsOverTimeProps)this.props;
            }
        }
        private List<TraitsOverTimeClass> traitsOverTimes
        {
            get
            {
                return this.Props.traits;
            }
        }
        public override void CompPostTick(ref float severityAdjustment)
        {
            Pawn pawn = this.parent.pawn;
            float pawnAge = pawn.ageTracker.AgeBiologicalYearsFloat;
            if ((double)pawnAge <= 0.0 && !((double)pawnAge >= 0.5))
            {
                pawn.story.traits.allTraits.Clear();
                pawn.workSettings.DisableAll();
            }
            else
            {
                if (pawnAge > 0.5f)
                {
                    pawn.workSettings.EnableAndInitializeIfNotAlreadyInitialized();
                }
                if (rand.Next(0, 5) == 5)
                {
                    makeTraits(rand.Next(0, traitsOverTimes.Count));
                }

            }
        }
        public void traitIfStatsMatch(TraitDef trait, RecordDef record, float recordCount, int degree, int age, SimpleCurve ageOverTime, bool changeDegree)
        {
            Pawn pawn = this.parent.pawn;
            float x = (float)age / pawn.RaceProps.lifeExpectancy;
            float daysPassed = (float)(((double)pawn.ageTracker.AgeBiologicalYearsFloat));
            if ((double)daysPassed < 0.0)
            {
                Log.Error("daysPassed < 0, pawn=" + (object)pawn, false);
                return;
            }
            if ((double)Rand.Value < (double)ageOverTime.Evaluate(x))
            {
                if (!(trait.degreeDatas.Count > 1) || changeDegree == false)
                {
                    if (pawn.records.GetValue(record) >= recordCount & !(pawn.story.traits.HasTrait(trait)) & !(pawn.story.traits.HasTrait(trait)))
                    {
                        pawn.story.traits.GainTrait(new Trait(trait, degree, false));
                    }
                }
                else
                {
                    if (pawn.story.traits.HasTrait(trait))
                    {
                        if (!(degree + 1 >= trait.degreeDatas.Count - 1))
                        {
                            pawn.story.traits.GainTrait(new Trait(trait, degree + 1, false));
                        }
                    }
                }
            }
        }
        public void makeTraits(int item)
        {
            Pawn pawn = this.parent.pawn;
            float recordCounts = traitsOverTimes[item].recordCount;
            TraitDef traitDef = traitsOverTimes[item].traitDef;
            SimpleCurve ageOverTime = traitsOverTimes[item].ageFractionCurve;
            RecordDef records = traitsOverTimes[item].recordDef;
            int degree = traitsOverTimes[item].degree;
            bool changeDegree = traitsOverTimes[item].changeDegree;
            traitIfStatsMatch(traitDef, records, recordCounts, degree, pawn.ageTracker.AgeBiologicalYears, ageOverTime, changeDegree);
        }
    }
}