﻿using RimWorld;
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
    internal class FactionManager : EditWindow, IFWindow
    {
        public override Vector2 InitialSize => new Vector2(205, 355);
        private Vector2 scrollPosition = Vector2.zero;

        private bool deleteSettlements = false;

        private RimWorld.FactionManager rimFactionManager;
        //private List<Faction> allSpawnedFactions;

        private Faction selectedFaction = null;

        internal FactionCreator factionCreator = null;
        internal FactionEditor factionEditor = null;

        public FactionManager()
        {
            resizeable = false;

            factionCreator = new FactionCreator();
            rimFactionManager = Find.FactionManager;
            factionEditor = new FactionEditor();
        }
        public override void DoWindowContents(Rect inRect)
        {
            Text.Font = GameFont.Small;

            Widgets.Label(new Rect(0, 0, 150, 20), Translator.Translate("FactionManagerTitle"));
            int factionDefSize = rimFactionManager.AllFactionsListForReading.Count * 25;

            if (Widgets.ButtonText(new Rect(0, 25, 180, 20), Translator.Translate("NoText")))
            {
                selectedFaction = null;
            }

            Rect scrollRectFact = new Rect(0, 50, 190, 170);
            Rect scrollVertRectFact = new Rect(0, 0, scrollRectFact.x, factionDefSize);
            Widgets.BeginScrollView(scrollRectFact, ref scrollPosition, scrollVertRectFact);
            int yButtonPos = 0;
            foreach(var spawnedFaction in rimFactionManager.AllFactionsListForReading)
            {
                if(Widgets.ButtonText(new Rect(0, yButtonPos, 180, 20), spawnedFaction.Name))
                {
                    selectedFaction = spawnedFaction;
                }
                yButtonPos += 22;
            }
            Widgets.EndScrollView();

            if (Widgets.ButtonText(new Rect(0, 230, 190, 20), Translator.Translate("AddNewFaction")))
            {
                factionCreator.Show();
            }
            if (Widgets.ButtonText(new Rect(0, 265, 190, 20), Translator.Translate("EditSelectedFaction")))
            {
                factionEditor.Show(selectedFaction);
            }
            if (Widgets.ButtonText(new Rect(0, 285, 190, 20), Translator.Translate("RemoveSpawnedFaction")))
            {
                RemoveFaction();
            }
            if (Widgets.RadioButtonLabeled(new Rect(0, 315, 180, 30), Translator.Translate("DeleteAllSettlements"), deleteSettlements == true))
            {
                deleteSettlements = !deleteSettlements;
            }
        }

        private void RemoveFaction()
        {
            if (selectedFaction == null)
                return;

            if (deleteSettlements)
            {
                List<Settlement> toDelete = (Find.WorldObjects.Settlements.Where(sett => sett.Faction == selectedFaction)).ToList();
                foreach(var del in toDelete)
                {
                    Find.WorldObjects.Remove(del);
                }
            }

            Find.FactionManager.Remove(selectedFaction);
        }

        public void Show()
        {
            if (Find.WindowStack.IsOpen(typeof(FactionManager)))
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