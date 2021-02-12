using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace AvaliMod
{
	public class CreditDef : Def
	{
		public string title;
		public List<string> names = new List<string>();
	}
	[HarmonyPatch(typeof(CreditsAssembler), "AllCredits")]
	public class Credits
	{
		[HarmonyPostfix]
		public static IEnumerable<CreditsEntry> CreditsPatch(IEnumerable<CreditsEntry> __result)
		{
			foreach (CreditsEntry creditsEntry in __result)
			{
				yield return creditsEntry;
				CreditRecord_Role creditRecord_Role = creditsEntry as CreditRecord_Role;
				if (creditRecord_Role != null && creditRecord_Role.creditee == "Many other gracious volunteers!" /*Thanks for playing!*/)
				{

					yield return new CreditRecord_Space(150f);
					yield return new CreditRecord_Title("RimVali would like to thank several people:");
					foreach (CreditDef def in DefDatabase<CreditDef>.AllDefs)
					{
						yield return new CreditRecord_Space(150f);
						yield return new CreditRecord_Title(def.title);
						foreach (String name in def.names)
						{
							yield return new CreditRecord_Role(null, name);
						}
					}

				}

			}
		}
	}
}
