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
    internal sealed class SettlementCreator : FWindow
    {
        public override Vector2 InitialSize => new Vector2(510, 450);
        private Vector2 scrollPosition = Vector2.zero;

        /// <summary>
        /// Имя поселения
        /// </summary>
        private string settlementName = string.Empty;

        /// <summary>
        /// Фракция поселения
        /// </summary>
        private Faction selectedFaction;

        /// <summary>
        /// Редактор товаров поселения
        /// </summary>
        private SettlementMarket settlementMarket = null;

        /// <summary>
        /// Новое поселение
        /// </summary>
        private Settlement newSettlement = null;

        public SettlementCreator()
        {
            resizeable = false;

            settlementMarket = new SettlementMarket();
        }

        public override void DoWindowContents(Rect inRect)
        {
            Text.Font = GameFont.Small;

            Widgets.Label(new Rect(0, 0, 350, 30), Translator.Translate("SettlementCreatorTitle"));

            Widgets.Label(new Rect(0, 40, 100, 30), Translator.Translate("SettlementNameField"));
            settlementName = Widgets.TextField(new Rect(105, 40, 385, 30), settlementName);

            int factionDefSize = Find.FactionManager.AllFactionsListForReading.Count * 25;
            Rect scrollRectFact = new Rect(0, 80, 490, 200);
            Rect scrollVertRectFact = new Rect(0, 0, scrollRectFact.x, factionDefSize);
            Widgets.BeginScrollView(scrollRectFact, ref scrollPosition, scrollVertRectFact);
            int yButtonPos = 0;
            foreach (var spawnedFaction in Find.FactionManager.AllFactionsListForReading)
            {
                if (Widgets.ButtonText(new Rect(0, yButtonPos, 480, 20), spawnedFaction.Name))
                {
                    selectedFaction = spawnedFaction;
                    Messages.Message($"Selected faction: {selectedFaction.Name}", MessageTypeDefOf.NeutralEvent, false);
                }
                yButtonPos += 22;
            }
            Widgets.EndScrollView();

            if (Widgets.ButtonText(new Rect(0, 330, 490, 20), Translator.Translate("CreateNewSettlement")))
            {
                CreateSettlement();
            }
        }

        private void CreateSettlement()
        {
            if (selectedFaction == null)
            {
                Messages.Message($"Select faction", MessageTypeDefOf.NeutralEvent, false);
                return;
            }

            if (string.IsNullOrEmpty(settlementName))
            {
                Messages.Message($"Enter valid settlement name", MessageTypeDefOf.NeutralEvent, false);
                return;
            }

            if (Find.WorldSelector.selectedTile < 0)
            {
                Messages.Message($"Select tile", MessageTypeDefOf.NeutralEvent, false);
                return;
            }

            if (Find.WorldObjects.AnySettlementAt(Find.WorldSelector.selectedTile))
            {
                Messages.Message($"Some settlement is already in this place", MessageTypeDefOf.NeutralEvent, false);
                return;
            }

            newSettlement = (Settlement)WorldObjectMaker.MakeWorldObject(WorldObjectDefOf.Settlement);
            newSettlement.SetFaction(selectedFaction);
            newSettlement.Tile = Find.WorldSelector.selectedTile;
            newSettlement.Name = settlementName;
            Find.WorldObjects.Add(newSettlement);
        }
    }
}
