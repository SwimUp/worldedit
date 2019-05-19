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
using WorldEdit.WorldGen;

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
        public static class Page_CreateWorldParams_DoWindowContents_Patch
        {
            private static Vector2 scrollPositionWorlds = Vector2.zero;
            private static Vector2 scrollPositionInfo = Vector2.zero;
            private static WorldTemplate selectedTemplate = null;
            private static string worldInfo = string.Empty;

            public static bool Prefix(Rect rect, ref Rect __state)
            {
                __state = rect;
                return true;
            }

            public static void Postfix(ref Rect __state, ref Page_CreateWorldParams __instance)
            {
                GUI.BeginGroup(__state);
                float y = 280f;
                Rect baseRect = new Rect(0f, y, 200f, 30f);
                Widgets.Label(baseRect, Translator.Translate("EditorLabel"));
                Rect EarthRect = new Rect(200f, y, 200f, 30f);
                if (Settings.FullyActiveEditor == false)
                {
                    if (Widgets.RadioButtonLabeled(EarthRect, Translator.Translate("isEnableEditorLabel"), isEdit == true))
                    {
                        isEdit = !isEdit;
                    }
                }
                else
                {
                    Widgets.Label(EarthRect, Translator.Translate("ActiveFullyMode"));
                    isEdit = true;
                }

                GUI.EndGroup();

                Widgets.Label(new Rect(440, 40, 200, 20), Translator.Translate("WorldTemplates"));
                int worldsSize = WorldTemplateManager.Templates.Count * 25;
                Rect scrollRect = new Rect(440, 70, 200, 400);
                Rect scrollVertRect = new Rect(0, 0, scrollRect.x, worldsSize);
                Widgets.BeginScrollView(scrollRect, ref scrollPositionWorlds, scrollVertRect);
                int x = 0;
                foreach (var world in WorldTemplateManager.Templates)
                {
                    if (Widgets.ButtonText(new Rect(0, x, 180, 20), world.WorldName))
                    {
                        selectedTemplate = world;
                        StringBuilder builder = new StringBuilder();
                        builder.AppendLine($"{Translator.Translate("TemplateHead")}{world.WorldName}");
                        builder.AppendLine($"{Translator.Translate("TemplateAuthor")}{world.Author}");
                        builder.AppendLine($"{Translator.Translate("TemplateStoryteller")}{world.Storyteller}");
                        builder.AppendLine($"{Translator.Translate("TemplateScenario")}{world.Scenario}");
                        builder.AppendLine($"{Translator.Translate("TemplateDesc")}{world.Description}");
                        worldInfo = builder.ToString();
                    }
                    
                    x += 22;
                }
                Widgets.EndScrollView();
                Widgets.LabelScrollable(new Rect(650, 70, 340, 400), worldInfo, ref scrollPositionInfo, false, false);

                if (selectedTemplate != null)
                {
                    if (Widgets.ButtonText(new Rect(440, 500, 200, 20), Translator.Translate("LoadTemplate")))
                    {
                        isWorldTemplate = true;
                        LoadedTemplate = selectedTemplate;
                        GameDataSaveLoader.LoadGame(selectedTemplate.FilePath);
                    }
                }
            }
        }

        public static void InitEditor()
        {
            if (isEdit || Settings.FullyActiveEditor)
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

    class RP_PatchWindow
    {
        private static Vector2 scrollPositionWorlds = Vector2.zero;
        private static Vector2 scrollPositionInfo = Vector2.zero;
        private static WorldTemplate selectedTemplate = null;
        private static string worldInfo = string.Empty;

        public static void PostfixRP(Rect rect)
        {
            GUI.BeginGroup(rect);
            float y = 280f;
            Rect baseRect = new Rect(0f, y, 200f, 30f);
            Widgets.Label(baseRect, Translator.Translate("EditorLabel"));
            Rect EarthRect = new Rect(200f, y, 200f, 30f);
            if (Settings.FullyActiveEditor == false)
            {
                if (Widgets.RadioButtonLabeled(EarthRect, Translator.Translate("isEnableEditorLabel"), WorldEditor.isEdit == true))
                {
                    WorldEditor.isEdit = !WorldEditor.isEdit;
                }
            }
            else
            {
                Widgets.Label(EarthRect, Translator.Translate("ActiveFullyMode"));
                WorldEditor.isEdit = true;
            }

            GUI.EndGroup();

            Widgets.Label(new Rect(440, 40, 200, 20), Translator.Translate("WorldTemplates"));
            int worldsSize = WorldTemplateManager.Templates.Count * 25;
            Rect scrollRect = new Rect(440, 70, 200, 400);
            Rect scrollVertRect = new Rect(0, 0, scrollRect.x, worldsSize);
            Widgets.BeginScrollView(scrollRect, ref scrollPositionWorlds, scrollVertRect);
            int x = 0;
            foreach (var world in WorldTemplateManager.Templates)
            {
                if (Widgets.ButtonText(new Rect(0, x, 180, 20), world.WorldName))
                {
                    selectedTemplate = world;
                    StringBuilder builder = new StringBuilder();
                    builder.AppendLine($"{Translator.Translate("TemplateHead")}{world.WorldName}");
                    builder.AppendLine($"{Translator.Translate("TemplateAuthor")}{world.Author}");
                    builder.AppendLine($"{Translator.Translate("TemplateStoryteller")}{world.Storyteller}");
                    builder.AppendLine($"{Translator.Translate("TemplateScenario")}{world.Scenario}");
                    builder.AppendLine($"{Translator.Translate("TemplateDesc")}{world.Description}");
                    worldInfo = builder.ToString();
                }

                x += 22;
            }
            Widgets.EndScrollView();
            Widgets.LabelScrollable(new Rect(650, 70, 340, 400), worldInfo, ref scrollPositionInfo, false, false);

            if (selectedTemplate != null)
            {
                if (Widgets.ButtonText(new Rect(440, 500, 200, 20), Translator.Translate("LoadTemplate")))
                {
                    WorldEditor.isWorldTemplate = true;
                    WorldEditor.LoadedTemplate = selectedTemplate;
                    GameDataSaveLoader.LoadGame(selectedTemplate.FilePath);
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
