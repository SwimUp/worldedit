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
            Text.Font = GameFont.Small;

            if(Widgets.ButtonText(new Rect(0, 15, 200, 20), Translator.Translate("BackstoryUtils")))
            {
                Find.WindowStack.Add(new BackStoryCreator(true));
            }

            if (Widgets.ButtonText(new Rect(210, 15, 200, 20), Translator.Translate("WorldGenSteps")))
            {
                Find.WindowStack.Add(new WorldGenStepsMenu());
            }
        }
    }
}
