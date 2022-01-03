using Verse;
using Verse.AI;

namespace AvaliMod
{
    public class PackLossWorker : MentalStateWorker
    {
        public override bool StateCanOccur(Pawn pawn)
        {
            if (pawn.def == AvaliDefs.RimVali &&
                (pawn.needs.mood.thoughts.memories.Memories.Any(x => x.def == AvaliDefs.AvaliPackLoss) ||
                 pawn.story.traits.HasTrait(AvaliDefs.AvaliPackBroken)))
            {
                return base.StateCanOccur(pawn);
            }

            return false;
        }
    }
}
