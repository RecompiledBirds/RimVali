using RimWorld;
using System.Linq;
using Verse;
using System.Collections.Generic;
namespace AvaliMod
{
   
    public class SlotPropsThingDef : CompProperties
    {
        public List<Slot> slots;
        public SlotPropsThingDef()
        {
            this.compClass = typeof(SlotCompThing);
        }
    }

    public class SlotCompThing : ThingComp
    {
        public SlotPropsThingDef Props
        {
            get
            {
                return (SlotPropsThingDef)this.props;
            }
        }
    }
    public class SlotPropsHediff : HediffCompProperties
    {
        public List<string> slotsUsed;
        public SlotPropsHediff()
        {
            this.compClass = typeof(SlotCompHediff);
        }
    }
    public class SlotCompHediff : HediffComp
    {
        private bool hasAdded = false;
        public SlotPropsHediff Props
        {
            get
            {
                return (SlotPropsHediff)this.props;
            }
        }
        public override string CompTipStringExtra
        {
            get {
                string returnString = "SlotsUsed".Translate() + ": ";
                SlotCompHediff slotComp = this.parent.TryGetComp<SlotCompHediff>();
                foreach (string slotUsed in slotComp.Props.slotsUsed)
                {
                    if (slotUsed.IndexOf(slotUsed) == slotUsed.Length-1)
                    {
                        returnString = returnString + slotUsed + ".";
                    }
                    else
                    {
                        returnString = returnString + slotUsed + ",\n";
                    }
                }
                return returnString;
            }
        }
        public override void CompPostTick(ref float severityAdjustment)
        {
            if (!LoadedModManager.GetMod<RimValiMod>().GetSettings<RimValiModSettings>().enableSlots)
            {
                return;
            }
            Pawn pawn = parent.pawn;
            if (!pawn.Spawned)
            {
                return;
            }
            List<Hediff> hediffsToUse = pawn.health.hediffSet.hediffs.Where<Hediff>(x => x.TryGetComp<SlotCompHediff>() != null).ToList();
            foreach(Hediff hediff in hediffsToUse)
            {
                SlotCompHediff slotComp = hediff.TryGetComp<SlotCompHediff>();
                SlotCompThing thingComp = pawn.TryGetComp<SlotCompThing>();
                if(thingComp == null)
                {
                    return;
                }
                if (slotComp != null)
                {
                    foreach(Slot slot in thingComp.Props.slots)
                    {
                        foreach (string slotused in slotComp.Props.slotsUsed)
                        {


                            if (slotused == slot.name && slot.curSize == slot.size+1)
                            {
                                try
                                {
                                    Hediff hediffToRemove = hediffsToUse[1];
                                    pawn.health.RemoveHediff(hediffToRemove);
                                    GenSpawn.Spawn(hediffToRemove.def.spawnThingOnRemoved, pawn.Position, pawn.Map, WipeMode.VanishOrMoveAside);
                                }
                                catch {
                                    Log.Message("Tried to remove, but something went wrong!");
                                }
                            }
                            else if (slotused == slot.name)
                            {
                                if (!hasAdded)
                                {
                                    slot.curSize++;
                                    hasAdded = true;
                                }
                                
                            }
                        }
                    }
                    
                }
                 
            }
        }

    }
}