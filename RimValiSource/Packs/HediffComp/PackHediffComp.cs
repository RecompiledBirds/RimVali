using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;
namespace AvaliMod
{
    public class PackMateComp : HediffComp
    {
        //Ask verse for a few settings.
        private readonly bool packsEnabled = LoadedModManager.GetMod<RimValiMod>().GetSettings<RimValiModSettings>().packsEnabled;
        private readonly bool packLossEnabled = LoadedModManager.GetMod<RimValiMod>().GetSettings<RimValiModSettings>().packLossEnabled;
        private readonly bool checkOtherRaces = LoadedModManager.GetMod<RimValiMod>().GetSettings<RimValiModSettings>().checkOtherRaces;
        private readonly bool allowAllRaces = LoadedModManager.GetMod<RimValiMod>().GetSettings<RimValiModSettings>().allowAllRaces;
        private int packMemberCountHistory;
        //the bool for making intial thoughts
        private bool hasMadeInitalPack = false;
        //the stage of the "alone" thought.
        private int thoughtStage;
        //have we checked for other races with "avali" in the name?
        private bool hasRunRaceCheck = false;

        //Pull our info from xml.
        public PackProps Props
        {
            get
            {
                return (PackProps)this.props;
            }
        }

        //Get a list of races in packs (defined in xml)
        private List<ThingDef> RacesInPacks
        {
            get
            {
                return Props.racesInPacks;
            }
        }
        //get the pack limit size (defined in xml)
        private int PackLimit
        {
            get
            {
                if (!allowSettingsOverride)
                {
                    return Props.maxMembers;
                }
                else
                {
                    return LoadedModManager.GetMod<RimValiMod>().GetSettings<RimValiModSettings>().maxPackSize;
                }
            }
        }

        //can they have a thought for being alone? (in xml)
        private bool CanHaveAloneThought
        {
            get
            {
                return Props.canHaveAloneThought;
            }
        }
        //what is the thought referred to as? (xml)
        private ThoughtDef AloneThought
        {
            get
            {
                return Props.aloneThought;
            }
        }

        //the "pack" relation we want. (xml)
        private PawnRelationDef RelationDef
        {
            get
            {
                return Props.relation;
            }
        }

        //A curve of age gen over time. (xml)
        private SimpleCurve AgeCurve
        {
            get
            {
                return Props.packGenChanceOverAge;
            }
        }

        //Pull a list from the races rimvali found during loading
        private List<ThingDef> OtherRaces
        {
            get
            {
                return RimvaliPotentialPackRaces.potentialPackRaces.ToList<ThingDef>();
            }
        }

        //Can the player's settings override the functions here?
        private bool allowSettingsOverride
        {
            get
            {
                return Props.allowOverride;
            }
        }

        //insert those races
        private void AddOtherRaces()
        {
            if (allowSettingsOverride)
            {
                RimvaliPotentialPackRaces.GetRaces();
                if (checkOtherRaces)
                {
                    foreach (ThingDef race in OtherRaces)
                    {
                        RacesInPacks.Add(race);
                    }
                }
                if (allowAllRaces)
                {
                    IEnumerable<ThingDef> AllRaces = RimvaliPotentialPackRaces.potentialRaces.ToList<ThingDef>();
                    foreach (ThingDef race in AllRaces)
                    {
                        RacesInPacks.Add(race);
                    }
                }
            }
            hasRunRaceCheck = true;
        }

        //Get a count of previous packmates.
        public override void CompExposeData()
        {
            Scribe_Values.Look<int>(ref packMemberCountHistory, "packMemberCountHistory", 0, true);
        }

