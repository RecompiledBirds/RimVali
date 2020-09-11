using Verse;

namespace AvaliMod
{
    public class AddHediff : HediffGiver
    {
        public BodyPartDef bodyPart;
        public override void OnIntervalPassed(Pawn pawn, Hediff cause)
        {
            if (pawn.Spawned == true)
            {
                if (pawn.health.hediffSet.HasHediff(this.hediff, false))
                {
                    return;
                }
                BodyPartRecord bodyPartRecord = pawn.RaceProps.body.GetPartsWithDef(bodyPart).RandomElement<BodyPartRecord>();
                pawn.health.AddHediff(hediff, bodyPartRecord);
                Log.Message("Added hediff " + this.hediff + " to pawn " + pawn.Name.ToString());
            }
            else
            {
                return;
            }
        }
    }
}
