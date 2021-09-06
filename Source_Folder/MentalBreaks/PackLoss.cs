using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;

namespace AvaliMod
{
    public class PackLossWorker : MentalStateWorker
    {
        public override bool StateCanOccur(Pawn pawn)
        {
            if ((pawn.def == AvaliDefs.RimVali || pawn.def == AvaliDefs.IWAvaliRace) && (pawn.needs.mood.thoughts.memories.Memories.Any(x => x.def == AvaliDefs.AvaliPackLoss) || pawn.story.traits.HasTrait(AvaliDefs.AvaliPackBroken)))
            {
                return base.StateCanOccur(pawn) && true;
            }
            return false;
        }
    }

    public class PackBreakworker : MentalBreakWorker
    {
        public override bool BreakCanOccur(Pawn pawn)
        {
            List<Thought> thoughts = new List<Thought>();
            pawn.needs.mood.thoughts.GetAllMoodThoughts(thoughts);
            if ((pawn.def == AvaliDefs.RimVali || pawn.def == AvaliDefs.IWAvaliRace) && thoughts.Any(x => x.def == AvaliDefs.AvaliPackLoss) || pawn.story.traits.HasTrait(AvaliDefs.AvaliPackBroken))
            {
                return base.BreakCanOccur(pawn) && true;
            }
            return base.BreakCanOccur(pawn);
        }
    }
}