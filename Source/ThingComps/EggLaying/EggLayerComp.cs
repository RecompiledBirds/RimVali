using RimWorld;
using UnityEngine;
using Verse;

namespace AvaliMod
{
    public class EggLayerComp : ThingComp
    {
        private readonly bool avaliLayEggs =
            LoadedModManager.GetMod<RimValiMod>().GetSettings<RimValiModSettings>().avaliLayEggs;

        private float eggProgress;
        private int fertilizationCount;
        private Pawn fertilizedBy;

        private bool Active
        {
            get
            {
                if (this.parent is Pawn parent)
                {
                    return (!Props.eggLayFemaleOnly || parent.gender == Gender.Female) &&
                           parent.ageTracker.CurLifeStage.milkable;
                }

                return true;
            }
        }

        public bool CanLayNow => Active && eggProgress >= 1.0;

        public bool FullyFertilized => fertilizationCount >= Props.eggFertilizationCountMax;


        public EggLayerProps Props => (EggLayerProps)props;

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref eggProgress, "eggProgress");
            Scribe_Values.Look(ref fertilizationCount, "fertilizationCount");
            Scribe_References.Look(ref fertilizedBy, "fertilizedBy");
        }

        public override void CompTick()
        {
            if (!avaliLayEggs || !Active)
            {
                return;
            }

            var num = (float)(1.0 / (Props.eggLayIntervalDays * GenDate.TicksPerDay));
            if (this.parent is Pawn parent)
            {
                num *= PawnUtility.BodyResourceGrowthSpeed(parent);
            }

            eggProgress += num;
            if (eggProgress > 1.0)
            {
                eggProgress = 1f;
            }
        }

        public void Fertilize(Pawn male)
        {
            fertilizationCount = Props.eggFertilizationCountMax;
            fertilizedBy = male;
        }

        public virtual Thing ProduceEgg()
        {
            if (!Active)
            {
                Log.Error("LayEgg while not Active: " + parent);
            }

            eggProgress = 0.0f;
            int randomInRange = Props.eggCountRange.RandomInRange;
            if (randomInRange == 0)
            {
                return null;
            }

            var thing = new Thing();

            if (fertilizationCount > 0)
            {
                thing = ThingMaker.MakeThing(Props.eggFertilizedDef);
                fertilizationCount = Mathf.Max(0, fertilizationCount - randomInRange);
            }

            return thing;
        }

        public override string CompInspectStringExtra()
        {
            if (!Active)
            {
                return null;
            }

            string str = "EggProgress".Translate() + ": " + eggProgress.ToStringPercent();
            if (fertilizationCount > 0)
            {
                str += "\n" + "Fertilized".Translate();
            }

            return str;
        }
    }
}
