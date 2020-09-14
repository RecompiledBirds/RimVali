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
            List<ThingDef> races = RimvaliPotentialPackRaces.potentialPackRaces.ToList<ThingDef>();
            IEnumerable<Pawn> pawns = RimValiUtility.CheckAllPawnsInMapAndFaction(map, Faction.OfPlayer);
            foreach(Pawn pawn in pawns)
            {
                if (pawn.IsHashIntervalTick(120))
                {
                    AvaliThoughtDriver avaliThoughtDriver = pawn.TryGetComp<AvaliThoughtDriver>();
                    PackComp packComp = pawn.TryGetComp<PackComp>();
                    if(!(avaliThoughtDriver == null)) {
                        if (pawn.def == AvaliDefs.RimVali)
                        {
                            if (RimValiUtility.GetPackSize(pawn, avaliThoughtDriver.Props.relationDef) > 1)
                            {
                                PawnRelationDef relationDef = avaliThoughtDriver.Props.relationDef;
                                UpdateSharedRoomThought(pawn, relationDef, avaliThoughtDriver.Props.inSameRoomThought);
                                UpdateBedRoomThought(pawn, relationDef, avaliThoughtDriver.Props.sharedBedroomThought, avaliThoughtDriver.Props.sleptApartThought);
                            }

                        }
                    }if (!(packComp == null))
                    {
                        if (packComp.Props.canHaveAloneThought)
                        {
                            UpdateThought(pawn, packComp.Props.relation, packComp.Props.aloneThought);
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