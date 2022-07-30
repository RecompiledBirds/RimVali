﻿using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using AvaliMod;
using RimValiCore;

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
            if(!packsComp.PawnHasPackWithMembers(recipient) && packsComp.PawnHasPack(recipient))
                packsComp.RemovePack(packsComp.GetPack(recipient));

            Pack pack = packsComp.GetPack(initiator);
            //Get pack avg opinion of recipient.
            float avgOP = pack.GetAvgOpinionOf(recipient);
            //Get recipient's opinion of the pack.
            float recipientOpinion = pack.GetPawnOpinionOf(recipient);
            //Get the current date.
            var date = new RimworldDate();
            bool joined = false;

            //Important for day 0 pack creation!
            bool isDayZero = date.ToString() == pack.CreationDate.ToString();
            if (isDayZero)
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
            if (joined && initiator.Faction==Faction.OfPlayer&&recipient.Faction==Faction.OfPlayer)
            {
                letterDef = LetterDefOf.PositiveEvent;

                lookTargets = new LookTargets(targets: new List<Pawn> { recipient, initiator });
                letterLabel = "AvaliPackJoinLabel".Translate(recipient.Name.ToStringShort.Named("JOINER"),initiator.Name.ToStringShort.Named("INVITER"));
                letterText = "AvaliPackJoinDescription".Translate(recipient.Name.ToStringShort.Named("JOINER"), initiator.Name.ToStringShort.Named("INVITER"), pack.Name.Named("PACK"));
            }

            //An interaction that simply serves as a little bit of flavour text when a recipient does not like the pack.
            //Day 0 check to prevent confusing messages
            if(recipientOpinion < RimValiMod.settings.packOpReq && !isDayZero)
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
            

            if (recipient.IsPackBroken() || packsComp.PawnHasPackWithMembers(recipient) || !bothAreAvali || !PacksV2WorldComponent.EnhancedMode || initiator.Faction != recipient.Faction || !initatiorPackHasSpace)
                return 0f;
            

            if (bothAreAvali && packsComp.GetPack(initiator).CreationDate.ToString() == new RimworldDate().ToString() && initatiorPackHasSpace)
                return 10000f;
           
            return PackCurve.Evaluate(initiator.relations.CompatibilityWith(recipient));
        }
    }
}
