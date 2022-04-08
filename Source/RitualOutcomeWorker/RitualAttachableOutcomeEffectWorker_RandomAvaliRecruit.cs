using RimWorld;
using RimWorld.QuestGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace AvaliMod
{
    public class RitualAttachableOutcomeEffectWorker_RandomAvaliRecruit : RitualAttachableOutcomeEffectWorker
	{
		public override void Apply(Dictionary<Pawn, int> totalPresence, LordJob_Ritual jobRitual, OutcomeChance outcome, out string extraOutcomeDesc, ref LookTargets letterLookTargets)
		{
			if (Rand.Chance(0.5f))
			{
				Slate slate = new Slate();
				slate.Set("map", jobRitual.Map, false);
				slate.Set("overridePawnGenParams", new PawnGenerationRequest(kind: AvaliDefs.RimValiColonist, context: PawnGenerationContext.NonPlayer, forceGenerateNewPawn: true, canGeneratePawnRelations: true, mustBeCapableOfViolence: false, colonistRelationChanceFactor: 20f, prohibitedTraits: null, fixedIdeo: jobRitual.Ritual.ideo), false);
				QuestUtility.GenerateQuestAndMakeAvailable(QuestScriptDefOf.WandererJoins, slate);
				extraOutcomeDesc = def.letterInfoText;
				return;
			}
			extraOutcomeDesc = null;
		}

		// Token: 0x040036AB RID: 13995
		public const float RecruitChance = 0.5f;
	}
}
