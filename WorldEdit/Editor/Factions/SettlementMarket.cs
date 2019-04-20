using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using Verse;
using WorldEdit.Editor.Factions;
using WorldEdit.Interfaces;

namespace WorldEdit.Editor
{
    internal sealed class SettlementMarket : FWindow
    {
        public override Vector2 InitialSize => new Vector2(620, 700);
        private Vector2 scrollPosition = Vector2.zero;

        /// <summary>
        /// Выбранное поселение
        /// </summary>
        private Settlement selectedSettlement = null;
        private string[] stackCount;

        private ItemEditor itemsMenu = null;

        public SettlementMarket()
        {
            resizeable = false;
            itemsMenu = new ItemEditor();
        }
        public override void DoWindowContents(Rect inRect)
        {
            Text.Font = GameFont.Small;

            Widgets.Label(new Rect(0, 0, 450, 20), Translator.Translate("SettlementMarketTitle"));

            if (selectedSettlement.trader != null)
            {
                int defSize = selectedSettlement.trader.StockListForReading.Count * 120;
                Rect scrollRectFact = new Rect(0, 50, 590, 495);
                Rect scrollVertRectFact = new Rect(0, 0, scrollRectFact.x, defSize);
                Widgets.DrawBox(new Rect(0, 49, 595, 500));
                Widgets.BeginScrollView(scrollRectFact, ref scrollPosition, scrollVertRectFact);
                int yButtonPos = 0;
                for (int i = 0; i < selectedSettlement.trader.StockListForReading.Count; i++)
                {
                    Thing good = selectedSettlement.trader.StockListForReading[i];

                    Widgets.DrawBoxSolid(new Rect(5, yButtonPos, 575, 110), new Color(0, 0, 0, 0.75f));
                    Widgets.Label(new Rect(5, yButtonPos, 570, 20), good.Label);
                    yButtonPos += 22;
                    Widgets.Label(new Rect(5, yButtonPos, 100, 20), Translator.Translate("CountMarket"));
                    Widgets.TextFieldNumeric(new Rect(110, yButtonPos, 460, 20), ref good.stackCount, ref stackCount[i], 0, int.MaxValue);
                    yButtonPos += 22;
                    Widgets.Label(new Rect(5, yButtonPos, 460, 20), $"{Translator.Translate("MarketValue")} {good.MarketValue}");
                    yButtonPos += 22;
                    if (Widgets.ButtonText(new Rect(5, yButtonPos, 600, 20), Translator.Translate("DeleteGood")))
                    {
                        selectedSettlement.trader.StockListForReading.Remove(good);
                    }
                    yButtonPos += 40;
                }
                Widgets.EndScrollView();
            }

            if(Widgets.ButtonText(new Rect(0, 560, 610, 20), Translator.Translate("RegenerateStock")))
            {
                RegenerateStock();
            }

            if(Widgets.ButtonText(new Rect(0, 590, 610, 20),Translator.Translate("AddNewItemIntoStock")))
            {
                itemsMenu.Show();
            }
        }

        public void Show(Settlement settlement)
        {
            if (Find.WindowStack.IsOpen(this))
            {
                Log.Message("Currntly open...");
            }
            else
            {
                Init(settlement);
                Find.WindowStack.Add(this);
            }
        }

        private void RegenerateStock()
        {
            selectedSettlement.trader.TryDestroyStock();
        }

        private void Init(Settlement settlement)
        {
            selectedSettlement = settlement;

            stackCount = new string[selectedSettlement.trader.StockListForReading.Count];
            for (int i = 0; i < selectedSettlement.trader.StockListForReading.Count; i++)
            {
                stackCount[i] = selectedSettlement.trader.StockListForReading[i].stackCount.ToString();
            }
        }
    }
}
