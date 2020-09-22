using RimWorld;
using Verse;
using UnityEngine;
namespace AvaliMod
{
    [StaticConstructorOnStartup]
    public class lister
    {
        private static readonly bool enableDebug = LoadedModManager.GetMod<RimValiMod>().GetSettings<RimValiModSettings>().enableDebugMode;
        public static void listAll()
        {
            int foundBundles = 0;
            int foundItems = 0;
            Log.Message("[RimVali_AssetFinder] RimVali is looking at all loaded asset bundles..");
            foreach(AssetBundle bundle in AssetBundle.GetAllLoadedAssetBundles())
            {
                foundBundles += 1;
                Log.Message("[RimVali] Asset bundle "+bundle.name + " is loaded.");
                if (enableDebug)
                {
                    foreach (string asset in bundle.GetAllAssetNames())
                    {
                        Log.Message("[RimVali] Found asset: " + asset);
                        foundItems += 1;
                    }
                }
            }
            Log.Message("[RimVali] There are " + foundBundles.ToString() + " bundles loaded.");
            if (enableDebug)
            {
                Log.Message("[RimVali] There are: " + foundItems.ToString() + " items loaded.");
            }
        }
        static lister()
        {
            listAll();
        }
    }
}