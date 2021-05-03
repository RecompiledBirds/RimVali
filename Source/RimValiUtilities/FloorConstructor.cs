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
using Verse.AI;
using Verse;

namespace AvaliMod
{
    [StaticConstructorOnStartup]
    public static class FloorConstructor
    {
        public static List<DesignationCategoryDef> toUpdateDesignationCatDefs = new List<DesignationCategoryDef>();
        public static List<DesignatorDropdownGroupDef> toUpdateDropdownDesDefs = new List<DesignatorDropdownGroupDef>();
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
                //I have NO IDEA why, but one of those archotech mods has something called archotechmatteraddingsomecraptoavoidproblems and it hates me.
                //So lets assume they arent a special case
                //And do this?
                if (!DefDatabase<TerrainDef>.AllDefs.Any(terrain => terrain.defName == $"{def.defName}_{tDef.defName}"))
                {
                    bool hasmaxedout = false;
                    bool hasminedout = false;
                    ushort uS = (ushort)$"{def.defName}_{tDef.defName}".GetHashCode();
                    while (DefDatabase<TerrainDef>.AllDefs.Any(terrain => terrain.shortHash == uS) || floorsMade.Any(t => t.shortHash == uS))
                    {
                        if (uS < 65535 && !hasmaxedout)
                        {
                            uS += 1;
                        }
                        else if (uS == ushort.MaxValue)
                        {
                            hasmaxedout = true;
                        }
                        if (uS > ushort.MinValue && hasmaxedout && !hasminedout)
                        {
                            uS -= 1;
                        }
                        else if (uS == ushort.MinValue && hasmaxedout)
                        {
                            hasminedout = true;
                        }
                        if (hasminedout && hasmaxedout)
                        {
                            //If you ever see this i'll be impressed
                            Log.Warning($"[RimVali/FloorConstructor] Could not generate tile {String.Format(def.label, tDef.label)}'s unique short hash, aborting..");
                            return;
                        }


                    }
                    //Sets up some basic stuff
                    //shortHash  & defName are the very important
                    TerrainDef output = new TerrainDef()
                    {
                        color = tDef.GetColorForStuff(tDef),
                        uiIconColor = tDef.GetColorForStuff(tDef),
                        defName = $"{def.defName}_{tDef.defName}",
                        label = String.Format(def.label, tDef.label),
                        debugRandomId = uS,
                        index = uS,
                        shortHash = uS,
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

                    List<string> avoidFields = new List<string>() { "color", "defname", "label", "debugrandomid", "index", "shorthash", "costlist", "uiiconcolor", "designatordropdown" };
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
                                if(!toUpdateDesignationCatDefs.Contains(DefDatabase<DesignationCategoryDef>.AllDefs.Where(cat => cat.defName == res).ToList()[0]))
                                {
                                    toUpdateDesignationCatDefs.Add(DefDatabase<DesignationCategoryDef>.AllDefs.Where(cat => cat.defName == res).ToList()[0]);
                                }
                                output.designationCategory = DefDatabase<DesignationCategoryDef>.AllDefs.Where(cat => cat.defName == res).ToList()[0];
                            }
                        }
                        //This looks for a DesignationCategoryDef with a defname that matches the string between AddDesDropDown_ and [ENDDNAME]
                        if (str.Contains("AddDesDropDown_"))
                        {
                            string cS = string.Copy(str);
                            string res = cS.Substring(cS.IndexOf("AddDesDropDown_") + "AddDesDropDown_".Length, (cS.IndexOf("[ENDDNAME]") - ("[ENDDNAME]".Length + 5)) - cS.IndexOf("AddDesDropDown_"));
                            if (DefDatabase<DesignatorDropdownGroupDef>.AllDefs.Any(cat => cat.defName == res))
                            {
                                if (!toUpdateDropdownDesDefs.Contains(DefDatabase<DesignatorDropdownGroupDef>.AllDefs.Where(cat => cat.defName == res).ToList()[0]))
                                {
                                    toUpdateDropdownDesDefs.Add(DefDatabase<DesignatorDropdownGroupDef>.AllDefs.Where(cat => cat.defName == res).ToList()[0]);
                                }
                                output.designatorDropdown = DefDatabase<DesignatorDropdownGroupDef>.AllDefs.Where(cat => cat.defName == res).ToList()[0];
                            }
                        }
                        //This removes the tag from clones.
                        if (str.EndsWith("RemoveFromClones") || str.EndsWith("_RFC"))
                        {
                            toRemove.Add(str);
                        }
                    }
                    foreach (string str in toRemove)
                    {
                        output.tags.Remove(str);
                    }
                    //How vanilla RW sets up some stuff

