using Harmony;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace WorldEdit.HarmonyHooks
{
    /*
    [HarmonyPatch(typeof(MainMenuDrawer))]
    [HarmonyPatch("Init")]
    class MainMenuHooks
    {
        static void Postfix()
        {
            var originalRP = typeof(Planets_Code.Planets_CreateWorldParams).GetMethod("DoWindowContents");
            var postfixRP = typeof(RPP).GetMethod("Postfix");

            WorldEdit.harmonyInstance.Patch(originalRP, postfix: new HarmonyMethod(postfixRP));
            var info = WorldEdit.harmonyInstance.GetPatchInfo(originalRP);
            if (info == null)
            {
                Log.Message("NOT PATCHED");
            }
            else
            {
                Log.Message($"PATCHED {info.Postfixes.Count}");
                foreach (var patch in info.Postfixes)
                {
                    Console.WriteLine("index: " + patch.index);
                    Console.WriteLine("index: " + patch.owner);
                    Console.WriteLine("index: " + patch.patch);
                    Console.WriteLine("index: " + patch.priority);
                    Console.WriteLine("index: " + patch.before);
                    Console.WriteLine("index: " + patch.after);
                }
            }

        }
    }
    */
}
