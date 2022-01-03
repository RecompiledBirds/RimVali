using UnityEngine;
using Verse;
using RimWorld;


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
                return (!this.Props.eggLayFemaleOnly || parent == null || parent.gender == Gender.Female) && (parent == null || parent.ageTracker.CurLifeStage.milkable);
            }
        }

        public bool CanLayNow
        {
            get
            {
                return this.Active && (double)this.eggProgress >= 1.0;
            }
        }

        public bool FullyFertilized
        {
            get
            {
                return this.fertilizationCount >= this.Props.eggFertilizationCountMax;
            }
        }


        public AvaliEggLayer_Props Props
        {
            get
            {
                return (AvaliEggLayer_Props)this.props;
            }
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look<float>(ref this.eggProgress, "eggProgress", 0.0f, false);
            Scribe_Values.Look<int>(ref this.fertilizationCount, "fertilizationCount", 0, false);
            Scribe_References.Look<Pawn>(ref this.fertilizedBy, "fertilizedBy", false);
        }

        public override void CompTick()
        {
            if (avaliLayEggs) {
                if (!this.Active)
                    return;
                float num = (float)(1.0 / ((double)this.Props.eggLayIntervalDays * 60000.0));
                if (this.parent is Pawn parent)
                    num *= PawnUtility.BodyResourceGrowthSpeed(parent);
                this.eggProgress += num;
                if ((double)this.eggProgress > 1.0)
                    this.eggProgress = 1f;
            }
        }

        public void Fertilize(Pawn male)
        {
            this.fertilizationCount = this.Props.eggFertilizationCountMax;
            this.fertilizedBy = male;
        }

        public virtual Thing ProduceEgg()
        {
            if (!this.Active)
                Log.Error("LayEgg while not Active: " + (object)this.parent, false);
            this.eggProgress = 0.0f;
            int randomInRange = this.Props.eggCountRange.RandomInRange;
            if (randomInRange == 0)
                return (Thing)null;
            Thing thing = new Thing();
            if (this.fertilizationCount > 0)
            {
                thing = ThingMaker.MakeThing(this.Props.eggFertilizedDef, (ThingDef)null);
                this.fertilizationCount = Mathf.Max(0, this.fertilizationCount - randomInRange);
            }
            return thing;
        }

        public override string CompInspectStringExtra()
        {
            if (!this.Active)
                return (string)null;
            string str = (string)("EggProgress".Translate() + ": " + this.eggProgress.ToStringPercent());
            if (this.fertilizationCount > 0)
                str = (string)(str + ("\n" + "Fertilized".Translate()));
            return str;
        }
    }
}