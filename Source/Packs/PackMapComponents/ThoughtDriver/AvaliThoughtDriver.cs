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
                    AvaliPack pawnPack = RimValiUtility.GetPack(pawn);
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
                if (!(packComp == null))
                {
                    if (packComp.Props.canHaveAloneThought)
                    {
                        UpdateThought(pawn, packComp.Props.relation, packComp.Props.aloneThought);
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
            if (mapCompOn)
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