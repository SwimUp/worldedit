using Harmony;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

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
                GUI.EndGroup();
            }
        }
    }
}
