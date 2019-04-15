using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using Verse;
using WorldEdit.Interfaces;

namespace WorldEdit.Editor
{
    internal class FactionEditor : EditWindow, IFWindow
    {
        public override Vector2 InitialSize => new Vector2(500, 600);

        internal SettlementManager settlementManager;
        internal FactionManager factionManager;

        private Vector2 scrollPositionFact = Vector2.zero;
        private Vector2 scrollGlobalPositionFact = Vector2.zero;

        public FactionEditor()
        {
            resizeable = false;

            settlementManager = new SettlementManager();
            factionManager = new FactionManager();
        }

        public void Show()
        {
            if (Find.WindowStack.IsOpen(typeof(FactionEditor)))
            {
                Log.Message("Currntly open...");
            }
            else
            {
                Find.WindowStack.Add(this);
            }
        }

        public override void DoWindowContents(Rect inRect)
        {
            Text.Font = GameFont.Small;

            WidgetRow mainRow = new WidgetRow(0, 0);
            if(mainRow.ButtonText(Translator.Translate("FactionManager")))
            {
                factionManager.Show();
            }
            if (mainRow.ButtonText(Translator.Translate("SettlementManager")))
            {
                settlementManager.Show();
            }

            /*
            float factionPosition = inRect.height - 300;
            int yButtonPos = 0;
            int factionDefSize = avaliableFactions.Count * 25;
            Rect scrollRectFact = new Rect(0, 20, 190, factionPosition);
            Rect scrollVertRectFact = new Rect(0, 0, scrollRectFact.x, factionDefSize);
            Widgets.Label(new Rect(0, 0, 200, 20), Translator.Translate("AllFactionsList"));
            Widgets.BeginScrollView(scrollRectFact, ref scrollPositionFact, scrollVertRectFact);

            yButtonPos = 5;
            if (Widgets.ButtonText(new Rect(0, yButtonPos, 190, 20), Translator.Translate("NoText")))
            {
                selectedFaction = null;
            }
            yButtonPos += 25;
            foreach (FactionDef def in avaliableFactions)
            {
                if (Widgets.ButtonText(new Rect(0, yButtonPos, 190, 20), def.label))
                {
                    selectedFaction = def;
                    selectedGlobalFaction = null;
                }
                yButtonPos += 22;
            }

            Widgets.EndScrollView();

            Widgets.Label(new Rect(210, 0, 200, 20), Translator.Translate("AllSpawnedFactionsList"));
            int factionGlobalDefSize = avaliableGlobalFactions.Count * 25;
            Rect scrollRectGlobalFact = new Rect(210, 20, 210, factionPosition);
            Rect scrollVertRectGlobalFact = new Rect(0, 0, scrollRectGlobalFact.x, factionGlobalDefSize);
            Widgets.BeginScrollView(scrollRectGlobalFact, ref scrollGlobalPositionFact, scrollVertRectGlobalFact);

            yButtonPos = 5;
            if (Widgets.ButtonText(new Rect(0, yButtonPos, 190, 20), Translator.Translate("NoText")))
            {
                selectedGlobalFaction = null;
            }
            yButtonPos += 25;
            foreach (Faction fact in avaliableGlobalFactions)
            {
                if (Widgets.ButtonText(new Rect(0, yButtonPos, 190, 20), fact.Name))
                {
                    selectedGlobalFaction = fact;
                    selectedFaction = null;
                }
                yButtonPos += 22;
            }

            Widgets.EndScrollView();

            float startPos = factionPosition + 30;
            float nameFieldSize = (500 - 150);
            Widgets.Label(new Rect(0, startPos, 100, 30), Translator.Translate("CustomFactionName"));
            customFactionName = Widgets.TextField(new Rect(110, startPos, nameFieldSize, 22), customFactionName);

            /*
            startPos += 40;
            Widgets.Label(new Rect(0, startPos, 100, 30), Translator.Translate("CustomFactionLore"));
            customHistory = Widgets.TextField(new Rect(110, startPos, nameFieldSize, 22), customHistory);
            
            startPos += 40;
            if (Widgets.RadioButtonLabeled(new Rect(0, startPos, 140, 30), Translator.Translate("UseCustomColor"), useCustomColor == true))
            {
                useCustomColor = !useCustomColor;
            }
            Widgets.FloatRange(new Rect(150, startPos, 100, 30), 1222, ref newColorR, 0, 1, Translator.Translate("RedLabel"));
            Widgets.FloatRange(new Rect(250, startPos, 100, 30), 1221, ref newColorG, 0, 1, Translator.Translate("GreenLabel"));
            Widgets.FloatRange(new Rect(350, startPos, 100, 30), 1234, ref newColorB, 0, 1, Translator.Translate("BlueLabel"));

            if (Widgets.ButtonText(new Rect(100, 550, 190, 20), Translator.Translate("SetFraction")))
            {
                PrepareFaction();
            }
            */
        }
    }
}
