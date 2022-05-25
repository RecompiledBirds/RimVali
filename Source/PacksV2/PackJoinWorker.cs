using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using AvaliMod;
namespace Rimvali.Rewrite.Packs
{
    public class PackJoinWorker : InteractionWorker
    {
        public static void AddPostJoinAction(Pawn joiner, Pack packJoined)
        {

        }
        
        public override void Interacted(Pawn initiator, Pawn recipient, List<RulePackDef> extraSentencePacks, out string letterText, out string letterLabel, out LetterDef letterDef, out LookTargets lookTargets)
        {
            PacksV2WorldComponent packsComp = Find.World.GetComponent<PacksV2WorldComponent>();
            base.Interacted(initiator, recipient, extraSentencePacks, out letterText, out letterLabel, out letterDef, out lookTargets);
            Pack pack = packsComp.GetPack(initiator);
            //Get pack avg opinion of recipient.
            float avgOP = pack.GetAvgOpinionOf(recipient);
            //Get recipient's opinion of the pack.
            float recipientOpinion = pack.GetPawnOpinionOf(recipient);
            //Get the current date.
            var date = new Date();
            bool joined = false;
            //Important for day 0 pack creation!
            if (date.ToString() == pack.CreationDate.ToString())
            {
                packsComp.JoinPawnToPack(recipient, ref pack);
                joined = true;
            }
            //Else, check the pack's average opinion of recipient and recipient's opinion of the pack.
            else if (avgOP >= RimValiMod.settings.packOpReq && recipientOpinion >= RimValiMod.settings.packOpReq)
            {
                packsComp.JoinPawnToPack(recipient, ref pack);
                joined = true;
            }
            
            //If they joined, notify the user
            if (joined)
            {
                letterDef = LetterDefOf.PositiveEvent;

                lookTargets = new LookTargets(targets: new List<Pawn> { recipient, initiator });
                letterLabel = "AvaliPackJoinLabel".Translate(recipient.Name.ToStringShort.Named("JOINER"),initiator.Name.ToStringShort.Named("INVITER"));
                letterText = "AvaliPackJoinDescription".Translate(recipient.Name.ToStringShort.Named("JOINER"), initiator.Name.ToStringShort.Named("INVITER"), pack.Name.Named("PACK"));
            }

            //An interaction that simply serves as a little bit of flavour text when a recipient does not like the pack.
            if(recipientOpinion < RimValiMod.settings.packOpReq)
                recipient.interactions.TryInteractWith(initiator, AvaliDefs.DeclinePackJoin);
        }

        private readonly SimpleCurve PackCurve = new SimpleCurve
        {
            new CurvePoint(-1, 0),
            new CurvePoint(0, 0.4f),
            new CurvePoint(0.5f, 1),
            new CurvePoint(1, 1.5f)
        };

        public override float RandomSelectionWeight(Pawn initiator, Pawn recipient)
        {
            PacksV2WorldComponent packsComp = Find.World.GetComponent<PacksV2WorldComponent>();
            bool initatiorPackHasSpace = packsComp.PawnHasPack(initiator) && packsComp.GetPack(initiator).GetAllPawns.Count < RimValiMod.settings.maxPackSize;
            bool bothAreAvali = AvaliDefs.IsAvali(initiator) && AvaliDefs.IsAvali(recipient);
            
            //Check both are avali, unstable mode is on, initator has a pack, recipient does not, both are of the same faction, and initator's pack has space.
            if (recipient.IsPackBroken() || packsComp.PawnHasPack(recipient) || !bothAreAvali || !PacksV2WorldComponent.EnhancedMode || initiator.Faction != recipient.Faction || !initatiorPackHasSpace)
                return 0f;
            
            //This is important for day 0 pack creation.
            //Recipient does not have pack, initiator does, initator's pack is less then max size, and the creation date is today.
            if (bothAreAvali && packsComp.GetPack(initiator).CreationDate.ToString() == new Date().ToString() && initatiorPackHasSpace)
                return 10000f;
           
            return PackCurve.Evaluate(initiator.relations.CompatibilityWith(recipient));
        }
    }
}
