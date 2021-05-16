using System;
using Verse;
using RimWorld;

namespace AvaliMod
{
    // Token: 0x020000DC RID: 220
    public class DeathActionWorker_Test : DeathActionWorker
    {
<<<<<<< HEAD
=======
        AvaliPackDriver AvaliPackDriver = Current.Game.GetComponent<AvaliPackDriver>();
>>>>>>> beta
        private readonly bool enableDebug = LoadedModManager.GetMod<RimValiMod>().GetSettings<RimValiModSettings>().enableDebugMode;
        public override void PawnDied(Corpse corpse)
        {
            if (enableDebug)
            {
                Log.Message("Death detected");
            }
<<<<<<< HEAD
            if (corpse.InnerPawn.def.race == AvaliDefs.RimVali.race)
            {
=======
            if (corpse.InnerPawn.def.HasComp(typeof(PackComp)))
            {
               
>>>>>>> beta
                if (!(AvaliPackDriver.packs == null) && AvaliPackDriver.packs.Count > 0)
                {
                    if (enableDebug)
                    {
                        Log.Message("I got here, packs exist.");
                    }
                    Pawn pawn = corpse.InnerPawn;
                    foreach (AvaliPack pack in AvaliPackDriver.packs)
                    {
                        if (pack.pawns.Contains(pawn))
                        {
                            if (enableDebug)
                            {
                                Log.Message("The pawn had a pack");
                            }
<<<<<<< HEAD
                            DeathDate deathDate = new DeathDate();
=======
                            DeathDate deathDate = new DeathDate(pawn);
>>>>>>> beta
                            Log.Message(GenDate.DayOfYear(1, Find.WorldGrid.LongLatOf(corpse.Map.Tile).x).ToString());
                            deathDate.day = GenDate.DayOfYear(1, Find.WorldGrid.LongLatOf(corpse.Map.Tile).x);
                            if(corpse.InnerPawn != null && enableDebug)
                            {
                                Log.Message("Pawn is not null");
                            }
<<<<<<< HEAD
=======
                            
>>>>>>> beta
                            deathDate.deadPawn = pawn;
                            if (enableDebug)
                            {
                                Log.Message("I got the deathdate");
                                Log.Message(deathDate.day.ToString());
                            }
                            pack.deathDates.Add(deathDate);
<<<<<<< HEAD
                            pack.size--;
                            if (enableDebug)
                            {
                                Log.Message(pack.size.ToString());
                            }
                            foreach(Pawn packmate in pack.pawns)
                            {
                                if(!(pawn == packmate) && !pawn.Dead)
                                {
                                    PackComp comp = packmate.TryGetComp<PackComp>();
                                    if(comp.Props.deathThought != null)
                                    {
                                        packmate.needs.mood.thoughts.memories.TryGainMemory(comp.Props.deathThought);
=======
                            //pack.size--;
                            if (enableDebug)
                            {
                                Log.Message(pack.pawns.Count.ToString());
                            }
                            foreach(Pawn packmate in pack.pawns)
                            {
                                Log.Message("On: "+ packmate.Name.ToString());
                                if((pawn != packmate) && !packmate.Dead)
                                {
                                    Log.Message(packmate.Name.ToString() + " is not dead, adding thought");
                                    PackComp comp = packmate.TryGetComp<PackComp>();
                                    if(comp.Props.deathThought != null)
                                    {
                                        packmate.needs.mood.thoughts.memories.TryGainMemory(comp.Props.deathThought, pawn);
>>>>>>> beta
                                    }
                                }
                            }
                            break;
                        }
                    }
                }
            }
        }
    }
}