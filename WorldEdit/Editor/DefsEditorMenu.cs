using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using WorldEdit.Interfaces;

namespace WorldEdit.Editor
{
    public class DefsEditorMenu : FWindow
    {
        public override Vector2 InitialSize => new Vector2(420, 200);
        public DefsEditorMenu()
        {
            resizeable = false;
        }
        public override void DoWindowContents(Rect inRect)
        {
            Text.Font = GameFont.Small;

            if (Widgets.ButtonText(new Rect(0, 15, 200, 20), Translator.Translate("RoadEditor")))
            {
               
            }

        }
    }
}
