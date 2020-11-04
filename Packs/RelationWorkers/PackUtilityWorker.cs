using RimWorld;
using UnityEngine;
using Verse;
namespace AvaliMod
{
    public static class PackRelationUtilityWorker
    {
        public static float PackmateGeneratorChance(
      Pawn generated,
      Pawn other,
      PawnGenerationRequest request,
      bool ex)
        {
            if ((double)generated.ageTracker.AgeBiologicalYearsFloat < 5.0)
                if (!(generated.RaceProps.body.defName == AvaliDefs.RimValiBody.defName))
                {
                    return 0.0f;
                }
            if (!(other.RaceProps.body.defName == AvaliDefs.RimValiBody.defName))
            {

                return 0.0f;
            }
            float num1 = 2f;
            float generationChanceAgeFactor1 = PackRelationUtilityWorker.GetGenerationChanceAgeFactor(generated);
            float generationChanceAgeFactor2 = PackRelationUtilityWorker.GetGenerationChanceAgeFactor(other);
            float chanceAgeGapFactor = PackRelationUtilityWorker.GetGenerationChanceAgeGapFactor(generated, other, ex);
            float num4 = 2f;
            float num5 = !request.FixedMelanin.HasValue ? PawnSkinColors.GetMelaninCommonalityFactor(other.story.melanin) : ChildRelationUtility.GetMelaninSimilarityFactor(request.FixedMelanin.Value, other.story.melanin);
            return num1 * generationChanceAgeFactor1 * generationChanceAgeFactor2 * chanceAgeGapFactor * num5 * num4;
        }
        private static float GetGenerationChanceAgeFactor(Pawn p)
        {
            return Mathf.Clamp(GenMath.LerpDouble(14f, 27f, 0.0f, 1f, p.ageTracker.AgeBiologicalYearsFloat), 0.0f, 1f);
        }

        private static float GetGenerationChanceAgeGapFactor(Pawn p1, Pawn p2, bool ex)
        {
            float num = Mathf.Abs(p1.ageTracker.AgeBiologicalYearsFloat - p2.ageTracker.AgeBiologicalYearsFloat);
            return (double)num > 40.0 ? 0.0f : Mathf.Clamp(GenMath.LerpDouble(0.0f, 20f, 1f, 1f / 1000f, num), 1f / 1000f, 1f);
        }
        private static float MinPossibleAgeGapAtMinAgeToGenerateAsPackMates(Pawn p1, Pawn p2)
        {
            float atChronologicalAge = p1.ageTracker.AgeChronologicalYearsFloat - 14f;
            if ((double)atChronologicalAge < 0.0)
            {
                Log.Warning("at < 0", false);
                return 0.0f;
            }
            float num1 = PawnRelationUtility.MaxPossibleBioAgeAt(p2.ageTracker.AgeBiologicalYearsFloat, p2.ageTracker.AgeChronologicalYearsFloat, atChronologicalAge);
            float num2 = PawnRelationUtility.MinPossibleBioAgeAt(p2.ageTracker.AgeBiologicalYearsFloat, atChronologicalAge);
            if ((double)num1 < 0.0 || (double)num1 < 14.0)
                return -1f;
            return (double)num2 <= 14.0 ? 0.0f : num2 - 14f;
        }
    }
}