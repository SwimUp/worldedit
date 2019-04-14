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
    internal class SettlementManager : EditWindow, IFWindow
    {
        public override Vector2 InitialSize => new Vector2(200, 260);
        private Vector2 scrollPosition = Vector2.zero;

        private List<Settlement> alLSpawnedSettlements;

        private Settlement selectedSettlement = null;

        public SettlementManager()
        {
            resizeable = false;
        }
        public override void DoWindowContents(Rect inRect)
        {
            Text.Font = GameFont.Small;

            Widgets.Label(new Rect(0, 0, 150, 20), Translator.Translate("SettlementManagerTitle"));
            int factionDefSize = alLSpawnedSettlements.Count * 25;

            if (Widgets.ButtonText(new Rect(0, 25, 180, 20), Translator.Translate("NoText")))
            {
                selectedSettlement = null;
            }

            Rect scrollRectFact = new Rect(0, 50, 190, 200);
            Rect scrollVertRectFact = new Rect(0, 0, scrollRectFact.x, factionDefSize);
            Widgets.BeginScrollView(scrollRectFact, ref scrollPosition, scrollVertRectFact);
            int yButtonPos = 0;
            foreach(var spawnedSettl in alLSpawnedSettlements)
            {
                if(Widgets.ButtonText(new Rect(0, yButtonPos, 180, 20), spawnedSettl.Name))
                {
                    selectedSettlement = spawnedSettl;
                }
                yButtonPos += 22;
            }
            Widgets.EndScrollView();

            yButtonPos = 230;

            if (Widgets.ButtonText(new Rect(0, yButtonPos, 180, 20), Translator.Translate("RemoveSpawnedSettlement")))
            {
                RemoveSettlement();
            }
        }

        private void RemoveSettlement()
        {
            if (selectedSettlement == null)
                return;

            Find.WorldObjects.Remove(selectedSettlement);

            alLSpawnedSettlements.Remove(selectedSettlement);
        }

        public void Show()
        {
            if (Find.WindowStack.IsOpen(typeof(SettlementManager)))
            {
                Log.Message("Currntly open...");
            }
            else
            {
                Find.WindowStack.Add(this);
                alLSpawnedSettlements = Find.WorldObjects.Settlements;
            }
        }
    }
}
