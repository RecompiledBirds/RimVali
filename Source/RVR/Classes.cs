using RimWorld;
using Verse;
using System.Collections.Generic;
using UnityEngine;

namespace AvaliMod
{
    public class raceOrganHarvestThought
    {
        public string organDefName = "any";
        public ThingDef race;
       
        public ThoughtDef thought;
    }
    public class raceButcherThought
    {
        public ThingDef race;
        public ThoughtDef butcheredPawnThought;
        public ThoughtDef knowButcheredPawn;
        public ThoughtDef ateThought;
        public ThoughtDef ateCookedThought;
    }

    public class butcherAndHarvestThoughts
    {
        public List<raceButcherThought> butcherThoughts;
        public List<raceOrganHarvestThought> harvestedThoughts;
        //If a race isnt defined in the above, it gets the default thought.
        public bool careAboutUndefinedRaces = true;
        public ThoughtDef myOrganHarvested = ThoughtDefOf.MyOrganHarvested;
    }

    public class graphics
    {
        public string skinColorSet;

        public string bodyTex;
        public string headTex;
        public string skeleton = "Things/Pawn/Humanlike/HumanoidDessicated";
        public string skull = "Things/Pawn/Humanlike/Heads/None_Average_Skull";
        public string stump = "Things/Pawn/Humanlike/Heads/None_Average_Stump";
        public headOffset headOffsets = new headOffset();
        public Vector2 headSize;
        public Vector2 bodySize = new Vector2(1f, 1f);
        public List<Colors> colorSets;
    }
    public class headOffset
    {
        public Vector2 south = new Vector2(0, 0);
        public Vector2 east;
        public Vector2 north;
        public Vector2 west;
        public headOffset()
        {
            if(east == null)
            {
                east = south;
            }if(north == null)
            {
                north = south;
            }if(west == null)
            {
                west = east;
            }
        }
    }
    public class restrictions
    {
        public List<ResearchProjectDef> researchProjectDefs = new List<ResearchProjectDef>();
        //ThingDefs
        public List<ThingDef> equippables = new List<ThingDef>();
        public List<ThingDef> consumables = new List<ThingDef>();
        public List<ThingDef> buildables = new List<ThingDef>();
        public List<ThingDef> bedDefs = new List<ThingDef>();
        //Thoughts
        public List<ThoughtDef> thoughtDefs = new List<ThoughtDef>();
        public List<ThoughtDef> thoughtBlacklist = new List<ThoughtDef>();

        //Traits
        public List<TraitDef> traits = new List<TraitDef>();
        public List<TraitDef> disabledTraits = new List<TraitDef>();

        //Bodytypes
        public List<BodyTypeDef> bodyTypes = new List<BodyTypeDef>();


        public bool canOnlyUseApprovedApparel = false;
        //Whitelists
        public List<ThingDef> equippablesWhitelist = new List<ThingDef>();
        public List<ThingDef> consumablesWhitelist = new List<ThingDef>();
        public List<ThingDef> buildablesWhitelist = new List<ThingDef>();

        //Mod lists

        //-Apparel
        public List<string> modContentRestrictionsApparelWhiteList = new List<string>();
        public List<string> modContentRestrictionsApparelList = new List<string>();
        //-Research
        public List<string> modResearchRestrictionsList = new List<string>();
        public List<string> modResearchRestrictionsWhiteList = new List<string>();
        //-Traits
        public List<string> modTraitRestrictions = new List<string>();
        public List<string> modDisabledTraits = new List<string>();
        //-Buildings
        public List<string> modBuildingRestrictions = new List<string>();
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

    public class ColorSet : IExposable
    {
        public Color colorOne = Color.white;
        public Color colorTwo = Color.white;
        public Color colorThree = Color.white;
        public bool dyeable = true;
        
        public ColorSet() { }
        public ColorSet(Color colorOne, Color colorTwo, Color colorThree, bool dyeable = true)
        {
            this.colorOne = colorOne;
            this.colorTwo = colorTwo;
            this.colorThree = colorThree;
            this.dyeable = dyeable;
        }

        public void ExposeData()
        {
            Scribe_Values.Look(ref colorOne, "colorOne");
            Scribe_Values.Look(ref colorTwo, "colorTwo");
            Scribe_Values.Look(ref colorThree, "colorThree");
        }
    }

    public class Colors
    {
        public string name;
        public TriColor_ColorGenerators colorGenerator;
        public TriColor_ColorGenerators colorGeneratorFemale;
        public bool isDyeable = true;
        public bool isGendered = false;

        public TriColor_ColorGenerators Generator(Pawn pawn)
        {
            if(this.isGendered==true && pawn.gender == Gender.Female)
            {
                if(colorGeneratorFemale != null)
                {
                    return colorGeneratorFemale;
                }
            }
            return colorGenerator;
        }
    }

    public class TriColor_ColorGenerators
    {
        public ColorGenerator firstColor;
        public ColorGenerator secondColor;
        public ColorGenerator thirdColor;
    }

    public class colorComp : ThingComp
    {
        public Dictionary<string, ColorSet> colors = new Dictionary<string, ColorSet>();
        public List<string> colorKey = new List<string>();
        public List<ColorSet> colorValue = new List<ColorSet>();


        public Dictionary<string, int> renderableDefIndexes = new Dictionary<string, int>();
        public List<string> renderableKeys = new List<string>();
        public List<int> index = new List<int>();

        public override void PostExposeData()
        {
            Scribe_Collections.Look(ref colors, "colors", LookMode.Value, LookMode.Deep, ref colorKey, ref colorValue);
            Scribe_Collections.Look(ref renderableDefIndexes, "renderables", LookMode.Value, LookMode.Deep, ref renderableKeys, ref index);
           
        }
    }
    public class colorCompProps : CompProperties
    {
        public colorCompProps()
        {
            this.compClass = typeof(colorComp);
        }
    }
    public class Entry
    {
        public PawnKindDef pawnkind;
        public int chance = 50;
        public bool isSlave = true;
        public bool isRefugee = true;
        public bool isWanderer = true;
        public bool isVillager = true;
    }
    public class RVRRaceInsertion
    {
        public int globalChance = 50;
        public List<Entry> entries = new List<Entry>();
    }
}  