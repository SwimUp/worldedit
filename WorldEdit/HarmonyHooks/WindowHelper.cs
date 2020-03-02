using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace WorldEdit
{
    [HarmonyPatch(typeof(WindowStack))]
    [HarmonyPatch("Add")]
    [HarmonyPatch(new Type[] { typeof(Window) })]
    class Patch
    {
        static void Prefix(Window window)
        {
            Log.Warning("Window: " + window);
        }
    }
}
