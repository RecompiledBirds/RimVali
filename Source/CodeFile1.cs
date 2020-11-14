using System;
using Verse;

namespace AvaliMod
{
    // Token: 0x020000DC RID: 220
    public class DeathActionWorker_Test : DeathActionWorker
    {
        // Token: 0x060003D2 RID: 978
        public override void PawnDied(Corpse corpse)
        {
            Log.Message("Hi! I'm a test!", false);
        }
    }
}