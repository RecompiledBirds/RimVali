using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using Verse;

namespace AvaliMod
{
    public class CreditDef : Def
    {
        public List<string> names = new List<string>();
        public string title;
    }

    [HarmonyPatch(typeof(CreditsAssembler), nameof(CreditsAssembler.AllCredits))]
    public class Credits
    {
        [HarmonyPostfix]
        public static IEnumerable<CreditsEntry> CreditsPatch(IEnumerable<CreditsEntry> __result)
        {
            foreach (CreditsEntry creditsEntry in __result)
            {
                yield return creditsEntry;               
            }

            yield return new CreditRecord_Space(150f);
            yield return new CreditRecord_Title("RimVali would like to thank several people:");
            foreach (CreditDef def in DefDatabase<CreditDef>.AllDefs)
            {
                yield return new CreditRecord_Space(150f);
                yield return new CreditRecord_Title(def.title);
                foreach (string name in def.names)
                {
                    yield return new CreditRecord_Role(null, name);
                }
            }
        }
    }
}
