using System.Collections.Generic;
using Verse;

namespace AvaliMod
{
    public class ShowTextProps : HediffCompProperties
    {
        public List<ShowTextClass> showText;

        public ShowTextProps()
        {
            compClass = typeof(ShowTextComp);
        }
    }
}
