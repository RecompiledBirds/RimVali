using RimWorld;
using Verse;
using System.Collections.Generic;
using UnityEngine;

namespace AvaliMod
{
    public class graphics
    {
        public string skinColorSet;

        public string bodyTex;
        public string headTex;
        public string skeleton = "Things/Pawn/Humanlike/HumanoidDessicated";
        public string skull = "Things/Pawn/Humanlike/Heads/None_Average_Skull";
        public string stump = "Things/Pawn/Humanlike/Heads/None_Average_Stump";
        public Vector2 headSize = new Vector2(1f, 1f);
        public Vector2 bodySize = new Vector2(1f, 1f);
        public List<Colors> colorSets;
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
        public List<ThoughtDef> thoughtBlacklist = new List<ThoughtDef>();

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

    public class BodyPartGraphicPos
    {
        public Vector2 position = new Vector2(0f,0f);
        public float layer = 1f;
        public Vector2 size = new Vector2(1f,1f);
      
    }

    public class ColorSet
    {
        public Color colorOne;
        public Color colorTwo;
        public Color colorThree;

        public ColorSet(Color colorOne, Color colorTwo, Color colorThree)
        {
            this.colorOne = colorOne;
            this.colorTwo = colorTwo;
            this.colorThree = colorThree;
        }
    }

    public class Colors
    {
        public string name;
        public TriColor_ColorGenerators colorGenerator;
    }

    public class TriColor_ColorGenerators
    {
        public ColorGenerator firstColor;
        public ColorGenerator secondColor;
        public ColorGenerator thirdColor;
    }
    
}  