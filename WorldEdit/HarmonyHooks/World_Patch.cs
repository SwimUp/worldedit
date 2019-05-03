using Harmony;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.Profile;

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
            if (!WorldEditor.isEdit && !WorldEditor.isInit)
                return;

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (Find.WindowStack.IsOpen(typeof(CustomStartingSite)))
                    return;

                Find.WindowStack.Add(new ConfirmActionPage(delegate
                {
                    LongEventHandler.ClearQueuedEvents();
                    LongEventHandler.QueueLongEvent(delegate
                    {
                        MemoryUtility.ClearAllMapsAndWorld();
                        Current.Game = null;
                    }, "Entry", "LoadingLongEvent", doAsynchronously: true, null);
                }));
            }

            if (Input.GetKeyDown(Settings.EditorHotKey))
            {
                WorldEditor.Editor.Reset();
                Find.WindowStack.Add(WorldEditor.Editor);
            }

            if (Input.GetKeyDown(Settings.FactionHotKey))
            {
                Find.WindowStack.Add(WorldEditor.Editor.factionEditor);
            }

            if (Input.GetKeyDown(Settings.RiversAndRoadsHotKey))
            {
                Find.WindowStack.Add(WorldEditor.Editor.roadEditor);
            }

            if (Input.GetKeyDown(Settings.WorldObjectHotKey))
            {
                Find.WindowStack.Add(WorldEditor.Editor.worldObjectsEditor);
            }
        }
    }
}
