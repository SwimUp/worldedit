using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using WorldEdit.Interfaces;
using WorldEdits;

namespace WorldEdit.Editor
{
    class UtilsMenu : FWindow
    {
        public override Vector2 InitialSize => new Vector2(420, 200);
        public UtilsMenu()
        {
            resizeable = false;
        }
        public override void DoWindowContents(Rect inRect)
        {
            WidgetRow row1 = new WidgetRow(0, 0, UIDirection.RightThenDown, 200);
            if (row1.ButtonText(Translator.Translate("BackstoryUtils")))
            {
                Find.WindowStack.Add(new BackStoryCreator(true));
            }

            WidgetRow row2 = new WidgetRow(210, 0, UIDirection.RightThenDown, 200);
            if (row2.ButtonText(Translator.Translate("WorldGenSteps"), Translator.Translate("WorldGenStepsInfo")))
            {
                Find.WindowStack.Add(new WorldGenStepsMenu());
            }
        }
    }
}
