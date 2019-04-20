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
    internal sealed class FactionMenu : FWindow
    {
        public override Vector2 InitialSize => new Vector2(935, 600);
        private Vector2 scrollPosition = Vector2.zero;
        private Vector2 scrollPosition2 = Vector2.zero;

        /// <summary>
        /// Выбранное поселение
        /// </summary>
        private Settlement selectedSettlement = null;

        /// <summary>
        /// Редактор поселений (создание)
        /// </summary>
        private SettlementCreator settlementCreator = null;

        /// <summary>
        /// Редактор поселений (редактирование)
        /// </summary>
        private SettlementEditor settlementEditor = null;

        /// <summary>
        /// Удалить все поселения вместе с удалением фракции?
        /// </summary>
        private bool deleteSettlements = false;

        /// <summary>
        /// FactionManager римки
        /// </summary>
        private RimWorld.FactionManager rimFactionManager;

        /// <summary>
        /// Выбранная фракция
        /// </summary>
        private Faction selectedFaction = null;

        /// <summary>
        /// Редактор фракций (создание)
        /// </summary>
        internal FactionCreator factionCreator = null;

        /// <summary>
        /// Редактор фракций (редактирование)
        /// </summary>
        internal FactionEditor factionEditor = null;

        public FactionMenu()
        {
            resizeable = false;

            factionCreator = new FactionCreator();
            rimFactionManager = Find.FactionManager;
            factionEditor = new FactionEditor();
            settlementCreator = new SettlementCreator();
            settlementEditor = new SettlementEditor();
        }

        public override void DoWindowContents(Rect inRect)
        {
            Text.Font = GameFont.Small;

            WidgetRow mainRow = new WidgetRow(0, 0);
            PrintFactionManager();
            PrintSettlementManager();
        }

        private void PrintFactionManager()
        {
            Text.Font = GameFont.Small;

            Widgets.Label(new Rect(0, 0, 350, 20), Translator.Translate("FactionManagerTitle"));
            int factionDefSize = rimFactionManager.AllFactionsListForReading.Count * 25;

            if (Widgets.ButtonText(new Rect(0, 25, 450, 20), Translator.Translate("NoText")))
            {
                selectedFaction = null;
            }

            Rect scrollRectFact = new Rect(0, 50, 460, 200);
            Rect scrollVertRectFact = new Rect(0, 0, scrollRectFact.x, factionDefSize);
            Widgets.BeginScrollView(scrollRectFact, ref scrollPosition, scrollVertRectFact);
            int yButtonPos = 0;
            foreach (var spawnedFaction in rimFactionManager.AllFactionsListForReading)
            {
                if (Widgets.ButtonText(new Rect(0, yButtonPos, 450, 20), spawnedFaction.Name))
                {
                    selectedFaction = spawnedFaction;
                }
                yButtonPos += 22;
            }
            Widgets.EndScrollView();

            if (Widgets.ButtonText(new Rect(0, 265, 450, 20), Translator.Translate("AddNewFaction")))
            {
                factionCreator.Show();
            }
            if (Widgets.ButtonText(new Rect(0, 295, 450, 20), Translator.Translate("EditSelectedFaction")))
            {
                if(selectedFaction != null)
                    factionEditor.Show(selectedFaction);
            }
            if (Widgets.ButtonText(new Rect(0, 325, 450, 20), Translator.Translate("RemoveSpawnedFaction")))
            {
                RemoveFaction();
            }
            if (Widgets.RadioButtonLabeled(new Rect(0, 345, 450, 30), Translator.Translate("DeleteAllSettlements"), deleteSettlements == true))
            {
                deleteSettlements = !deleteSettlements;
            }
            if (Widgets.ButtonText(new Rect(0, 385, 450, 20), Translator.Translate("FixRelatives")))
            {
                foreach (Faction item in Find.FactionManager.AllFactionsInViewOrder)
                {
                    if(item != selectedFaction)
                        selectedFaction.TryMakeInitialRelationsWith(item);
                }
            }
        }

        private void RemoveFaction()
        {
            if (selectedFaction == null)
                return;

            if (deleteSettlements)
            {
                List<Settlement> toDelete = (Find.WorldObjects.Settlements.Where(sett => sett.Faction == selectedFaction)).ToList();
                foreach (var del in toDelete)
                {
                    Find.WorldObjects.Remove(del);
                }
            }

            if (Find.WorldPawns.Contains(selectedFaction.leader))
                Find.WorldPawns.RemoveAndDiscardPawnViaGC(selectedFaction.leader);

            Find.FactionManager.Remove(selectedFaction);
        }

        private void PrintSettlementManager()
        {
            Widgets.Label(new Rect(460, 0, 450, 20), Translator.Translate("SettlementManagerTitle"));
            int factionDefSize = Find.WorldObjects.Settlements.Count * 25;

            if (Widgets.ButtonText(new Rect(460, 25, 450, 20), Translator.Translate("NoText")))
            {
                selectedSettlement = null;
            }

            Rect scrollRectFact = new Rect(460, 50, 460, 200);
            Rect scrollVertRectFact = new Rect(0, 0, scrollRectFact.x, factionDefSize);
            Widgets.BeginScrollView(scrollRectFact, ref scrollPosition2, scrollVertRectFact);
            int yButtonPos = 0;
            foreach (var spawnedSettl in Find.WorldObjects.Settlements)
            {
                if (Widgets.ButtonText(new Rect(0, yButtonPos, 450, 20), spawnedSettl.Name))
                {
                    selectedSettlement = spawnedSettl;
                }
                yButtonPos += 22;
            }
            Widgets.EndScrollView();

            if (Widgets.ButtonText(new Rect(460, 265, 450, 20), Translator.Translate("AddNewSettlement")))
            {
                settlementCreator.Show();
            }
            if (Widgets.ButtonText(new Rect(460, 295, 450, 20), Translator.Translate("EditSelectedFaction")))
            {
                if (selectedSettlement != null)
                    settlementEditor.Show(selectedSettlement);
            }
            if (Widgets.ButtonText(new Rect(460, 325, 450, 20), Translator.Translate("RemoveSpawnedSettlement")))
            {
                RemoveSettlement();
            }
            if (Widgets.ButtonText(new Rect(460, 355, 450, 20), Translator.Translate("RemoveAllSpawnedSettlement")))
            {
                RemoveAllSettlements();
            }
        }

        private void RemoveSettlement()
        {
            if (selectedSettlement == null)
                return;

            Find.WorldObjects.Remove(selectedSettlement);
        }

        private void RemoveAllSettlements()
        {
            WorldObjectsHolder objects = Find.WorldObjects;

            Log.Message($"{objects.Settlements.Count}");

            List<Settlement> settlements = new List<Settlement>(objects.Settlements);

            foreach(var settlement in settlements)
            {
                objects.Remove(settlement);
            }

            settlements.Clear();
        }
    }
}
