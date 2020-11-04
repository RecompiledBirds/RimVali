using RimWorld;
using Verse;
namespace AvaliMod
{
    public class AvaliRelationWorker_PackMate : PawnRelationWorker
    {
        public override float GenerationChance(Pawn generated, Pawn other, PawnGenerationRequest request)
        {
            return PackRelationUtilityWorker.PackmateGeneratorChance(generated, other, request, false) * this.BaseGenerationChanceFactor(generated, other, request);
        }
        public override void CreateRelation(Pawn generated, Pawn other, ref PawnGenerationRequest request)
        {
            generated.relations.AddDirectRelation(AvaliDefs.Packmate, other);
        }
    }
}