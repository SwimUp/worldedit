using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using WorldEdit.Interfaces;

namespace WorldEdit.Editor
{
    internal class SettlementCreator : EditWindow, IFWindow
    {
        public override Vector2 InitialSize => new Vector2(400, 300);

        private string settlementName = string.Empty;

        public SettlementCreator()
        {
            resizeable = false;
        }

        public override void DoWindowContents(Rect inRect)
        {
            Text.Font = GameFont.Small;

            Widgets.Label(new Rect(150, 0, 150, 30), Translator.Translate("SettlementCreatorTitle"));

            Widgets.Label(new Rect(0, 40, 50, 30), Translator.Translate("SettlementNameField"));
            settlementName = Widgets.TextField(new Rect(55, 40, 200, 30), settlementName);
            if(Widgets.ButtonText(new Rect(260, 40, 130, 30), Translator.Translate("GenerateSettlementName")))
            {
                settlementName = SettlementNameGenerator.GenerateSettlementName()
            }
        }

        public void Show()
        {
            if (Find.WindowStack.IsOpen(typeof(SettlementCreator)))
            {
                Log.Message("Currntly open...");
            }
            else
            {
                Find.WindowStack.Add(this);
            }
        }
    }
}
