using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using WorldEdit.Interfaces;

namespace WorldEdit.Editor.WorldObjectsMenu
{
    internal class DownedRefugeeMenu : FWindow
    {
        public override Vector2 InitialSize => new Vector2(500, 590); //635
        private Vector2 scroll = Vector2.zero;

        private SiteCoreDef core = SiteCoreDefOf.DownedRefugee;
        private List<SitePartDef> parts = new List<SitePartDef>();

        public string time = string.Empty;

        private string threats = string.Empty;
        private float threatsFloat = 0f;

        private List<Pawn> pawnList = new List<Pawn>();

        public DownedRefugeeMenu()
        {
            resizeable = false;
        }
        public override void DoWindowContents(Rect inRect)
        {
            Text.Font = GameFont.Small;

            Widgets.Label(new Rect(0, 0, 290, 20), Translator.Translate("DownedRefugeeMenuTitle"));

            Widgets.Label(new Rect(0, 40, 400, 20), Translator.Translate("ThreatTypes"));
            int size2 = DefDatabase<SitePartDef>.AllDefsListForReading.Count * 25;
            Rect scrollRectFact = new Rect(0, 65, 490, 200);
            Rect scrollVertRectFact = new Rect(0, 0, scrollRectFact.x, size2);
            Widgets.BeginScrollView(scrollRectFact, ref scroll, scrollVertRectFact);
            int x = 0;
            foreach (var sitePart in DefDatabase<SitePartDef>.AllDefsListForReading)
            {
                if (Widgets.RadioButtonLabeled(new Rect(0, x, 480, 20), sitePart.defName, parts.Contains(sitePart)))
                {
                    if (parts.Contains(sitePart))
                        parts.Remove(sitePart);
                    else
                        parts.Add(sitePart);
                }
                x += 22;
            }
            Widgets.EndScrollView();

            if(Widgets.ButtonText(new Rect(0, 280, 490, 20), Translator.Translate("OpenPawnMenu")))
            {
                DrawPawnMenu();
            }
            Widgets.Label(new Rect(0, 310, 490, 20), $"Pawns: {pawnList.Count}");

            Widgets.Label(new Rect(0, 340, 490, 20), Translator.Translate("DurationTimeDaysLump"));
            time = Widgets.TextField(new Rect(0, 340, 490, 20), time);

            Widgets.Label(new Rect(0, 370, 100, 40), Translator.Translate("ThreatPoint"));
            Widgets.TextFieldNumeric(new Rect(110, 370, 380, 20), ref threatsFloat, ref threats, 0, 2000000);

            Widgets.Label(new Rect(0, 510, 290, 20), $"Selected tile ID: {Find.WorldSelector.selectedTile}");

            if (Widgets.ButtonText(new Rect(0, 550, 490, 20), Translator.Translate("AddNewItemStash")))
            {

            }
        }

        private void DrawPawnMenu()
        {
            Find.WindowStack.Add(new PawnMenu(pawnList));
        }
    }

}
