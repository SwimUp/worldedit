using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using WorldEdit.Interfaces;

namespace WorldEdit.Editor.Factions
{
   internal class ItemEditor : FWindow
    {
        public override Vector2 InitialSize => new Vector2(400, 500);
        private Vector2 scrollPosition = Vector2.zero;

        public override void DoWindowContents(Rect inRect)
        {
            Text.Font = GameFont.Small;

            Widgets.Label(new Rect(0, 0, 450, 20), Translator.Translate("ItemListTitle"));

            int defSize = DefDatabase<ThingDef>.DefCount * 30;
            Rect scrollRectFact = new Rect(0, 50, 385, 400);
            Rect scrollVertRectFact = new Rect(0, 0, scrollRectFact.x, defSize);
            Widgets.BeginScrollView(scrollRectFact, ref scrollPosition, scrollVertRectFact);
            int x = 0;
            foreach (var item in DefDatabase<ThingDef>.AllDefs)
            {
                if (Widgets.ButtonText(new Rect(0, x, 375, 20), item.label))
                {

                }
                x += 22;
            }
        }
    }
}
