using Verse;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
namespace AvaliMod
{
    public class AvaliUpdater : MapComponent
    {
        private readonly bool mapCompOn = LoadedModManager.GetMod<RimValiMod>().GetSettings<RimValiModSettings>().mapCompOn;
        private readonly bool packLossEnabled = LoadedModManager.GetMod<RimValiMod>().GetSettings<RimValiModSettings>().packLossEnabled;
        private int onTick;
        public AvaliUpdater(Map map)
            : base(map)
        {

        }

        public void UpdateSharedRoomThought(Pawn pawn, PawnRelationDef relationDef, ThoughtDef thought)
        {
            if (pawn.Awake())
            {
                AvaliThoughtDriver avaliThoughtDriver = pawn.TryGetComp<AvaliThoughtDriver>();
                if (RimValiUtility.CheckIfPackmatesInRoom(pawn, relationDef))
                {
                    RimValiUtility.AddThought(pawn, thought);
                }
                else
                {
                    RimValiUtility.RemoveThought(pawn, thought);
                }
            }
        }

        public void UpdateBedRoomThought(Pawn pawn, PawnRelationDef relationDef, ThoughtDef togetherThought, ThoughtDef aloneThought)
        {
            if(RimValiUtility.CheckIfBedRoomHasPackmates(pawn, relationDef))
            {
                RimValiUtility.AddThought(pawn, togetherThought);
            }
            else
            {
                RimValiUtility.RemoveThought(pawn, aloneThought);
            }
        }

        public void UpdatePawns(Map map)
        {
            IEnumerable<Pawn> pawns = RimValiUtility.CheckAllPawnsInMapAndFaction(map, Faction.OfPlayer).Where(x => x.def == AvaliDefs.RimVali);
            IEnumerable<AvaliPack> packs = AvaliPackDriver.packs;
            foreach (Pawn pawn in pawns)
            {
                AvaliThoughtDriver avaliThoughtDriver = pawn.TryGetComp<AvaliThoughtDriver>();
                PackComp packComp = pawn.TryGetComp<PackComp>();
                if (!(avaliThoughtDriver == null))
                {
                    //Log.Message("Pawn has pack comp, moving to next step...");
                    if (AvaliPackDriver.packs == null || AvaliPackDriver.packs.Count == 0)
                    {
                        Log.Message("How did we get here? [Pack list was 0 or null]");
                        return;
                    }
                    AvaliPack pawnPack = null;
                    //This errors out when pawns dont have a pack. That is bad. This stops it from doing that.
                    try { pawnPack = RimValiUtility.GetPack(pawn); }
                    catch
                    {
                        return;
                    }
                    Log.Message("Tried to get packs pack, worked.");
                    if (pawnPack == null)
                    {
                        Log.Message("How did we get here? [Pack was null.]");
                    }
                    foreach (Pawn packmate in pawnPack.pawns)
                    {
                        Thought_Memory thought_Memory2 = (Thought_Memory)ThoughtMaker.MakeThought(AvaliDefs.AvaliPackmateThought);
                        if (!(packmate == pawn))
                        {
                            bool bubble;
                            if (!thought_Memory2.TryMergeWithExistingMemory(out bubble))
                            {
                                Log.Message("Adding thought to pawn.");
                                pawn.needs.mood.thoughts.memories.TryGainMemory(thought_Memory2, packmate);
                            }
                        }
                    }

                }
            }
        }


        public void RemoveThought(ThoughtDef thought, PawnRelationDef relationDef, Pawn pawn)
        {
            if (RimValiUtility.GetPackSize(pawn, relationDef) > 1 && packLossEnabled)
            {
                RimValiUtility.RemoveThought(pawn, thought);
            }
        }

        public void AddThought(ThoughtDef thought, PawnRelationDef relationDef, Pawn pawn)
        {
            if (RimValiUtility.GetPackSize(pawn, relationDef) == 1 && packLossEnabled)
            {
                RimValiUtility.AddThought(pawn, thought);
            }
        }

        public void UpdateThought(Pawn pawn, PawnRelationDef relationDef, ThoughtDef thought)
        {
            AddThought(thought, relationDef, pawn);
            RemoveThought(thought, relationDef, pawn);
        }
        public override void MapComponentTick()
        {
            if (mapCompOn && !(AvaliPackDriver.packs == null) && AvaliPackDriver.packs.Count > 0)
            {
                if (onTick == 120)
                {
                    Map map = this.map;
                    UpdatePawns(map);
                    onTick = 0;
                }
                else
                {
                    onTick += 1;
                }
            }
        }

    }
}