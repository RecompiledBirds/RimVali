using RimWorld;
using Verse;
namespace AvaliMod
{
    [DefOf]
    public static class AvaliDefs
    {
        //Body & BodyTypeDefs
        public static BodyTypeDef Avali;
        public static BodyDef RimValiBody;

        //ThingDefs
        public static ThingDef AvaliNanoForge;
        public static ThingDef AvaliNanoLoom;
        public static ThingDef RimVali;
        public static ThingDef AvaliAerogel;
        public static ThingDef AvaliResearchBench;

        //Stat defs
        public static StatDef ExplodeBombRadius;
        public static StatDef ExplodeFireRadius;
        public static StatDef ExplodeEMPRadius;
        public static StatDef KillOnExplosion;



        //Relation Defs
        public static PawnRelationDef Packmate;
        public static PawnRelationDef PackLeader;

        //HediffDefs
        public static HediffDef TestHediffOne;
        public static HediffDef TestHediffTwo;
        public static HediffDef TestHediffThree;


        //Thoughts
        public static ThoughtDef AvaliSharedBedRoom;
        public static ThoughtDef AvaliSleptWithoutPack;
        public static ThoughtDef AvaliSharingRoom;

        //Letters
        public static LetterDef IlluminateAirdrop;
    }
}
