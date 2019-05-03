using Harmony;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace WorldEdit.HarmonyHooks
{
    /// <summary>
    /// Базовая инициализация редактора
    /// </summary>
    [HarmonyPatch("PostOpen"), HarmonyPatch(typeof(Page_SelectStartingSite))]
    class PostOpen_Patch
    {
        public static void Postfix()
        {
            if (WorldEditor.isEdit)
            {
                if (WorldEditor.isInit)
                    return;

                WorldEditor.WorldUpdater = new WorldUpdater();
                WorldEditor.Editor = new InGameEditor();
                Find.WindowStack.Add(WorldEditor.Editor);

                WorldEditor.isInit = true;
            }
        }
    }

    [HarmonyPatch("ExtraOnGUI"), HarmonyPatch(typeof(Page_SelectStartingSite))]
    class ExtraOnGUI_Patch
    {
        public static bool Prefix()
        {
            if (WorldEditor.isEdit)
            {
                if (WorldEditor.isInit)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
