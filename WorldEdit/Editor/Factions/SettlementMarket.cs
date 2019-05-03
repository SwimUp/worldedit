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

        private ItemEditor itemsMenu = null;

        internal List<Thing> stockList = new List<Thing>();

        public SettlementMarket()
        {
            resizeable = false;
            itemsMenu = new ItemEditor();
        }
        public override void DoWindowContents(Rect inRect)
        {
            Text.Font = GameFont.Small;

            Widgets.Label(new Rect(0, 0, 450, 20), Translator.Translate("SettlementMarketTitle"));

            int defSize = stockList.Count * 45;
            Rect scrollRectFact = new Rect(0, 50, 590, 495);
            Rect scrollVertRectFact = new Rect(0, 0, scrollRectFact.x, defSize);
            Widgets.DrawBox(new Rect(0, 49, 595, 500));
            Widgets.BeginScrollView(scrollRectFact, ref scrollPosition, scrollVertRectFact);
            int x = 0;
            for (int i = 0; i < stockList.Count; i++)
            {
                if (i >= stockList.Count)
                    break;

                Thing good = stockList[i];

                Widgets.DrawBoxSolid(new Rect(5, x, 575, 40), new Color(0, 0, 0, 0.75f));
                Widgets.Label(new Rect(5, x, 240, 40), good.Label);
                int.TryParse(Widgets.TextField(new Rect(245, x, 215, 40), good.stackCount.ToString()), out good.stackCount);
                if (Widgets.ButtonText(new Rect(460, x, 110, 40), Translator.Translate("DeleteGood")))
                {
                    DeleteGood(good);
                }
                x += 44;
            }
            Widgets.EndScrollView();

            if (Widgets.ButtonText(new Rect(0, 560, 610, 20), Translator.Translate("RegenerateStock")))
            {
                RegenerateStock();
            }

            if (Widgets.ButtonText(new Rect(0, 590, 610, 20), Translator.Translate("AddNewItemIntoStock")))
            {
                itemsMenu.Show(stockList);
            }
        }

        private void DeleteGood(Thing good)
        {
            selectedSettlement.trader.StockListForReading.Remove(good);

            UpdateStock();
        }

        public void Show(Settlement settlement)
        {
            Init(settlement);
            Find.WindowStack.Add(this);
        }

        private void RegenerateStock()
        {
            selectedSettlement.trader.TryDestroyStock();

            UpdateStock();
        }

        private void UpdateStock()
        {
            stockList = selectedSettlement.trader.StockListForReading;
        }

        private void Init(Settlement settlement)
        {
            selectedSettlement = settlement;

            stockList = selectedSettlement.trader.StockListForReading;
        }
    }
}
