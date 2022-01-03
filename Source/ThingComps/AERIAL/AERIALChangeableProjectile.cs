using System;
using System.Collections.Generic;
using RimWorld;
using Verse;

namespace AvaliMod
{
    public class AERIALChangeableProjectile : CompChangeableProjectile
    {
        public new StorageSettings allowedShellsSettings;
        private bool hasSetup;

        public new int loadedCount;


        public List<ThingDef> loadedShells = new List<ThingDef>();

        public int maxShells;

        public new AERIALChangeable Props => (AERIALChangeable)props;

        public new ThingDef Projectile => !Loaded ? null : loadedShells[loadedShells.Count - 1].projectileWhenLoaded;

        public new bool Loaded => loadedShells.Count > 0;

        public new bool StorageTabVisible => true;

        public override void PostExposeData()
        {
            //Scribe_Defs.Look<ThingDef>(ref this.loadedShell, "loadedShell");
            Scribe_Values.Look(ref loadedCount, "loadedCount");
            Scribe_Deep.Look(ref allowedShellsSettings, "allowedShellsSettings", Array.Empty<object>());
            Scribe_Collections.Look(ref loadedShells, "loadedShells");
            if (loadedShells == null)
            {
                loadedShells = new List<ThingDef>();
            }
        }

        public override void Initialize(CompProperties props)
        {
            base.Initialize(props);

            allowedShellsSettings = new StorageSettings(this);
            if (parent.def.building.defaultStorageSettings != null)
            {
                allowedShellsSettings.CopyFrom(parent.def.building.defaultStorageSettings);
            }
        }

        public override void CompTick()
        {
            if (!hasSetup)
            {
                maxShells = Props.maxShellCount;
                if (parent.def == AvaliDefs.Aerial)
                {
                    maxShells = RimValiMod.settings.AERIALShellCap;
                }

                hasSetup = true;
            }

            base.CompTick();
        }

        // Token: 0x060085FF RID: 34303 RVA: 0x00059D03 File Offset: 0x00057F03
        public override void Notify_ProjectileLaunched()
        {
            if (loadedCount > 0)
            {
                loadedCount--;
            }

            if (loadedCount <= 0)
            {
                loadedShells[loadedShells.Count - 1] = null;
            }
        }

        public void NewLoadShell(ThingDef shell, int count)
        {
            loadedShells.Add(shell);
        }

        public Thing NewRemoveShell()
        {
            Thing thing = ThingMaker.MakeThing(loadedShells[loadedShells.Count - 1]);
            thing.stackCount = 1;
            loadedShells.RemoveAt(loadedShells.Count - 1);
            return thing;
        }

        public new StorageSettings GetStoreSettings()
        {
            return allowedShellsSettings;
        }

        // Token: 0x06008603 RID: 34307 RVA: 0x00059D79 File Offset: 0x00057F79
        public new StorageSettings GetParentStoreSettings()
        {
            return parent.def.building.fixedStorageSettings;
        }
    }
}
