using RimValiCore.RVR;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace AvaliMod
{
    [DefOf]
    public static class AvaliDefs
    {

        public static FactionDef RimValiPlayerColony;

        public  static bool IsAvali(Pawn pawn)
        {
            return AvaliRaces.Contains(pawn.def);
        }
         static AvaliDefs()
        {
            avaliDefs = DefDatabase<RimValiRaceDef>.AllDefs.Where(x => x.defName == "RimVali").ToList();
        }

        private static List<RimValiRaceDef> avaliDefs = new List<RimValiRaceDef>();

        public static List<RimValiRaceDef> AvaliRaces => avaliDefs;

        //TerrainAffordance defs
        public static TerrainAffordanceDef IcySoil;

        //Body & BodyTypeDefs
        public static BodyTypeDef Avali;
        public static BodyDef RimValiBody;

        //ThingDefs
        public static ThingDef AvaliNanoForge;
        public static ThingDef AvaliNanoLoom;
        public static ThingDef AvaliAerogel;
        public static ThingDef AvaliResearchBench;
        public static ThingDef AvaliNexus;

        public static ThingDef Aerial;

        //Trait defs
        public static TraitDef AvaliPackBroken;


        //Stat defs
        public static StatDef ExplodeBombRadius;
        public static StatDef ExplodeFireRadius;
        public static StatDef ExplodeEMPRadius;

        public static StatDef KillOnExplosion;

        //Some vanilla incidents
        public static IncidentDef Flashstorm;
        public static IncidentDef ShortCircuit;
        public static IncidentDef HeatWave;
        public static IncidentDef CropBlight;
        public static IncidentDef Alphabeavers;
        public static IncidentDef PsychicDrone;
        public static IncidentDef WildManWandersIn;
        public static IncidentDef HerdMigration;
        public static IncidentDef MeteoriteImpact;
        public static IncidentDef RansomDemand;
        public static IncidentDef ThrumboPasses;
        public static IncidentDef SelfTame;
        public static IncidentDef ResourcePodCrash;
        public static IncidentDef AmbrosiaSprout;
        public static IncidentDef FarmAnimalsWanderIn;
        public static IncidentDef WandererJoin;
        public static IncidentDef RefugeePodCrash;

        public static IncidentDef VolcanicWinter;

        //A vanilla category I need
        public static ThingCategoryDef BuildingsSpecial;

        //HediffDefs
        public static HediffDef TestHediffOne;
        public static HediffDef TestHediffTwo;
        public static HediffDef TestHediffThree;

        //Letters
        public static LetterDef IlluminateAirdrop;

        //Faction defs
        public static FactionDef AvaliFaction;
        public static FactionDef NesiSpecOps;

        //Research defs
        public static ResearchProjectDef AvaliAdvancedGuns;
        public static ResearchProjectDef AvaliAdvancedMelee;
        public static ResearchProjectDef AeroWeaveResearch;

        public static ResearchProjectDef AvaliAeroTungsten;
        //Job defs

        public static JobDef RefuelAerial;

        public static ThoughtDef KnowButcheredHumanlikeCorpse;
        public static ThoughtDef ButcheredHumanlikeCorpse;
        public static ThoughtDef AvaliSharedBedRoom;
        public static ThoughtDef AvaliSleptAlone;
        public static ThoughtDef AvaliPackLoss;

        public static HairDef RVRNoHair;

        //Pawn Kinds
        public static PawnKindDef RimValiColonist;

        //Interaction Defs
        public static InteractionDef DeclinePackJoin;

        //ThingCategoryDefs
        public static ThingCategoryDef AvaliResources;
    }
}
