using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace AvaliMod
{
    public class NexusCompProps : CompProperties
    {
        public NexusCompProps()
        {
            compClass = typeof(NexusComp);
        }
    }

    public class NexusComp : ThingComp
    {
        public bool AerialIsEnabled { get; private set; }

        public bool HasAERIAL
        {
            get
            {
                var power = parent.TryGetComp<CompPowerTrader>();

                return power != null && power.PowerNet != null &&
                       power.PowerNet.connectors.Any(x => x.parent.def == AvaliDefs.Aerial);
            }
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            if (HasAERIAL)
            {
                var command_Action = new Command_Action
                {
                    defaultLabel = AerialIsEnabled
                        ? "NexusDisableDefense".Translate()
                        : "NexusEnableDefense".Translate(),
                    defaultDesc = "CommandStopForceAttackDesc".Translate(),
                    icon = ContentFinder<Texture2D>.Get("UI/Commands/Attack"),
                    action = delegate { AerialIsEnabled = !AerialIsEnabled; },
                    hotKey = KeyBindingDefOf.Misc5,
                };
                yield return command_Action;
            }
        }
    }
}
