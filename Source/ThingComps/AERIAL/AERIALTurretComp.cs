using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace AvaliMod
{
    public class AERIALTurretComp : ThingComp
    {
        public AERIALProps Props => (AERIALProps)props;

        public override IEnumerable<FloatMenuOption> CompFloatMenuOptions(Pawn pawn)
        {
            if (!pawn.RaceProps.ToolUser)
            {
                yield break;
            }

            if (!pawn.CanReserveAndReach(parent, PathEndMode.InteractionCell, Danger.Deadly))
            {
                yield break;
            }

            var floatMenuOption = new FloatMenuOption("RefuelThing".Translate(parent.LabelShort, parent), delegate
            {
                Job job = JobMaker.MakeJob(AvaliDefs.RefuelAerial, parent);
                pawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
            });
            yield return floatMenuOption;
        }
    }
}
