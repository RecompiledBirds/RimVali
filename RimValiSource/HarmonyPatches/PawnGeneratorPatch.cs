using HarmonyLib;
using RimWorld;
using System.Linq;
using Verse;
//Code by LiQd
namespace AvaliMod
{
    [HarmonyPatch(typeof(PawnGenerator))]
    [HarmonyPatch("GeneratePawnRelations")]
    class ExtraRelations
    {
        public static void Postfix(Pawn pawn, ref PawnGenerationRequest request)
        {
            if (pawn.def.defName.ToLower().Contains("vali"))
            {
                AddExtraRelations(pawn, ref request, 4);
            }
        }

        private static Pair<Pawn, PawnRelationDef>[] GenerateSamples(Pawn[] pawns, PawnRelationDef[] relations, int count)
        {
            Pair<Pawn, PawnRelationDef>[] array = new Pair<Pawn, PawnRelationDef>[count];
            for (int i = 0; i < count; i++)
            {
                array[i] = new Pair<Pawn, PawnRelationDef>(pawns[Rand.Range(0, pawns.Length)], relations[Rand.Range(0, relations.Length)]);
            }
            return array;
        }
        private static PawnRelationDef[] relationsGeneratableNonblood = (from rel in DefDatabase<PawnRelationDef>.AllDefsListForReading
                                                                         where !rel.familyByBloodRelation && rel.generationChanceFactor > 0f
                                                                         select rel).ToArray<PawnRelationDef>();
        private static void AddExtraRelations(Pawn pawn, ref PawnGenerationRequest request, int amount)
        {


            Pawn[] array = (from x in PawnsFinder.AllMapsWorldAndTemporary_AliveOrDead
                            where x.def == pawn.def
                            select x).ToArray<Pawn>();
            if (array.Length == 0)
            {
                return;
            }
            int num = 0;
            foreach (Pawn pawn2 in array)
            {
                if (pawn2.Discarded)
                {
                    Log.Warning(string.Concat(new object[]
                    {
                        "Warning during generating pawn relations for ",
                        pawn,
                        ": Pawn ",
                        pawn2,
                        " is discarded, yet he was yielded by PawnUtility. Discarding a pawn means that he is no longer managed by anything."
                    }), false);
                }
                else if (pawn2.Faction != null && pawn2.Faction.IsPlayer)
                {
                    num++;
                }
            }
            float num2 = 45f;
            num2 += (float)num * 2.7f;
            PawnGenerationRequest localReq = request;
            int i = 0;
            while (i < amount)
            {
                i++;
                Pair<Pawn, PawnRelationDef> pair2 = ExtraRelations.GenerateSamples(array, ExtraRelations.relationsGeneratableNonblood, 40).RandomElementByWeightWithDefault((Pair<Pawn, PawnRelationDef> x) => x.Second.generationChanceFactor * x.Second.Worker.GenerationChance(pawn, x.First, localReq), num2 * 40f / (float)(array.Length * ExtraRelations.relationsGeneratableNonblood.Length));
                if (pair2.First != null)
                {
                    pair2.Second.Worker.CreateRelation(pawn, pair2.First, ref request);
                }
            }
        }
    }
}
