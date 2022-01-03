using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using UnityEngine;
using Verse;
namespace AvaliMod
{
    public class NexusCompProps : CompProperties
    {
        public NexusCompProps()
        {
            this.compClass = typeof(NexusComp);
        }
    }
    public class NexusComp : ThingComp
    {
        private bool enableAERIAL;

        public bool AERIAlIsEnabled
        {
            get
            {
                return enableAERIAL;
            }
        }

        public bool HasAERIAL
        {
            get
            {
                CompPowerTrader power = parent.TryGetComp<CompPowerTrader>();

                return power != null && power.PowerNet!=null&&power.PowerNet.connectors.Any(x => x.parent.def == AvaliDefs.Aerial);
            }
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {

            if (HasAERIAL)
            {
                Command_Action command_Action = new Command_Action();
                command_Action.defaultLabel = enableAERIAL? "NexusDisableDefense".Translate() : "NexusEnableDefense".Translate();
                command_Action.defaultDesc = "CommandStopForceAttackDesc".Translate();
                command_Action.icon = ContentFinder<Texture2D>.Get("UI/Commands/Attack", true);
                command_Action.action = delegate ()
                {
                    enableAERIAL = !enableAERIAL;
                };
                command_Action.hotKey = KeyBindingDefOf.Misc5;
                yield return command_Action;
            }
           
        }
    }
}
