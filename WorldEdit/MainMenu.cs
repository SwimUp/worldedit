using Harmony;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using Verse;
using Verse.Profile;

namespace WorldEdit
{
    internal class MainMenu : GameComponent
    {
        /// <summary>
        /// Включен ли редактор
        /// </summary>
        public static bool isEdit = false;
        /// <summary>
        /// Редактор
        /// </summary>
        public static InGameEditor Editor = null;

        public static WorldUpdater WorldUpdater = null;

        public static bool isWorldTemplate = false;

        public MainMenu()
        {
        }

        public MainMenu(Game game)
        {
        }

        [HarmonyPatch(typeof(Page_CreateWorldParams), "DoWindowContents", new Type[]
        {
            typeof(Rect)
        })]
        private static class Page_CreateWorldParams_DoWindowContents_Patch
        {
            private static Vector2 scrollPositionWorlds = Vector2.zero;

            private static bool Prefix(Rect rect, ref Rect __state)
            {
                __state = rect;
                isWorldTemplate = false;
                return true;
            }

            private static void Postfix(ref Rect __state, ref Page_CreateWorldParams __instance)
            {
                GUI.BeginGroup(__state);
                float y = 280f;
                Rect baseRect = new Rect(0f, y, 200f, 30f);
                Widgets.Label(baseRect, Translator.Translate("EditorLabel"));
                Rect EarthRect = new Rect(200f, y, 200f, 30f);
                if (Widgets.RadioButtonLabeled(EarthRect, Translator.Translate("isEnableEditorLabel"), isEdit == true))
                {
                    isEdit = !isEdit;
                }

                GUI.EndGroup();

                string worldsPath = Path.Combine(GenFilePaths.SaveDataFolderPath, "Saves");
                string[] worlds = Directory.GetFiles(worldsPath, "wtemplate_*");

                Widgets.Label(new Rect(510, 40, 300, 20), Translator.Translate("WorldTemplates"));
                int worldsSize = worlds.Length * 25;
                Rect scrollRect = new Rect(510, 65, 300, 600);
                Rect scrollVertRect = new Rect(0, 0, scrollRect.x, worldsSize);
                Widgets.BeginScrollView(scrollRect, ref scrollPositionWorlds, scrollVertRect);
                int x = 0;
                foreach (string world in worlds)
                {
                    string fileName = Path.GetFileNameWithoutExtension(world).Substring(10);
                    if (Widgets.ButtonText(new Rect(0, x, 290, 20), fileName))
                    {
                        isWorldTemplate = true;
                        GameDataSaveLoader.LoadGame(Path.GetFileNameWithoutExtension(world));
                    }
                    x += 22;
                }

                Widgets.EndScrollView();
            }
        }
    }

    [HarmonyPatch(typeof(World))]
    [HarmonyPatch("ExposeComponents")]
    class WorldTemplateLoadHook
    {
        static void Postfix()
        {
            if (MainMenu.isWorldTemplate)
            {
                Find.WindowStack.Add(new CustomStartingSite());
            }
        }
    }

    [HarmonyPatch(typeof(Dialog_FileList))]
    [HarmonyPatch("PostOpen")]
    class GetSavesGamesConstructorHook
    {
        static void Postfix()
        {
            MainMenu.isWorldTemplate = false;
        }
    }

    [HarmonyPatch(typeof(Dialog_SaveFileList))]
    [HarmonyPatch("ReloadFiles")]
    class GetSavesListHook
    {
        static void Postfix(Dialog_SaveFileList __instance)
        {
            ChangeFileList(__instance);
        }

        static void ChangeFileList(Dialog_SaveFileList instance)
        {
            object value = Utils.GetInstanceField(typeof(Dialog_SaveFileList), instance, "files");

            if (value != null)
            {
                List<SaveFileInfo> infos = value as List<SaveFileInfo>;

                if (infos != null)
                {
                    for (int i = 0; i < infos.Count; i++)
                    {
                        SaveFileInfo info = infos[i];

                        if (info.FileInfo.Name.Contains("wtemplate_"))
                            infos.Remove(info);
                    }
                }
            }
        }
    }
}
