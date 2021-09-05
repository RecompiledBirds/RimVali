using RimWorld;
using UnityEngine;
using Verse;


namespace AvaliMod
{
    public class AvaliEggLayer : ThingComp
    {
        private float eggProgress;
        private int fertilizationCount;
        private Pawn fertilizedBy;
        private readonly bool avaliLayEggs = LoadedModManager.GetMod<RimValiMod>().GetSettings<RimValiModSettings>().avaliLayEggs;

        private bool Active
        {
            get
            {
                Pawn parent = this.parent as Pawn;
                return (!Props.eggLayFemaleOnly || parent == null || parent.gender == Gender.Female) && (parent == null || parent.ageTracker.CurLifeStage.milkable);
            }
        }

        public bool CanLayNow => Active && eggProgress >= 1.0;

        public bool FullyFertilized => fertilizationCount >= Props.eggFertilizationCountMax;


        public AvaliEggLayer_Props Props => (AvaliEggLayer_Props)props;

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look<float>(ref eggProgress, "eggProgress", 0.0f, false);
            Scribe_Values.Look<int>(ref fertilizationCount, "fertilizationCount", 0, false);
            Scribe_References.Look<Pawn>(ref fertilizedBy, "fertilizedBy", false);
        }

        public override void CompTick()
        {
            if (avaliLayEggs)
            {
                if (!Active)
                {
                    return;
                }

                float num = (float)(1.0 / (Props.eggLayIntervalDays * 60000.0));
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

            Thing thing = new Thing();
            if (fertilizationCount > 0)
            {
                thing = ThingMaker.MakeThing(Props.eggFertilizedDef, null);
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
                str += ("\n" + "Fertilized".Translate());
            }

            return str;
        }
    }
}