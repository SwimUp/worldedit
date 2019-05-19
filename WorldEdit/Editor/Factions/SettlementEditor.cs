using RimWorld;
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
    internal sealed class SettlementEditor : FWindow
    {
        public override Vector2 InitialSize => new Vector2(510, 450);
        private Vector2 scrollPosition = Vector2.zero;

        /// <summary>
        /// Выбранное поселение
        /// </summary>
        private Settlement selectedSettlement = null;

        /// <summary>
        /// Редактор товаров поселения
        /// </summary>
        private SettlementMarket settlementMarket = null;

        public SettlementEditor()
        {
            resizeable = false;

            settlementMarket = new SettlementMarket();
        }

        public override void DoWindowContents(Rect inRect)
        {
            Text.Font = GameFont.Small;

            Widgets.Label(new Rect(0, 0, 350, 30), Translator.Translate("SettlementEditTitle"));

            Widgets.Label(new Rect(0, 40, 100, 30), Translator.Translate("SettlementNameField"));
            selectedSettlement.Name = Widgets.TextField(new Rect(105, 40, 385, 30), selectedSettlement.Name);

            int factionDefSize = Find.FactionManager.AllFactionsListForReading.Count * 25;
            Rect scrollRectFact = new Rect(0, 80, 490, 200);
            Rect scrollVertRectFact = new Rect(0, 0, scrollRectFact.x, factionDefSize);
            Widgets.BeginScrollView(scrollRectFact, ref scrollPosition, scrollVertRectFact);
            int yButtonPos = 0;
            foreach (var spawnedFaction in Find.FactionManager.AllFactionsListForReading)
            {
                if (Widgets.ButtonText(new Rect(0, yButtonPos, 480, 20), spawnedFaction.Name))
                {
                    selectedSettlement.SetFaction(spawnedFaction);
                }
                yButtonPos += 22;
            }
            Widgets.EndScrollView();

            if (Widgets.ButtonText(new Rect(0, 340, 490, 20), Translator.Translate("EditMarketList")))
            {
                settlementMarket.Show(selectedSettlement);
            }

            if (Widgets.ButtonText(new Rect(0, 370, 490, 20), Translator.Translate("SaveNewSettlement")))
            {
                SaveSettlement();
            }
        }

        public void Show(Settlement settlement)
        {
            selectedSettlement = settlement;
            Find.WindowStack.Add(this);
        }

        private void SaveSettlement()
        {
            Close();
        }
    }
}
