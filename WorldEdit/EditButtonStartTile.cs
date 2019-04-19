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
    /// <summary>
    /// Отлов события отрисовки кнопок для того, чтобы открывать меню на F5
    /// </summary>
    [HarmonyPatch(typeof(Page_SelectStartingSite))]
    [HarmonyPatch("DoCustomBottomButtons")]
    class EditButtonStartTile
    {
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
