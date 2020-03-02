using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using WorldEdit.Editor.WorldObjectsMenu;
using WorldEdit.Interfaces;

namespace WorldEdit.Editor
{
    internal class WorldObjectsCreator : FWindow
    {
        //universal
        private class SiteMenu : FWindow
        {
            public override Vector2 InitialSize => new Vector2(635, 700); //635
            private Vector2 scroll = Vector2.zero;
            private Vector2 scroll2 = Vector2.zero;
            private Vector2 scroll3 = Vector2.zero;

            private List<SitePartDef> parts = new List<SitePartDef>();

            private string threatPoints = string.Empty;
            private Faction selectedFaction = null;

            public SiteMenu()
            {
                resizeable = false;
            }
            public override void DoWindowContents(Rect inRect)
            {
                Text.Font = GameFont.Small;

                Widgets.Label(new Rect(0, 0, 240, 20), Translator.Translate("AddSite"));

                Widgets.Label(new Rect(330, 25, 240, 20), Translator.Translate("SiteParts"));
                int size2 = DefDatabase<SitePartDef>.AllDefsListForReading.Count * 25;
                Rect scrollRectFact2 = new Rect(330, 45, 295, 200);
                Rect scrollVertRectFact2 = new Rect(0, 0, scrollRectFact2.x, size2);
                Widgets.BeginScrollView(scrollRectFact2, ref scroll2, scrollVertRectFact2);
                int yButtonPos = 0;
                foreach (var sitePart in DefDatabase<SitePartDef>.AllDefsListForReading)
                {
                    if (Widgets.RadioButtonLabeled(new Rect(0, yButtonPos, 290, 20), sitePart.defName, parts.Contains(sitePart)))
                    {
                        if (parts.Contains(sitePart))
                            parts.Remove(sitePart);
                        else
                            parts.Add(sitePart);
                    }
                    yButtonPos += 22;
                }
                Widgets.EndScrollView();

                Widgets.Label(new Rect(0, 260, 240, 20), Translator.Translate("FactionOwner"));
                int factionDefSize = Find.FactionManager.AllFactionsListForReading.Count * 25;
                Rect scrollRectFact3 = new Rect(0, 290, 615, 200);
                Rect scrollVertRectFact3 = new Rect(0, 0, scrollRectFact3.x, factionDefSize);
                Widgets.BeginScrollView(scrollRectFact3, ref scroll3, scrollVertRectFact3);
                yButtonPos = 0;
                foreach (var spawnedFaction in Find.FactionManager.AllFactionsListForReading)
                {
                    if (Widgets.ButtonText(new Rect(0, yButtonPos, 610, 20), spawnedFaction.Name))
                    {
                        selectedFaction = spawnedFaction;
                        Messages.Message($"Selected faction: {selectedFaction.Name}", MessageTypeDefOf.NeutralEvent, false);
                    }
                    yButtonPos += 22;
                }
                Widgets.EndScrollView();

                Widgets.Label(new Rect(0, 520, 400, 23), Translator.Translate("ThreatPoint"));
                threatPoints = Widgets.TextField(new Rect(0, 550, 290, 20), threatPoints);

                Widgets.Label(new Rect(0, 600, 240, 20), $"Selected tile ID: {Find.WorldSelector.selectedTile}");

                if (Widgets.ButtonText(new Rect(0, 670, 290, 20), Translator.Translate("AddNewSite")))
                {
                    AddNewSite();
                }
            }

            private void AddNewSite()
            {
                if(Find.WorldSelector.selectedTile == -1)
                {
                    Messages.Message($"Select tile", MessageTypeDefOf.NeutralEvent, false);
                    return;
                }

                if(parts.Count == 0)
                {
                    Messages.Message($"Select at least one part", MessageTypeDefOf.NeutralEvent, false);
                    return;
                }

                if(selectedFaction == null)
                {
                    Messages.Message($"Select faction", MessageTypeDefOf.NeutralEvent, false);
                    return;
                }

                Site site;
                if (!string.IsNullOrEmpty(threatPoints) && int.TryParse(threatPoints, out int points))
                    site = SiteMaker.MakeSite(parts, Find.WorldSelector.selectedTile, selectedFaction, threatPoints: points);
                else
                    site = SiteMaker.MakeSite(parts, Find.WorldSelector.selectedTile, selectedFaction);

                site.sitePartsKnown = true;
                Find.WorldObjects.Add(site);

                Messages.Message($"Site created", MessageTypeDefOf.NeutralEvent, false);
            }
        }

        public override Vector2 InitialSize => new Vector2(350, 240);

        public WorldObjectsCreator()
        {
            resizeable = false;
        }

        public override void DoWindowContents(Rect inRect)
        {
            Text.Font = GameFont.Small;

            Widgets.Label(new Rect(0, 0, 330, 40), $"{Translator.Translate("CreateNewObject")}\nSelected tile ID: {Find.WorldSelector.selectedTile}");

            if (Widgets.ButtonText(new Rect(0, 45, 340, 20), Translator.Translate("AddAbandonedDestroyedSettlement")))
            {
                Find.WindowStack.Add(new AbandonedSettlementMenu());
            }

            if (Widgets.ButtonText(new Rect(0, 70, 340, 20), Translator.Translate("AddEscapeShipPod")))
            {
                Find.WindowStack.Add(new EscapeShipMenu());
            }

            if (Widgets.ButtonText(new Rect(0, 95, 340, 20), Translator.Translate("AddSingleObject")))
            {
                Find.WindowStack.Add(new SingleObjectEditor());
            }

            if (Widgets.ButtonText(new Rect(0, 120, 340, 20), Translator.Translate("AddItemStash")))
            {
                Find.WindowStack.Add(new StashMenu());
            }

            if (Widgets.ButtonText(new Rect(0, 145, 340, 20), Translator.Translate("AddPreciousLump")))
            {
                Find.WindowStack.Add(new PreciousLumpMenu());
            }

            if (Widgets.ButtonText(new Rect(0, 170, 340, 20), Translator.Translate("AddDownedRefugee")))
            {
                Find.WindowStack.Add(new DownedRefugeeMenu());
            }

            if (Widgets.ButtonText(new Rect(0, 195, 340, 20), Translator.Translate("AddPrisonerWillingToJoin")))
            {
                Find.WindowStack.Add(new PrisonerWillingToJoinMenu());
            }
        }
    }
}
