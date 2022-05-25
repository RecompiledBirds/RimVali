using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace AvaliMod
{
    public class PackBreakWorker : MentalBreakWorker
    {
        public override bool BreakCanOccur(Pawn pawn)
        {
            var thoughts = new List<Thought>();
            pawn.needs.mood.thoughts.GetAllMoodThoughts(thoughts);
            if (AvaliDefs.IsAvali(pawn) && thoughts.Any(x => x.def == AvaliDefs.AvaliPackLoss) ||
                pawn.story.traits.HasTrait(AvaliDefs.AvaliPackBroken))
            {
                return base.BreakCanOccur(pawn) && true;
            }

            return base.BreakCanOccur(pawn);
        }
    }
}
