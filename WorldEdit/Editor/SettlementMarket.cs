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
    internal class SettlementMarket : EditWindow, IFWindow
    {
        public override Vector2 InitialSize => new Vector2(600, 700);
        private Vector2 scrollPosition = Vector2.zero;

        private Settlement selectedSettlement = null;

        public SettlementMarket()
        {
            resizeable = false;
        }
        public override void DoWindowContents(Rect inRect)
        {
            Text.Font = GameFont.Small;

            int defSize = selectedSettlement.Goods.ToList().Count * 120;
            Rect scrollRectFact = new Rect(0, 50, 460, 200);
            Rect scrollVertRectFact = new Rect(0, 0, scrollRectFact.x, defSize);
            Widgets.BeginScrollView(scrollRectFact, ref scrollPosition, scrollVertRectFact);
            int yButtonPos = 0;
            foreach (var good in selectedSettlement.Goods)
            {
                Widgets.Label(new Rect(0, yButtonPos, 100, 20), good.Label);
                yButtonPos += 22;
                Widgets.Label(new Rect(0, yButtonPos, 100, 20), good.stackCount.ToString());
                yButtonPos += 22;
                Widgets.Label(new Rect(0, yButtonPos, 100, 20), good.MarketValue.ToString());
                yButtonPos += 50;
            }
            Widgets.EndScrollView();
        }

        public void Show()
        {
            if (Find.WindowStack.IsOpen(typeof(SettlementMarket)))
            {
                Log.Message("Currntly open...");
            }
            else
            {
                Find.WindowStack.Add(this);
            }
        }

        public void Show(Settlement settlement)
        {
            if (Find.WindowStack.IsOpen(typeof(SettlementMarket)))
            {
                Log.Message("Currntly open...");
            }
            else
            {
                selectedSettlement = settlement;
                Find.WindowStack.Add(this);
            }
        }
    }
}
