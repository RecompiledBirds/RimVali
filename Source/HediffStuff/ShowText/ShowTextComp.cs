using System;
using System.Collections.Generic;
using RimWorld;
using Verse;

namespace AvaliMod
{
    public class ShowTextComp : HediffComp
    {
        private readonly Random random = new Random();

        public ShowTextProps Props => (ShowTextProps)props;

        private List<ShowTextClass> ShowTexts => Props.showText;

        public void SpawnText(Pawn pawn,
            string textToShow,
            bool mustBeAwake,
            bool genderLock,
            Gender gender,
            int timeToFade,
            bool mustBeAsleep)
        {
            if (pawn.Awake() && mustBeAwake && !mustBeAsleep)
            {
                if (genderLock && pawn.gender == gender)
                {
                    MoteMaker.ThrowText(pawn.Position.ToVector3ShiftedWithAltitude(AltitudeLayer.Blueprint), pawn.Map,
                        textToShow, timeToFade);
                }
                else if (!genderLock)
                {
                    MoteMaker.ThrowText(pawn.Position.ToVector3ShiftedWithAltitude(AltitudeLayer.Blueprint), pawn.Map,
                        textToShow, timeToFade);
                }
            }
            else if (mustBeAsleep && !pawn.Awake())
            {
                if (genderLock && pawn.gender == gender)
                {
                    MoteMaker.ThrowText(pawn.Position.ToVector3ShiftedWithAltitude(AltitudeLayer.Blueprint), pawn.Map,
                        textToShow, timeToFade);
                }
                else if (!genderLock)
                {
                    MoteMaker.ThrowText(pawn.Position.ToVector3ShiftedWithAltitude(AltitudeLayer.Blueprint), pawn.Map,
                        textToShow, timeToFade);
                }
            }
            else if (!mustBeAsleep && !mustBeAwake)
            {
                if (genderLock && pawn.gender == gender)
                {
                    MoteMaker.ThrowText(pawn.Position.ToVector3ShiftedWithAltitude(AltitudeLayer.Blueprint), pawn.Map,
                        textToShow, timeToFade);
                }
                else if (!genderLock)
                {
                    MoteMaker.ThrowText(pawn.Position.ToVector3ShiftedWithAltitude(AltitudeLayer.Blueprint), pawn.Map,
                        textToShow, timeToFade);
                }
            }
            else if (mustBeAsleep && mustBeAwake)
            {
                Log.Warning("mustBeAsleep and mustBeAwake are both true, the text will never be shown.");
            }
        }

        public void ShowText(List<ShowTextClass> showTextClasses, int item)
        {
            string textToShow = showTextClasses[item].textToShow;
            float randomDays = showTextClasses[item].randomDays;
            int timeToFade = showTextClasses[item].timeToFade;
            bool genderLock = showTextClasses[item].genderLocked;
            Gender gender = showTextClasses[item].gender;
            float mtbUnit = showTextClasses[item].mtbUnit;
            float checkDuration = showTextClasses[item].checkDuration;
            bool mustBeAwake = showTextClasses[item].mustBeAwake;
            bool mustBeAsleep = showTextClasses[item].mustBeAsleep;
            bool mustBeSharingRoomWithPack = showTextClasses[item].mustBeSharingRoomWithPack;
            PawnRelationDef relationDef = showTextClasses[item].relationDef;

            Pawn pawn = parent.pawn;
            Map map = pawn.Map;
            if (pawn.Spawned)
            {
                if (!map.moteCounter.SaturatedLowPriority)
                {
                    if (!Rand.MTBEventOccurs(randomDays, mtbUnit, checkDuration))
                    {
                        return;
                    }

                    if (pawn.Position.ShouldSpawnMotesAt(pawn.Map))
                    {
                        if (mustBeSharingRoomWithPack && pawn.PackMatesInRoom())
                        {
                            SpawnText(pawn, textToShow, mustBeAwake, genderLock, gender, timeToFade, mustBeAsleep);
                        }
                        else if (!mustBeSharingRoomWithPack)
                        {
                            SpawnText(pawn, textToShow, mustBeAwake, genderLock, gender, timeToFade, mustBeAsleep);
                        }
                    }
                }
            }
        }


        public override void CompPostTick(ref float severityAdjustment)
        {
            int itemToGet = random.Next(0, ShowTexts.Count);
            if (LoadedModManager.GetMod<RimValiMod>().GetSettings<RimValiModSettings>().textEnabled &&
                random.Next(0, 5) == 4)
            {
                ShowText(ShowTexts, itemToGet);
            }
        }
    }
}
