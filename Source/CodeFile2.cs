using Verse;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.IO;

namespace AvaliMod
{
    [StaticConstructorOnStartup]
    public class AvaliShaderDatabase
    {
        static AvaliShaderDatabase()
        {
           string dir = RimValiUtility.dir;
        
            Log.Message(dir);
            string test = dir;
            string info = test + "/RimValiAssetBundles/shader";
            AssetBundle bundle = RimValiUtility.shaderLoader(info);
            Tricolor = (Shader)bundle.LoadAsset("assets/resources/materials/avalishader.shader");
            if (!(Tricolor == null))
            {
                Log.Message(Tricolor.name);
                Log.Message("Load worked!");
            }
        }
       
        public static Shader Tricolor;
        public static Dictionary<string, Shader> lookup;

        public static Shader DefaultShader
        {
            get
            {
                return ShaderDatabase.Cutout;
            }
        }
    }
}