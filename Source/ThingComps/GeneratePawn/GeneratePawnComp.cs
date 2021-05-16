using RimWorld;
using Verse;
using System.Collections.Generic;
using System.Linq;
namespace AvaliMod
{

    public class AvaliTransfer : ThingComp
    {
        private List<PawnRelationDef> relations = new List<PawnRelationDef>();
        private List<Pawn> pawns = new List<Pawn>();


        public Pawn PawnToUpload()
        {
            ThingWithComps parent = this.parent;
            List<Pawn> pawns = RimValiUtility.CheckAllPawnsInMapAndFaction(parent.Map, parent.Faction).ToList<Pawn>();
            foreach(Pawn pawn in pawns)
            {
                PhysicalInteractionReservationManager physicalInteractionReservationManager = new PhysicalInteractionReservationManager();
                if (physicalInteractionReservationManager.IsReservedBy(pawn, this.parent) == true){
                    return pawn;
                }
            }
            return null;
        }


       /* private void GetPawnRelations(Pawn pawn)
        {
            IEnumerable<Pawn> relatedPawnList = pawn.relations.RelatedPawns;
            foreach(Pawn relatedPawn in relatedPawnList)
            {
                foreach(PawnRelationDef relationDef in RimValiRelationsFound.relationsFound)
                {
                    if(relatedPawn.relations.DirectRelationExists(relationDef, pawn))
                    {
                        relations.Add(relationDef);
                        pawns.Add(relatedPawn);
                    }
                }
            }
        }

        private void TransferPawnRelations(Pawn pawn, Pawn newPawn)
        {
            GetPawnRelations(pawn);
            int onPawn = 0;
            while(onPawn < pawns.Count)
            {
                pawn.relations.RemoveDirectRelation(relations[onPawn], pawns[onPawn]);
                newPawn.relations.AddDirectRelation(relations[onPawn], pawns[onPawn]);
            }
            pawns.Clear();
            relations.Clear();
        }

        private void MakeNewPawn(Pawn pawn, PawnKindDef pawnKind)
        {
            PawnGenerationRequest newPawnGenerationRequest = new PawnGenerationRequest(pawnKind, Faction.OfPlayer);
            Pawn newPawn = PawnGenerator.GeneratePawn(newPawnGenerationRequest);
            TransferPawnRelations(pawn, newPawn);
        }*/

        public override void CompTick()
        {
            ThingWithComps parent = this.parent;
            if (parent.IsHashIntervalTick(10))
            {
                IntVec3 cell = parent.InteractionCell;
                Pawn pawn = PawnToUpload();
                if (!(pawn == null))
                {
                    if (pawn.Position == cell)
                    {

                    }
                }
            }

        }
    } 
}