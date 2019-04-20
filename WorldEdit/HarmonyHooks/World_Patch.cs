using Harmony;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace WorldEdit.HarmonyHooks
{
    [HarmonyPatch(typeof(World))]
    [HarmonyPatch("ExposeComponents")]
    class ExposeComponents_Patch
    {
        static void Postfix()
        {
            if (WorldEditor.isWorldTemplate)
            {
                Find.WindowStack.Add(new CustomStartingSite());
            }
        }
    }

    /// <summary>
    /// Базовая инициализация редактора
    /// </summary>
    [HarmonyPatch("WorldUpdate"), HarmonyPatch(typeof(World))]
    class World_WindowUpdate_Patch
    {
        public static void Postfix()
        {
            if (Input.GetKeyDown(KeyCode.F5))
            {
                if (!WorldEditor.isEdit)
                    return;

                if (!WorldEditor.isInit)
                    return;

                if (Find.WindowStack.IsOpen(typeof(InGameEditor)))
                {
                    Log.Message("Currntly open...");
                }
                else
                {
                    WorldEditor.Editor.Reset();
                    Find.WindowStack.Add(WorldEditor.Editor);
                }
            }
        }
    }
}
