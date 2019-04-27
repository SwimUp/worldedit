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
    internal sealed class WorldEditor : GameComponent
    {
        /// <summary>
        /// Включен ли редактор
        /// </summary>
        public static bool isEdit = false;

        /// <summary>
        /// Инициализирован ли редактор
        /// </summary>
        public static bool isInit = false;

        /// <summary>
        /// Редактор
        /// </summary>
        public static InGameEditor Editor = null;

        /// <summary>
        /// Updater мира
        /// </summary>
        public static WorldUpdater WorldUpdater = null;

        /// <summary>
        /// Была ли загрузка из шаблона
        /// </summary>
        public static bool isWorldTemplate = false;

        public static WorldTemplate LoadedTemplate = null;

        public WorldEditor()
        {
        }

        public WorldEditor(Game game)
        {
        }

        [HarmonyPatch(typeof(Page_CreateWorldParams), "DoWindowContents", new Type[]
        {
            typeof(Rect)
        })]
        class Page_CreateWorldParams_DoWindowContents_Patch
        {
            private static Vector2 scrollPositionWorlds = Vector2.zero;
            
            private static bool Prefix(Rect rect, ref Rect __state)
            {
                Log.Message("HOOK");
                __state = rect;
                return true;
            }

            private static void Postfix(ref Rect __state)
            {
                Log.Message("HOOK2");
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
                        LoadedTemplate = new WorldTemplate()
                        {
                            FilePath = world,
                            WorldName = fileName
                        };
                        GameDataSaveLoader.LoadGame(Path.GetFileNameWithoutExtension(world));
                    }
                    x += 22;
                }

                Widgets.EndScrollView();
            }
        }

        public static void InitEditor()
        {
            if (isEdit)
            {
                if (!isInit)
                {
                    WorldUpdater = new WorldUpdater();
                    Editor = new InGameEditor();
                    isInit = true;
                }
            }
        }
    }

    /// <summary>
    /// Базовая инициализация редактора
    /// </summary>
    [HarmonyPatch("PostOpen"), HarmonyPatch(typeof(Page_CreateWorldParams))]
    class Page_CreateWorldParams_PostOpen_Patch
    {
        public static void Postfix()
        {
            WorldEditor.isEdit = false;
            WorldEditor.isInit = false;

            WorldEditor.WorldUpdater = null;
            WorldEditor.Editor = null;

            WorldEditor.LoadedTemplate = null;
        }
    }
}
