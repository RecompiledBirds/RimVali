using Verse;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
namespace AvaliMod
{
    public class AvaliUpdater : MapComponent
    {
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
                    if (pawn.def == AvaliDefs.RimVali)
                    {
                        if (RimValiUtility.GetPackSize(pawn, avaliThoughtDriver.Props.relationDef) > 0)
                        {
                            PawnRelationDef relationDef = avaliThoughtDriver.Props.relationDef;
                            UpdateSharedRoomThought(pawn, relationDef, avaliThoughtDriver.Props.inSameRoomThought);
                            UpdateBedRoomThought(pawn, relationDef, avaliThoughtDriver.Props.sharedBedroomThought, avaliThoughtDriver.Props.sleptApartThought);
                        }
                    }
                }
            }
        }
        public override void MapComponentTick()
        {
            Map map = this.map;
            UpdatePawns(map);
        }

    }
}