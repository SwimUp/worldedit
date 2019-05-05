using Harmony;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace WorldEdit.HarmonyHooks
{
    [HarmonyPatch(typeof(World)), HarmonyPatch("NaturalRockTypesIn")]
    public class Harmony_NaturalRockTypesIn
    {
        public static bool Prefix(int tile, ref IEnumerable<ThingDef> __result)
        {
            if (CustomNaturalRocks.ResourceData.Keys.Contains(tile))
            {
                __result = CustomNaturalRocks.ResourceData[tile].Rocks;
                return false;
            }

            return true;

        }
    }

    [HarmonyPatch(typeof(World)), HarmonyPatch("HasCaves")]
    public class Harmony_HasCaves
    {
        public static bool Prefix(int tile, ref bool __result)
        {
            if (CustomNaturalRocks.ResourceData.Keys.Contains(tile))
            {
                Log.Message($"--{CustomNaturalRocks.ResourceData[tile].Caves}");
                __result = CustomNaturalRocks.ResourceData[tile].Caves;
                return false;
            }

            return true;

        }
    }

}
