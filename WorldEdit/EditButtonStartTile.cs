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
    [HarmonyPatch(typeof(Page_SelectStartingSite))]
    [HarmonyPatch("DoCustomBottomButtons")]
    class EditButtonStartTile
    {
        protected static readonly Vector2 BottomButSize = new Vector2(150f, 38f);
        static void Postfix()
        {
            if(Input.GetKeyDown(KeyCode.F5))
            {
                if (!MainMenu.isEdit)
                    return;

                if (Find.WindowStack.IsOpen(typeof(InGameEditor)))
                {
                    Log.Message("Currntly open...");
                }
                else
                {
                    MainMenu.Editor.Reset();
                    Find.WindowStack.Add(MainMenu.Editor);
                }
            }
        }
    }
}
