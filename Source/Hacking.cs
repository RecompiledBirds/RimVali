using System;
using System.Collections.Generic;
using System.Linq;
using RimValiCore.RVR;
using RimWorld;
using RimWorld.Planet;
using Verse;
using Random = UnityEngine.Random;

namespace AvaliMod
{
    public static class ResearchExtension
    {
        public static bool AllPrerequisitesDone(this ResearchProjectDef def)
        {
            return def.prerequisites == null || def.prerequisites.All(prerequisite => prerequisite.IsFinished);
        }
    }

    public class HackingGameComp : WorldComponent
    {
        public Dictionary<ResearchProjectDef, bool> hackProjects = new Dictionary<ResearchProjectDef, bool>();
        public List<string> techLvls;
        private int tick;

        public HackingGameComp(World world) : base(world)
        {
            techLvls = Enum.GetNames(typeof(TechLevel)).ToList();
        }

        public List<ResearchProjectDef> GetAllUnhacked
        {
            get
            {
                return DefDatabase<ResearchProjectDef>.AllDefs.Where(researchDef =>
                    {
                        if (!researchDef.AllPrerequisitesDone())
                        {
                            return false;
                        }

                        if (researchDef.IsFinished)
                        {
                            return false;
                        }

                        if (hackProjects.TryGetValue(researchDef))
                        {
                            return false;
                        }

                        return DefDatabase<FactionResearchRestrictionDef>.AllDefs
                            .Any(
                                restrictionDef => restrictionDef
                                                      .factionResearchRestrictionBlackList
                                                      .Any(
                                                          restriction =>
                                                              restriction.researchProj == researchDef
                                                              && restriction.isHackable
                                                              && restriction.factionDef != Faction.OfPlayer.def)
                                                  || restrictionDef
                                                      .factionResearchRestrictions.Any(restriction =>
                                                          restriction.researchProj == researchDef
                                                          && restriction.isHackable
                                                          && restriction.factionDef != Faction.OfPlayer.def));
                    })
                    .ToList();
            }
        }

        public override void ExposeData()
        {
            Scribe_Collections.Look(ref hackProjects, "hackProjects");
            base.ExposeData();
        }

        public void SendMessage(ResearchProjectDef def)
        {
            ChoiceLetter choiceLetter = LetterMaker.MakeLetter("HackedProject".Translate(def.label.Named("PROJECT")),
                "UnlockedResearch".Translate(def.label.Named("PROJECT")), AvaliDefs.IlluminateAirdrop);
            Find.LetterStack.ReceiveLetter(choiceLetter);
        }

        public void HackResearch(ResearchProjectDef def)
        {
            if (!def.AllPrerequisitesDone())
            {
                return;
            }

            hackProjects.Add(def, true);
            SendMessage(def);
        }

        public override void WorldComponentTick()
        {
            if (Find.ResearchManager.currentProj != null)
            {
                if (++tick >= GenDate.TicksPerDay * 5)
                {
                    if (Random.Range(0, 100) < RimValiMod.settings.hackChance)
                    {
                        ResearchProjectDef def = GetAllUnhacked.RandomElement();
                        if (RimValiMod.settings.enableDebugMode)
                        {
                            Log.Message($"Available projects for hacking: {GetAllUnhacked.Count}");
                        }

                        if (def != null)
                        {
                            HackResearch(def);
                        }
                    }

                    tick = 0;
                }
            }

            base.WorldComponentTick();
        }
    }
}
