using Harmony;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace WorldEdit
{
    [HarmonyPatch("WorldGrid"), HarmonyPatch(typeof(WorldGrid))]
    internal class WorldGridHook
    {
        public static void Postfix()
        {
            Log.Message("Worldgrid gen");

            if(MainMenu.isEdit)
            {
                Log.Message("Editor ON");
                MainMenu.Editor = new InGameEditor();
            }
        }
    }

    internal class InGameEditor : Window
    {
        private Vector2 scrollPosition = Vector2.zero;
        private List<BiomeDef> avaliableBiomes { get; set; }

        public InGameEditor()
        {
            avaliableBiomes = new List<BiomeDef>();

            foreach(BiomeDef biome in DefDatabase<BiomeDef>.AllDefs)
            {
                avaliableBiomes.Add(biome);
            }
        }

        public override void DoWindowContents(Rect inRect)
        {
            Rect rect = new Rect(0, 0, 500, 200);
            GUI.BeginGroup(rect);
            Text.Font = GameFont.Medium;
            GUI.BeginScrollView(new Rect(10, 10, 200, 100), scrollPosition, new Rect(0, 0, 200, 200));
            Rect buttonRect = new Rect(0, 0, 100, 20);
            GUILayout.BeginVertical();
            foreach(BiomeDef def in avaliableBiomes)
            {
                GUI.Button(buttonRect, def.label);
            //    buttonRect.y += 20;
            }
            GUILayout.EndVertical();
            GUI.EndScrollView();

            GUI.EndGroup();
        }
    }
}
