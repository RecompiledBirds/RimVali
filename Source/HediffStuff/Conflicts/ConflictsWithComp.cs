using System.Collections.Generic;
using Verse;

namespace AvaliMod
{
    public class ConflictsWith : HediffComp
    {
        private int onItem;

        public ConflictsWithProps Props => (ConflictsWithProps)props;

        private List<HediffDef> ConflictingHediffs => Props.conflictingHediffs;

        private bool ShowConflicts => Props.showConflicts;

        private bool DebugInfo => Props.debugInfo;

        public override string CompTipStringExtra
        {
            get
            {
                if (!ShowConflicts)
                {
                    return string.Empty;
                }

                var output = "Conflicts with: ";
                onItem = 0;
                foreach (HediffDef hediffDef in ConflictingHediffs)
                {
                    if (DebugInfo)
                    {
                        Log.Message("Conflict count: " + ConflictingHediffs.Count);
                        Log.Message("onItem [pre-add]: " + onItem);
                        Log.Message("Hediff at position: " + ConflictingHediffs[onItem]);
                    }

                    if (onItem + 1 >= ConflictingHediffs.Count)
                    {
                        output = output + hediffDef.label + ". ";
                    }
                    else
                    {
                        output = output + hediffDef.label + ", ";
                    }

                    onItem++;
                    if (DebugInfo)
                    {
                        Log.Message("onItem [post-add]: " + onItem);
                        Log.Message("-------------------------");
                    }
                }

                return output;
            }
        }

        public override void CompPostTick(ref float severityAdjustment)
        {
            Pawn pawn = parent.pawn;
            if (!pawn.Spawned)
            {
                return;
            }

            onItem = 0;
            foreach (HediffDef hediffDef in ConflictingHediffs)
            {
                Hediff hediff = HediffMaker.MakeHediff(hediffDef, pawn);
                if (!pawn.health.hediffSet.HasHediff(hediffDef))
                {
                    continue;
                }

                pawn.health.RemoveHediff(pawn.health.hediffSet.GetFirstHediffOfDef(hediffDef));
                if (hediffDef.spawnThingOnRemoved != null)
                {
                    GenSpawn.Spawn(hediffDef.spawnThingOnRemoved, pawn.Position, pawn.Map);
                }

                Log.Message("Removed hediff: " + hediffDef.defName + " from pawn " + hediff.pawn);
            }
        }
    }
}
