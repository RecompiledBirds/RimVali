using Verse;

namespace AvaliMod
{
    public class AERIALChangeable : CompProperties
    {
        public int maxShellCount = 6;

        public AERIALChangeable()
        {
            compClass = typeof(AERIALChangeableProjectile);
        }
    }
}