        //Check if we've had a pack
        private bool CheckIfHadPack(Pawn pawn)
        {
            if (packMemberCountHistory > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        //Check if we can remove the thought.
        public void RemoveThought(Pawn pawn, PawnRelationDef relationDef, ThoughtDef thought)
        {
            if (RimValiUtility.GetPackSize(pawn, relationDef) > 0 || packLossEnabled == false)
            {
                pawn.needs.mood.thoughts.memories.RemoveMemoriesOfDef(thought);
            }
        }

        //Add the thought.

        public static void AddThought(Pawn pawn, PawnRelationDef relationDef, ThoughtDef thought)
        {
            //Get a "list" of all pawns on the pawns's faction
            IEnumerable<Pawn> pawns = PawnsFinder.AllMaps_SpawnedPawnsInFaction(pawn.Faction);
            int packmates = 0;
            foreach (Pawn packmate in pawns)
            {
                //if a packmate relation exists, add one.
                if (pawn.relations.DirectRelationExists(relationDef, packmate))
                {
                    packmates = +1;
                }
            }
            //if 0 packmates, try to add a thought with a stage.
            if (packmates <= 0)
            {
                RimValiUtility.AddThought(pawn, thought);
            }

        }

        //Try to call the pack making function over time. Random.
        public void PackOverTime(Pawn pawn, float age)
        {
            float x = (float)age / pawn.RaceProps.lifeExpectancy;
            _ = (float)(((double)pawn.ageTracker.AgeBiologicalYearsFloat));
            if ((double)Rand.Value < AgeCurve.Evaluate(x))
            {
                MakePack();
            }
        }


        //Update the alone thought.
        public void UpdateAloneThought(ThoughtDef thought, Pawn pawn)
        {
            //If the pack size is at 0 or somehow below, the race can have an alone though, and pack loss is enabled
            //then check that alonethought isnt null, and check if the pawn has had a pack.
            //if it is, add the loss thought.
            if (RimValiUtility.GetPackSize(pawn, RelationDef) <= 1 && CanHaveAloneThought && packLossEnabled)
            {
                if (!(AloneThought == null))
                {
                    AddThought(pawn,    RelationDef, thought);
                }
            }
            else
            {
                RemoveThought(pawn, RelationDef, thought);
            }
        }

        //The most important function here.
        public void MakePack(Pawn pawn = null)
        {

            //If a pawn override was not set, pick the pawn this comp is on.
            if (pawn == null)
            {
                pawn = this.parent.pawn;
            }

            //Get a list of all pawns in the faction.
            IEnumerable<Pawn> packMates = PawnsFinder.AllMaps_SpawnedPawnsInFaction(pawn.Faction);
            //if the pawn is spawned, start than runs until we've checked all packmates.
            if (pawn.Spawned)
            {
                foreach (Pawn packmate in packMates)
                {
                    //if they dont have the relation, continue. 
                    if (!pawn.relations.DirectRelationExists(RelationDef, packmate))
                    {
                        //Get our second packmate.
                        foreach (Pawn packmate2 in packMates)
                        {
                            //checks to make sure they aren't the same.
                            if (!(packmate == packmate2))
                            {
                                //make sure the packmates are of our race.
                                foreach (ThingDef race in RacesInPacks)
                                {
                                    if (packmate2.def.defName == race.defName)
                                    {
                                        foreach (ThingDef race2 in RacesInPacks)
                                        {
                                            if (packmate.def.defName == race2.defName)
                                            {
                                                //check if the second packmate has the relation, if they dont, continue
                                                if (!(packmate2.relations.DirectRelationExists(RelationDef, packmate)))
                                                {
                                                    //Check that a pack isn't too big.
                                                    if ((RimValiUtility.GetPackSize(packmate, RelationDef) < PackLimit) && ((RimValiUtility.GetPackSize(packmate2, RelationDef) < PackLimit)))
                                                    {
                                                        //add the relations, then have rimvali utilities collect them all to make the group.
                                                        packmate2.relations.AddDirectRelation(RelationDef, packmate);
                                                        packmate.relations.AddDirectRelation(RelationDef, packmate2);
                                                        RimValiUtility.CollectPackmates(packmate, packmate2, RelationDef);
                                                    }

                                                }
                                                //Removes dead or destroyed/null pawns.
                                                RimValiUtility.RemovePackRelationIfDead(pawn, packmate, packmate2, RelationDef);
                                                //Did a pack somehow get bigger than intended?
                                                RimValiUtility.TrimPack(packmate, packmate2, RelationDef, null, PackLimit);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        //Updates the pack
        public void UpdatePack()
        {
            //Get a list of all packmates from rimvali utilities.
            IEnumerable<Pawn> packmates = RimValiUtility.GetPackPawns(this.parent.pawn, RelationDef);
            //for each pawn found
            foreach (Pawn packmate in packmates)
            {
                //get another pawn
                foreach (Pawn packmate2 in packmates)
                {
                    //if they arent each other
                    if (!(packmate == packmate2))
                    {
                        //Have Rimvali utilities check they arent dead or null/destroyed
                        RimValiUtility.RemovePackRelationIfDead(this.parent.pawn, packmate, packmate2, RelationDef);
                    }
                }
            }
        }

        //makes the starting pack
        public void MakeInitialPack()
        {
            //if hasMadeIntialPack is false
            if (!hasMadeInitalPack)
            {
                //call MakePack, and set it to true.
                MakePack();
                hasMadeInitalPack = true;
            }
        }
        //run every tick
        public override void CompPostTick(ref float severityAdjustment)
        {
            //check the plater has packs enabled
            if (packsEnabled)
            {
                //run race check- AddOtherRaces will add jaffers or any other "avali"
                if (!hasRunRaceCheck)
                {
                    AddOtherRaces();
                }
                //call make intial pack.
                MakeInitialPack();
                //if time has passed
                if (this.parent.pawn.IsHashIntervalTick(60))
                {
                    //call PackOverTime with the pawn, and its age.
                    PackOverTime(this.parent.pawn, this.parent.pawn.ageTracker.AgeBiologicalYearsFloat);
                    //Update the loss thought.
                    UpdateAloneThought(AloneThought, this.parent.pawn);
                }
                //Makes sure the entire pack is updated.
                UpdatePack();
            }
        }
    }
}