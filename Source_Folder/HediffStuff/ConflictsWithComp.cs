using System.Collections.Generic;
using Verse;
namespace AvaliMod
{
    public class ConflictsWith : HediffComp
    {
        public ConflictsWithProps Props
        {
            get
            {
                return (ConflictsWithProps)this.props;
            }
        }
        private List<HediffDef> conflictingHediffs
        {
            get
            {
                return this.Props.conflictingHediffs;
            }
        }
        private bool showConflicts
        {
            get
            {
                return this.Props.showConflicts;
            }
        }
        private bool debugInfo
        {
            get
            {
                return this.Props.debugInfo;
            }
        }
        private int onItem = 0;
        public override void CompPostTick(ref float severityAdjustment)
        {
            Pawn pawn = parent.pawn;
            if (pawn.Spawned)
            {
                onItem = 0;
                foreach (HediffDef hediffDef in conflictingHediffs)
                {

                    Hediff hediff = HediffMaker.MakeHediff(hediffDef, pawn, null);
                    if (pawn.health.hediffSet.HasHediff(hediffDef))
                    {
                        pawn.health.RemoveHediff(pawn.health.hediffSet.GetFirstHediffOfDef(hediffDef));
                        if (!(hediffDef.spawnThingOnRemoved == null))
                        {
                            GenSpawn.Spawn(hediffDef.spawnThingOnRemoved, pawn.Position, pawn.Map, WipeMode.Vanish);
                        }
                        Log.Message("Removed hediff: " + hediffDef.defName.ToString() + " from pawn " + hediff.pawn.ToString());
                    }
                }
            }
        }
        public override string CompTipStringExtra
        {
            get
            {
                string empty = string.Empty;
                if (!showConflicts)
                    return empty;
                string output = empty + "Conflicts with: ";
                onItem = 0;
                foreach (HediffDef hediffDef in conflictingHediffs)
                {
                    if (debugInfo)
                    {
                        Log.Message("Conflict count: " + conflictingHediffs.Count.ToString());
                        Log.Message("onItem [pre-add]: " + onItem.ToString());
                        Log.Message("Hediff at position: " + conflictingHediffs[onItem].ToString());
                    }
                    if ((onItem + 1) >= conflictingHediffs.Count){output = output + hediffDef.label + ". ";}
                    else{output = output + hediffDef.label + ", ";}
                    onItem = onItem + 1;
                    if (debugInfo)
                    {
                        Log.Message("onItem [post-add]: " + onItem.ToString());
                        Log.Message("-------------------------");
                    }
                }
                return output;
            }
        }

    }
}