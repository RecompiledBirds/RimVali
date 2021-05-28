using System;
using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using Verse;

namespace RimVali
{
    public class RimValiMapComponent : MapComponent
    {
        public Dictionary<ThingDef, HashSet<Pawn>> defToPawns = new Dictionary<ThingDef, HashSet<Pawn>>();
        public Dictionary<Faction, HashSet<Pawn>> factionToPawns = new Dictionary<Faction, HashSet<Pawn>>();

        public HashSet<Pawn> alivePawns = new HashSet<Pawn>();

        private static Map[] maps = new Map[20];
        private static RimValiMapComponent[] comps = new RimValiMapComponent[20];

        // NOTE:
        // Use this to access the tracker
        public static RimValiMapComponent GetRimValiPawnTracker(Map map)
        {
            int index = map.Index;
            if (maps[index] == map && comps[index] != null)
                return comps[index];
            comps[index] = map.GetComponent<RimValiMapComponent>();
            maps[index] = map;
            return comps[index];
        }

        public RimValiMapComponent(Map map) : base(map)
        {
            foreach (Faction faction in Find.FactionManager.AllFactions)
                factionToPawns[faction] = new HashSet<Pawn>();
            foreach (ThingDef def in DefDatabase<ThingDef>.AllDefs.Where(d => d.race != null))
                defToPawns[def] = new HashSet<Pawn>();
        }

        public IEnumerable<Pawn> CheckAllPawnsInMapAndFaction(Faction faction)
        {
            return factionToPawns[faction];
        }

        public IEnumerable<Pawn> AllPawnsOfRaceInWorld(List<ThingDef> races)
        {
            List<Pawn> pawns = new List<Pawn>();
            foreach (ThingDef def in races)
            {
                var temp = defToPawns[def];
                if (temp != null && temp.Count > 0)
                    pawns.AddRange(temp);
            }
            return pawns;
        }

        public void Notify_Spawned(Pawn pawn)
        {
            defToPawns[pawn.def].Add(pawn);
            Faction faction = pawn.Faction;
            if (faction != null)
                factionToPawns[faction].Add(pawn);
        }

        public void Notify_Removed(Pawn pawn)
        {
            defToPawns[pawn.def].RemoveWhere(p => p == pawn);
            Faction faction = pawn.Faction;
            if (faction != null)
                factionToPawns[faction].RemoveWhere(p => p == pawn);
        }

        public void Notify_Dead(Pawn pawn)
        {
            Notify_Removed(pawn);
        }

        public void Notify_FactionUnset(Pawn pawn)
        {
            Faction faction = pawn.Faction;
            if (faction != null)
                factionToPawns[faction].RemoveWhere(p => p == pawn);
        }

        public void Notify_FactionSet(Pawn pawn)
        {
            Faction faction = pawn.Faction;
            if (faction != null)
                factionToPawns[faction].Add(pawn);
        }

        [HarmonyPatch(typeof(Pawn), nameof(Pawn.SpawnSetup))]
        [HarmonyPostfix]
        public static void Pawn_SpawnSetup(Pawn __instance)
        {
            GetRimValiPawnTracker(__instance.Map).Notify_Spawned(__instance);
        }

        [HarmonyPatch(typeof(Pawn), nameof(Pawn.DeSpawn))]
        [HarmonyPostfix]
        public static void Pawn_DeSpawn(Pawn __instance)
        {
            GetRimValiPawnTracker(__instance.Map).Notify_Removed(__instance);
        }

        [HarmonyPatch(typeof(Pawn), nameof(Pawn.Destroy))]
        [HarmonyPostfix]
        public static void Pawn_Destroy(Pawn __instance)
        {
            GetRimValiPawnTracker(__instance.Map).Notify_Removed(__instance);
        }

        [HarmonyPatch(typeof(Pawn), nameof(Pawn.Kill))]
        [HarmonyPostfix]
        public static void Pawn_Kill(Pawn __instance)
        {
            GetRimValiPawnTracker(__instance.Map).Notify_Spawned(__instance);
        }

        [HarmonyPatch(typeof(Pawn), nameof(Pawn.Faction), MethodType.Setter)]
        [HarmonyPrefix]
        public static void Pawn_Faction_Setter_Prefix(Pawn __instance)
        {
            GetRimValiPawnTracker(__instance.Map).Notify_FactionUnset(__instance);
        }

        [HarmonyPatch(typeof(Pawn), nameof(Pawn.Faction), MethodType.Setter)]
        [HarmonyPostfix]
        public static void Pawn_Faction_Setter_Postfix(Pawn __instance)
        {
            GetRimValiPawnTracker(__instance.Map).Notify_FactionSet(__instance);
        }

        [HarmonyPatch(typeof(Pawn_HealthTracker), nameof(Pawn_HealthTracker.SetDead))]
        [HarmonyPostfix]
        public static void Pawn_HealthTracker_SetDead(Pawn ___pawn)
        {
            GetRimValiPawnTracker(___pawn.Map).Notify_Dead(___pawn);
        }
    }
}