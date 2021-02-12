using Verse;
using RimWorld;
using System.Collections.Generic;

namespace AvaliMod
{
    public class AERIALCompReloadableProps : CompProperties
    {
        public int maxShells;
        public AERIALCompReloadableProps()
        {
            compClass = typeof(AERIALCompReloadable);
        }
    }
    public class AERIALCompReloadable : CompChangeableProjectile
    {
        
        public new AERIALCompReloadableProps Props
        {
            get
            {
                return (AERIALCompReloadableProps)this.props;
            }
        }
        public List<ThingDef> loadedShells;
        
        public AERIALCompReloadable()
        {
            loadedShells = new List<ThingDef>();
        }
        public override void PostExposeData()
        {
            Scribe_Collections.Look(ref loadedShells, "loadedShells", LookMode.Undefined);
            base.PostExposeData();
        }

        public new void LoadShell(ThingDef shell, int count)
        {
             if(loadedCount+count <= Props.maxShells)
            {
                loadedShells.Add(shell);
                loadedCount = +count;
            }
        }
    }
}