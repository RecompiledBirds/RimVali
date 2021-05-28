using RimValiCore.RVR;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
namespace AvaliMod
{
    public static class ResExtenstion
    {
        public static bool allPreReqDone(this ResearchProjectDef d)
        {
            if (d.prerequisites != null)
            {
                if (d.prerequisites.Any(def => !def.IsFinished))
                {
                    return false;
                }
            }
            return true;
        }
    }
    public class HackingGameComp : WorldComponent
    {
        public List<string> techLvls;
        public Dictionary<ResearchProjectDef, bool> hackProjects = new Dictionary<ResearchProjectDef, bool>();
        public HackingGameComp(World world) : base(world)
        {
            techLvls = Enum.GetNames(typeof(TechLevel)).ToList();
        }

        public override void ExposeData()
        {
            Scribe_Collections.Look(ref hackProjects, "hackProjects");
            base.ExposeData();
        }

        public List<ResearchProjectDef> getAllUnhacked
        {
            get
            {
                List <ResearchProjectDef> list1 = DefDatabase<ResearchProjectDef>.AllDefs.Where(x => x.allPreReqDone() && !x.IsFinished
                 && !(hackProjects.ContainsKey(x) && hackProjects[x])
                 && DefDatabase<FactionResearchRestrictionDef>.AllDefs.Any(z =>
                     z.factionResearchRestrictionBlackList.Any(y => y.isHackable
                         && y.researchProj == x
                         && y.factionDef != Faction.OfPlayer.def)
                     || z.factionResearchRestrictions.Any(y => y.isHackable
                         && y.researchProj == x
                         && y.factionDef != Faction.OfPlayer.def))).ToList();
                return list1;
            }
        }


        public void SendMessage(ResearchProjectDef def)
        {
            ChoiceLetter choiceLetter = LetterMaker.MakeLetter("HackedProject".Translate(def.label.Named("PROJECT")), "UnlockedResearch".Translate(def.label.Named("PROJECT")), AvaliMod.AvaliDefs.IlluminateAirdrop);
            Find.LetterStack.ReceiveLetter(choiceLetter, null);
        }

        public void HackRes(ResearchProjectDef def)
        {
            if (def.allPreReqDone())
            {
                hackProjects.Add(def, true);
                SendMessage(def);
            }
        }
        float day = 60000;
        int tick;
        public override void WorldComponentTick()
        {
            if (Find.ResearchManager.currentProj != null)
            {

                tick++;
                if (tick == day*5)
                {
                    if (UnityEngine.Random.Range(0, 100) < RimValiMod.settings.hackChance)
                    {
                        ResearchProjectDef def = getAllUnhacked.RandomElement();
                        if (RimValiMod.settings.enableDebugMode)
                        {
                            Log.Message($"Availble projects for hacking: {getAllUnhacked.Count}");
                        }
                        if (def != null)
                        {
                            HackRes(def);
                        }
                    }
                    tick = 0;
                }
            }
            base.WorldComponentTick();
        }
    }
}