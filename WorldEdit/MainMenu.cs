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
        [HarmonyPatch(typeof(Page_CreateWorldParams), "DoWindowContents", new Type[]
        {
            typeof(Rect)
        })]
        private static class Page_CreateWorldParams_DoWindowContents_Patch
        {
            private static Vector2 planetInfoSliderPos;

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
                Widgets.Label(baseRect, Translator.Translate("PlanetTypeSettings"));
                Rect EarthRect = new Rect(200f, y, 200f, 30f);
                Rect IceGigantRect = new Rect(200f, y + 40f, 200f, 30f);
                Rect textRect = new Rect(450f, y, __state.width / 2, 300f);
                if (Widgets.RadioButtonLabeled(EarthRect, Translator.Translate("PlanetTypeSettings_Earth"), isEdit == true))
                {
                    isEdit = !isEdit;
                }
                GUI.EndGroup();
            }
        }

        public static bool isEdit = false;

        public MainMenu()
        {
        }

        public MainMenu(Game game)
        {

        }

    }
}
