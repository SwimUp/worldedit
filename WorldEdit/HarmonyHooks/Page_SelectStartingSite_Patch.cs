using Harmony;
using RimWorld;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using WorldEdit.WorldGen;

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

                WorldEditor.LoadedTemplate = new WorldTemplate();

                WorldEditor.isInit = true;
            }
        }
    }
}
