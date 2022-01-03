using System.Collections.Generic;
using RimWorld;
using Verse;

namespace AvaliMod
{
    public class MultiPartBionic : HediffComp
    {
        private BodyPartDef bodyPart;
        private int hediffsFound;
        private int onItem;
        private string output;
        private bool triggeredTextAdd;
        private bool triggeredTextRemove;

        public MultiPartBionicCompProperties Props => (MultiPartBionicCompProperties)props;

        private string hediffsLeft => Props.stringForHediffsLeft;

        private bool displayTextWhenChanged => Props.displayTextWhenChanged;

        private List<BodyPartDef> bodyParts => Props.bodyPartsToAffect;

        private List<HediffDef> otherHediffs => Props.otherHediffs;

        private List<HediffDef> hediffsToAdd => Props.hediffsToAdd;

        private string textOnAdd => Props.textOnAdd;

        private string textOnRemove => Props.textOnRemove;

        private int timeToFade => Props.timeToFade;

        private List<BodyPartDef> bodyPartsMustBeOn => Props.bodyPartsMustBeOn;

        public override string CompTipStringExtra
        {
            get
            {
                Pawn pawn = parent.pawn;

                output = hediffsLeft;
                foreach (HediffDef hediffDef in otherHediffs)
                {
                    if (!pawn.health.hediffSet.HasHediff(hediffDef))
                    {
                        if (onItem + 1 >= otherHediffs.Count)
                        {
                            output = output + hediffDef.label + ". ";
                        }
                        else
                        {
                            output = output + hediffDef.label + ", ";
                        }
                    }
                }

                return output;
            }
        }

        public override void CompPostTick(ref float severityAdjustment)
        {
            Pawn pawn = parent.pawn;
            if (pawn.Spawned)
            {
                hediffsFound = 0;
                foreach (HediffDef hediff in otherHediffs)
                {
                    if (onItem <= bodyPartsMustBeOn.Count)
                    {
                        bodyPart = bodyPartsMustBeOn[onItem];
                    }
                    else
                    {
                        bodyPart = bodyPartsMustBeOn[bodyPartsMustBeOn.Count];
                    }

                    BodyPartRecord bodyPartRecord = pawn.RaceProps.body.GetPartsWithDef(bodyPart).RandomElement();
                    if (pawn.health.hediffSet.HasHediff(otherHediffs[onItem], bodyPartRecord))
                    {
                        hediffsFound += 1;
                    }
                }

                if (hediffsFound == otherHediffs.Count)
                {
                    foreach (HediffDef hediffDef in hediffsToAdd)
                    {
                        if (onItem <= bodyParts.Count)
                        {
                            bodyPart = bodyParts[onItem];
                        }
                        else
                        {
                            bodyPart = bodyParts[bodyParts.Count];
                        }

                        BodyPartRecord bodyPartRecord = pawn.RaceProps.body.GetPartsWithDef(bodyPart).RandomElement();
                        Hediff hediff = HediffMaker.MakeHediff(hediffDef, pawn);
                        if (!pawn.health.hediffSet.HasHediff(hediffDef))
                        {
                            pawn.health.AddHediff(hediff, bodyPartRecord);
                            Log.Message("Added hediff: " + hediffDef.defName + " to pawn " + hediff.pawn);
                            if (displayTextWhenChanged & !triggeredTextAdd)
                            {
                                triggeredTextAdd = true;
                                triggeredTextRemove = false;
                                MoteMaker.ThrowText(pawn.Position.ToVector3ShiftedWithAltitude(AltitudeLayer.Blueprint),
                                    pawn.Map, textOnAdd, timeToFade);
                            }
                        }

                        onItem += 1;
                    }
                }
                else
                {
                    hediffsFound = 0;
                    foreach (HediffDef hediffDef in otherHediffs)
                    {
                        if (pawn.health.hediffSet.HasHediff(hediffDef))
                        {
                            hediffsFound += 1;
                        }

                        onItem += 1;
                    }

                    if (!(hediffsFound == otherHediffs.Count))
                    {
                        foreach (HediffDef hediffDef in hediffsToAdd)
                        {
                            Hediff hediff = HediffMaker.MakeHediff(hediffDef, pawn);
                            if (pawn.health.hediffSet.HasHediff(hediffDef))
                            {
                                pawn.health.RemoveHediff(pawn.health.hediffSet.GetFirstHediffOfDef(hediffDef));
                                Log.Message("Removed hediff: " + hediffDef.defName + " from pawn " + hediff.pawn);
                                if (displayTextWhenChanged & !triggeredTextRemove)
                                {
                                    triggeredTextRemove = true;
                                    triggeredTextAdd = false;
                                    MoteMaker.ThrowText(
                                        pawn.Position.ToVector3ShiftedWithAltitude(AltitudeLayer.Blueprint), pawn.Map,
                                        textOnRemove, timeToFade);
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
