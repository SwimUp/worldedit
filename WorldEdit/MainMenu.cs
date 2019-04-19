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
            private static Vector2 scrollPosition = Vector2.zero;

            private static bool Prefix(Rect rect, ref Rect __state)
            {
                __state = rect;
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

                string worldsPath = Path.Combine(GenFilePaths.SaveDataFolderPath, "Saves");
                string[] worlds = Directory.GetFiles(worldsPath, "wtemplate_*");

                int biomeDefSize = worlds.Length * 25;
                Rect scrollRect = new Rect(200, 310, 200, 200);
                Rect scrollVertRect = new Rect(0, 0, scrollRect.x, biomeDefSize);
                Widgets.BeginScrollView(scrollRect, ref scrollPosition, scrollVertRect);
                int x = 0;
                foreach(string world in worlds)
                {
                    string fileName = Path.GetFileNameWithoutExtension(world);
                    if (Widgets.ButtonText(new Rect(0, x, 200, 20), fileName))
                    {
                        isWorldTemplate = true;
                        GameDataSaveLoader.LoadGame(fileName);
                    }
                    x += 22;
                }


                Widgets.EndScrollView();

                GUI.EndGroup();
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
}
