using System.Collections.Generic;
using Verse;

namespace AvaliMod
{
    public class ShowTextPropsV3 : HediffCompProperties
    {
        public List<ShowTextClass> showText;

        public ShowTextPropsV3()
        {
            compClass = typeof(ShowTextComp);
        }
    }
}