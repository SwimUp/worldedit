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
    internal class FactionManager : EditWindow, IFWindow
    {
        public override Vector2 InitialSize => new Vector2(205, 320);
        private Vector2 scrollPosition = Vector2.zero;

        private bool deleteSettlements = false;

        private List<Faction> allSpawnedFactions;

        private Faction selectedFaction = null;

        private FactionCreator factionCreator = null;

        public FactionManager()
        {
            resizeable = false;

            factionCreator = new FactionCreator();
        }
        public override void DoWindowContents(Rect inRect)
        {
            Text.Font = GameFont.Small;

            Widgets.Label(new Rect(0, 0, 150, 20), Translator.Translate("FactionManagerTitle"));
            int factionDefSize = allSpawnedFactions.Count * 25;

            if (Widgets.ButtonText(new Rect(0, 25, 180, 20), Translator.Translate("NoText")))
            {
                selectedFaction = null;
            }

            Rect scrollRectFact = new Rect(0, 50, 190, 160);
            Rect scrollVertRectFact = new Rect(0, 0, scrollRectFact.x, factionDefSize);
            Widgets.BeginScrollView(scrollRectFact, ref scrollPosition, scrollVertRectFact);
            int yButtonPos = 0;
            foreach(var spawnedFaction in allSpawnedFactions)
            {
                if(Widgets.ButtonText(new Rect(0, yButtonPos, 180, 20), spawnedFaction.Name))
                {
                    selectedFaction = spawnedFaction;
                }
                yButtonPos += 22;
            }
            Widgets.EndScrollView();

            if (Widgets.RadioButtonLabeled(new Rect(0, 230, 180, 30), Translator.Translate("DeleteAllSettlements"), deleteSettlements == true))
            {
                deleteSettlements = !deleteSettlements;
            }
            if (Widgets.ButtonText(new Rect(0, 265, 190, 20), Translator.Translate("RemoveSpawnedFaction")))
            {
                RemoveFaction();
            }

            if (Widgets.ButtonText(new Rect(0, 285, 190, 20), Translator.Translate("AddNewFaction")))
            {
                factionCreator.Show();
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
            allSpawnedFactions.Remove(selectedFaction);
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
                allSpawnedFactions = Find.FactionManager.AllFactions.ToList();
            }
        }
    }
}
