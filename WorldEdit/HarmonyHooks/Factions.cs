using Harmony;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace WorldEdit.HarmonyHooks
{
    [HarmonyPatch(typeof(Settlement)), HarmonyPatch("ExpandingIcon", MethodType.Getter)]
    class ExpandingIcon_Patch
    {
        static bool Prefix(Settlement __instance, ref Texture2D __result)
        {
            if(CustomFactions.CustomIcons.Keys.Contains(__instance.Name))
            {
                __result = CustomFactions.CustomIcons[__instance.Name].Texture;
                return false;
            }

            return true;
        }
    }

    [HarmonyPatch(typeof(Settlement)), HarmonyPatch("Material", MethodType.Getter)]
    class Material_Patch
    {
        static bool Prefix(Settlement __instance, ref Material __result)
        {
            if (CustomFactions.CustomIcons.Keys.Contains(__instance.Name))
            {
                __result = MaterialPool.MatFrom(CustomFactions.CustomIcons[__instance.Name].Texture);
                return false;
            }

            return true;
        }
    }
}
