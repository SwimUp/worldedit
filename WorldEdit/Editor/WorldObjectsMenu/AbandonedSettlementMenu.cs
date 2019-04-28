using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using WorldEdit.Interfaces;

namespace WorldEdit.Editor.WorldObjectsMenu
{
    internal class AbandonedSettlementMenu : FWindow
    {
        public override Vector2 InitialSize => new Vector2(300, 320);
        private Vector2 scroll = Vector2.zero;
        private Settlement selectedSettlement = null;

        private WorldObjectDef type = WorldObjectDefOf.AbandonedSettlement;

        public AbandonedSettlementMenu()
        {
            resizeable = false;
        }
        public override void DoWindowContents(Rect inRect)
        {
            Text.Font = GameFont.Small;

            Widgets.Label(new Rect(0, 0, 290, 20), Translator.Translate("AddAbandonedDestroyedSettlement"));

            int factionDefSize = Find.WorldObjects.Settlements.Count * 25;

            Widgets.Label(new Rect(0, 20, 240, 20), Translator.Translate("FactionOwner"));
            Rect scrollRectFact = new Rect(0, 45, 295, 180);
            Rect scrollVertRectFact = new Rect(0, 0, scrollRectFact.x, factionDefSize);
            Widgets.BeginScrollView(scrollRectFact, ref scroll, scrollVertRectFact);
            int yButtonPos = 0;
            foreach (var spawnedSettl in Find.WorldObjects.Settlements)
            {
                if (Widgets.ButtonText(new Rect(0, yButtonPos, 290, 20), spawnedSettl.Name))
                {
                    selectedSettlement = spawnedSettl;
                }
                yButtonPos += 22;
            }
            Widgets.EndScrollView();

            Widgets.Label(new Rect(0, 230, 240, 20), $"Selected tile ID: {Find.WorldSelector.selectedTile}");

            if (type == WorldObjectDefOf.AbandonedSettlement)
            {
                if (Widgets.ButtonText(new Rect(0, 255, 290, 20), Translator.Translate("AbandonedType")))
                {
                    type = WorldObjectDefOf.DestroyedSettlement;
                }
            }
            else
            {
                if (Widgets.ButtonText(new Rect(0, 255, 290, 20), Translator.Translate("DestroyedType")))
                {
                    type = WorldObjectDefOf.AbandonedSettlement;
                }
            }

            if (Widgets.ButtonText(new Rect(0, 275, 290, 20), Translator.Translate("SetAbandonedDestroyedSettlement")))
            {
                AddSettlementObject();
            }

        }

        private void AddSettlementObject()
        {
            if (Find.WorldSelector.selectedTile == -1)
                return;

            if (selectedSettlement == null)
                return;

            Utils.CreateWorldObject(type, Find.WorldSelector.selectedTile, selectedSettlement.Faction);
        }
    }

}
