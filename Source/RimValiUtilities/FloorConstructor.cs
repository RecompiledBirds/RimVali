using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace AvaliMod
{
    [StaticConstructorOnStartup]
    public static class FloorConstructor
    {
        public static List<string> materials = new List<string>();
        public static List<TerrainDef> floorsMade = new List<TerrainDef>();
        public static StringBuilder builder = new StringBuilder();

        /// <summary>
        /// Creates all versions of a floor from a material; it's on the label
        /// </summary>
        /// <param name="def">The terrain def we are "duplicating"</param>
        /// <param name="name">The NAME of the category we want to duplicate.</param>
        public static void CreateAllVersions(TerrainDef def, string name)
        {
            foreach (ThingDef tDef in DefDatabase<ThingDef>.AllDefs.Where(d => d.stuffProps != null && !d.stuffProps.categories.NullOrEmpty() && d.stuffProps.categories.Any(cat => cat.defName == name)))
            {
                if (!materials.Contains(tDef.defName))
                {
                    materials.Add(tDef.defName);
                }
                //Sets up some basic stuff
                //shortHash  & defName are the very important
                TerrainDef output = new TerrainDef()
                {
                    color = tDef.GetColorForStuff(tDef),
                    uiIconColor = tDef.GetColorForStuff(tDef),
                    defName = $"{def.defName}_{tDef.defName}",
                    label = String.Format(def.label, tDef.label),
                    debugRandomId = (ushort)$"{def.defName}_{tDef.defName}".GetHashCode(),
                    index = (ushort)$"{def.defName}_{tDef.defName}".GetHashCode(),
                    shortHash = (ushort)$"{def.defName}_{tDef.defName}".GetHashCode(),
                    costList = ((Func<List<ThingDefCountClass>>)delegate
                    {
                        List<ThingDefCountClass> costList = new List<ThingDefCountClass>();
                        int amount = 0;
                        foreach (ThingDefCountClass thingDefCountClass in def.costList)
                        {
                            amount += thingDefCountClass.count;
                        }
                        costList.Add(new ThingDefCountClass()
                        {
                            thingDef = tDef,
                            count = amount
                        });
                        return costList;
                    })(),
                    designationCategory = def.designationCategory,
                    designatorDropdown = def.designatorDropdown
                };

                var bindingFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
                //This copies some of the varibles from the floor we are duplicating over
                //We don't want it to touch the fields we've already set, so I keep a list here to help.
                List<String> avoidFields = new List<string>() { "color", "defname", "label", "debugrandomid", "index", "shorthash", "costlist", "uiiconcolor", "designatordropdown" };
                foreach (FieldInfo field in def.GetType().GetFields(bindingFlags).Where(f => !avoidFields.Contains(f.Name.ToLower())))
                {
                    foreach (FieldInfo f2 in output.GetType().GetFields(bindingFlags).Where(f => f.Name == field.Name))
                    {
                        f2.SetValue(output, field.GetValue(def));
                    }
                }
                
                List<string> toRemove = new List<string>();
                foreach (string str in output.tags)
                {
                    //This looks for a DesignationCategoryDef with a defname that matches the string between AddDesCat_ and [ENDDESNAME]
                    if (str.Contains("AddDesCat_"))
                    {
                        string cS = string.Copy(str);
                        string res = cS.Substring(cS.IndexOf("AddDesCat_") + "AddDesCat_".Length, (cS.IndexOf("[ENDDESNAME]") - ("[ENDDESNAME]".Length - 2)) - cS.IndexOf("AddDesCat_"));
                        if (DefDatabase<DesignationCategoryDef>.AllDefs.Any(cat => cat.defName == res))
                        {
                            output.designationCategory = DefDatabase<DesignationCategoryDef>.AllDefs.Where(cat => cat.defName == res).ToList()[0];
                        }
                    }
                    //This looks for a DesignationCategoryDef with a defname that matches the string between AddDesDropDown_ and [ENDDNAME]
                    if (str.Contains("AddDesDropDown_"))
                    {
                        string cS = string.Copy(str);
                        string res = cS.Substring(cS.IndexOf("AddDesDropDown_") + "AddDesDropDown_".Length, (cS.IndexOf("[ENDDNAME]") - ("[ENDDNAME]".Length+5)) - cS.IndexOf("AddDesDropDown_"));
                        if (DefDatabase<DesignatorDropdownGroupDef>.AllDefs.Any(cat => cat.defName == res))
                        {
                            output.designatorDropdown = DefDatabase<DesignatorDropdownGroupDef>.AllDefs.Where(cat => cat.defName == res).ToList()[0];
                        }
                    }
                    //This removes the tag from clones.
                    if (str.EndsWith("RemoveFromClones"))
                    {
                        toRemove.Add(str);
                    }
                }
                foreach (string str in toRemove)
                {
                    output.tags.Remove(str);
                }
                //This makes sure everything is setup how it should be
                output.PostLoad();
                output.ResolveReferences();
                builder.AppendLine("---------------------------------------------");
                builder.AppendLine($"Generated {output.label}\n Mat color: { tDef.stuffProps.color.ToString()},\n Floor color: {output.color} \n UI Icon color: {output.uiIconColor}");
                floorsMade.Add(output);

            }
        }

        static FloorConstructor()
        {
            Log.Message("[RimVali] Starting up floor constructor...");


            List<TerrainDef> workOn = new List<TerrainDef>();
            foreach (TerrainDef def in DefDatabase<TerrainDef>.AllDefs)
            {
                workOn.Add(def);
            }
            //Tells us to clone a terrain
            foreach (TerrainDef def in DefDatabase<TerrainDef>.AllDefs)
            {

                if (!def.tags.NullOrEmpty())
                {
                    if (def.tags.Any(str => str.Contains("cloneMaterial")))
                    {
                        List<string> tags = def.tags.Where(x => x.Contains("cloneMaterial") && !x.NullOrEmpty()).ToList();
                        for (int a = 0; a < tags.Count; a++)
                        {
                            string s = tags[a];
                            try
                            {
                                //Gets the category name between cloneMaterial_ and [ENDCATNAME]
                                string cS = string.Copy(s);
                                string res = cS.Substring(cS.IndexOf("cloneMaterial_") + "_cloneMaterial".Length, (cS.IndexOf("[ENDCATNAME]") - ("[ENDCATNAME]".Length + 2)) - cS.IndexOf("cloneMaterial_"));
                                CreateAllVersions(def, res);
                            }
                            catch
                            {

                            }
                        }
                    }

                }
            }
            //Ensures we are adding to the DefDatabase. Just a saftey check.
            foreach (TerrainDef def in floorsMade)
            {
                if (!DefDatabase<TerrainDef>.AllDefs.Contains(def))
                {
                    DefDatabase<TerrainDef>.Add(def);
                }
            }
            Log.Message($"{builder}");
            Log.Message("Updating architect menu..");

            //Reloads all these so they pick up the new floors.
            //Yes, it's a bit slow. 
            //Idea: Gather a list of which DesignationCategoryDefs and DesignatorDropdownGroupDefs need to be "refreshed"
            foreach (DesignationCategoryDef def in DefDatabase<DesignationCategoryDef>.AllDefs)
            {
                def.PostLoad();
                def.ResolveReferences();

            }
            foreach (DesignatorDropdownGroupDef def in DefDatabase<DesignatorDropdownGroupDef>.AllDefs)
            {
                def.PostLoad();
                def.ResolveReferences();

            }
            //We need to do this or RW has a fit
            WealthWatcher.ResetStaticData();
            Log.Message($"[RimVali] Built  {floorsMade.Count} floors from {materials.Count} materials.");
        }
    }
}
