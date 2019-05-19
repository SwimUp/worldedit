using Harmony;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.Profile;
using WorldEdit.Editor;

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

                Dialog_MessageBox.CreateConfirmation("Back to menu?", delegate
                {
                    LongEventHandler.ClearQueuedEvents();
                    LongEventHandler.QueueLongEvent(delegate
                    {
                        MemoryUtility.ClearAllMapsAndWorld();
                        Current.Game = null;
                    }, "Entry", "LoadingLongEvent", doAsynchronously: false, null);
                });
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
                Find.WindowStack.Add(new RoadAndRiversEditor());
            }

            if (Input.GetKeyDown(Settings.WorldObjectHotKey))
            {
                Find.WindowStack.Add(WorldEditor.Editor.worldObjectsEditor);
            }
        }
    }

    [HarmonyPatch("SetInitialSizeAndPosition"), HarmonyPatch(typeof(WorldInspectPane))]
    class WorldInspect_Patch
    {
        public static void Postfix()
        {
            if (Settings.FullyActiveEditor)
            {
                if(!WorldEditor.isInit)
                {
                    WorldEditor.InitEditor();
                }
            }
        }
    }
}