                    //Blueprint
                    ThingDef thingDef= new ThingDef()
                    {
                        category = ThingCategory.Ethereal,
                        label = "Unspecified blueprint",
                        altitudeLayer = AltitudeLayer.Blueprint,
                        useHitPoints = false,
                        selectable = true,
                        seeThroughFog = true,
                        comps =
                        {
                            new CompProperties_Forbiddable()
                         },
                        drawerType = DrawerType.MapMeshAndRealTime
                    };
                    thingDef.thingClass = typeof(Blueprint_Build);
                    thingDef.defName = ThingDefGenerator_Buildings.BlueprintDefNamePrefix + output.defName;
                    thingDef.label = output.label + "BlueprintLabelExtra".Translate();
                    thingDef.entityDefToBuild = output;
                    thingDef.graphicData = new GraphicData();
                    thingDef.graphicData.shaderType = ShaderTypeDefOf.MetaOverlay;
                    thingDef.graphicData.texPath = "Things/Special/TerrainBlueprint";
                    thingDef.graphicData.graphicClass = typeof(Graphic_Single);
                    thingDef.constructionSkillPrerequisite = output.constructionSkillPrerequisite;
                    thingDef.artisticSkillPrerequisite = output.artisticSkillPrerequisite;
                    thingDef.clearBuildingArea = false;
                    thingDef.modContentPack = output.modContentPack;
                    output.blueprintDef = thingDef;

                    //Framedef
                    ThingDef frameDef = new ThingDef()
                    {
                        isFrameInt = true,
                        category = ThingCategory.Building,
                        label = "Unspecified building frame",
                        thingClass = typeof(Frame),
                        altitudeLayer = AltitudeLayer.Building,
                        useHitPoints = true,
                        selectable = true,
                        building = new BuildingProperties(),
                        comps =
                         {
                             new CompProperties_Forbiddable()
                         },
                        scatterableOnMapGen = false,
                        leaveResourcesWhenKilled = true
                    };
                    frameDef.building.artificialForMeditationPurposes = false;
                    frameDef.defName = ThingDefGenerator_Buildings.BuildingFrameDefNamePrefix + output.defName;
                    frameDef.label = output.label + "FrameLabelExtra".Translate();
                    frameDef.entityDefToBuild = output;
                    frameDef.useHitPoints = false;
                    frameDef.fillPercent = 0f;
                    frameDef.description = "Terrain building in progress.";
                    frameDef.passability = Traversability.Standable;
                    frameDef.selectable = true;
                    frameDef.constructEffect = output.constructEffect;
                    frameDef.building.isEdifice = false;
                    frameDef.constructionSkillPrerequisite = output.constructionSkillPrerequisite;
                    frameDef.artisticSkillPrerequisite = output.artisticSkillPrerequisite;
                    frameDef.clearBuildingArea = false;
                    frameDef.modContentPack = output.modContentPack;
                    frameDef.category = ThingCategory.Ethereal;
                    frameDef.entityDefToBuild = output;
                    output.frameDef = frameDef;


                    //This makes sure everything is setup how it should be
                    output.PostLoad();
                    output.ResolveReferences();
                    builder.AppendLine("---------------------------------------------");
                    builder.AppendLine($"[RimVali/FloorConstructor] Generated {output.label}\n Mat color: { tDef.stuffProps.color.ToString()},\n Floor color: {output.color} \n UI Icon color: {output.uiIconColor}");
                    floorsMade.Add(output);
                }
            }
        }

        static FloorConstructor()
        {
            Log.Message("[RimVali] Starting up floor constructor...");


            List<TerrainDef> workOn = new List<TerrainDef>();
            workOn.AddRange(DefDatabase<TerrainDef>.AllDefs);
            //Tells us to clone a terrain
            foreach (TerrainDef def in DefDatabase<TerrainDef>.AllDefs)
            {
                bool hasDoneTask = false;
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
                    Log.Message(def.tags.Any(str => str.Contains("removeFromResearch")).ToString());
                    if (def.tags.Any(str => str.Contains("removeFromResearch")))
                    {
                        List<string> tags = def.tags.Where(x => x.Contains("removeFromResearch_") && !x.NullOrEmpty()).ToList();
                        for (int a = 0; a < tags.Count; a++)
                        {
                            string s = tags[a];
                            try
                            {
                                hasDoneTask = true;
                                //Gets the category name between cloneMaterial_ and [ENDCATNAME]
                                string cS = string.Copy(s);
                                string res = cS.Substring(cS.IndexOf("removeFromResearch_") + "removeFromResearch_".Length, (cS.IndexOf("[ENDRESNAME]") - ("[ENDRESNAME]".Length + 7)) - cS.IndexOf("removeFromResearch_"));
                                Log.Message(res);
                                def.researchPrerequisites.RemoveAll(x => x.defName == res);

                            }
                            catch
                            {
                                
                            }
                        }
                    }
                    if (hasDoneTask)
                    {
                        def.PostLoad();
                        def.ResolveReferences();
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

            Log.Message($"[RimVali/FloorConstructor] {builder}");
            Log.Message("[RimVali/FloorConstructor] Updating architect menu..");

            //Updates/refreshes menus
            foreach (DesignationCategoryDef def in toUpdateDesignationCatDefs)
            {
                def.PostLoad();
                def.ResolveReferences();

            }
           
            foreach (DesignatorDropdownGroupDef def in toUpdateDropdownDesDefs)
            {
                def.PostLoad();
                def.ResolveReferences();
                
            }
            Log.Message($"[RimVali/FloorConstructor] Updated {toUpdateDesignationCatDefs.Count} designation categories & {toUpdateDropdownDesDefs.Count} dropdown designations.");
            //We need to do this or RW has a fit
            WealthWatcher.ResetStaticData();
           
            Log.Message($"[RimVali/FloorConstructor] Built  {floorsMade.Count} floors from {materials.Count} materials.");
        }
    }
}
