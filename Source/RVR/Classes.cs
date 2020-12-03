using RimWorld;
using Verse;
using System.Collections.Generic;
namespace AvaliMod
{
    public class graphics
    {
        public string bodyTex;
        public string headTex;
        public string skeleton = "Things/Pawn/Humanlike/HumanoidDessicated";
        public string skull = "Things/Pawn/Humanlike/Heads/None_Average_Skull";
        public string stump = "Things/Pawn/Humanlike/Heads/None_Average_Stump";

    }

    public class restrictions
    {
        public List<ResearchProjectDef> researchProjectDefs = new List<ResearchProjectDef>();
        //ThingDefs
        public List<ThingDef> equippables = new List<ThingDef>();
        public List<ThingDef> consumables = new List<ThingDef>();
        public List<ThingDef> buildables = new List<ThingDef>();
        //Thoughts
        public List<ThoughtDef> thoughtDefs = new List<ThoughtDef>();
        //Traits
        public List<TraitDef> traits = new List<TraitDef>();


        //Are these whitelists?
        public bool researchProjectDefsIsWhiteList = false;
        public bool thoughtDefsIsWhiteList = false;
        public bool traitsIsWhiteList = false;

        //Whitelists
        public List<ThingDef> equippablesWhitelist = new List<ThingDef>();
        public List<ThingDef> consumablesWhitelist = new List<ThingDef>();
        public List<ThingDef> buildablesWhitelist = new List<ThingDef>();

    }

    public class Main
    {
        public restrictions restrictions= new restrictions();
        public graphics graphics = new graphics();

        public List<BodyTypeDef> bodyTypeDefs = new List<BodyTypeDef>();
    }
}