using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using WorldEdit.Editor.Factions;
using WorldEdit.Interfaces;

namespace WorldEdit.Editor.WorldObjectsMenu
{
    internal class StashMenu : FWindow
    {
        public override Vector2 InitialSize => new Vector2(640, 670);
        private Vector2 scroll = Vector2.zero;
        private Vector2 scroll2 = Vector2.zero;
        private Vector2 scroll3 = Vector2.zero;

        private SiteCoreDef core = SiteCoreDefOf.ItemStash;

        private string threats = string.Empty;
        private float threatsFloat = 0f;
        private Faction selectedFaction = null;

        private List<SitePartDef> parts = new List<SitePartDef>();

        private List<Thing> stock = new List<Thing>();

        private ItemEditor editor = new ItemEditor();

        public StashMenu()
        {
            resizeable = false;
        }
        public override void DoWindowContents(Rect inRect)
        {
            Text.Font = GameFont.Small;

            Widgets.Label(new Rect(0, 0, 240, 20), Translator.Translate("AdditemStashTitle"));

            Widgets.Label(new Rect(0, 25, 300, 20), Translator.Translate("SiteParts"));
            int size2 = DefDatabase<SitePartDef>.AllDefsListForReading.Count * 25;
            Rect scrollRectFact2 = new Rect(0, 45, 300, 200);
            Rect scrollVertRectFact2 = new Rect(0, 0, scrollRectFact2.x, size2);
            Widgets.BeginScrollView(scrollRectFact2, ref scroll, scrollVertRectFact2);
            int x = 0;
            foreach (var sitePart in DefDatabase<SitePartDef>.AllDefsListForReading)
            {
                if (Widgets.RadioButtonLabeled(new Rect(0, x, 290, 20), sitePart.defName, parts.Contains(sitePart)))
                {
                    if (parts.Contains(sitePart))
                        parts.Remove(sitePart);
                    else
                        parts.Add(sitePart);
                }
                x += 22;
            }
            Widgets.EndScrollView();

            Widgets.Label(new Rect(310, 25, 240, 20), Translator.Translate("FactionOwner"));
            int factionDefSize = Find.FactionManager.AllFactionsListForReading.Count * 25;
            Rect scrollRectFact3 = new Rect(310, 48, 300, 200);
            Rect scrollVertRectFact3 = new Rect(0, 0, scrollRectFact3.x, factionDefSize);
            Widgets.BeginScrollView(scrollRectFact3, ref scroll2, scrollVertRectFact3);
            x = 0;
            foreach (var spawnedFaction in Find.FactionManager.AllFactionsListForReading)
            {
                if (Widgets.ButtonText(new Rect(0, x, 290, 20), spawnedFaction.Name))
                {
                    selectedFaction = spawnedFaction;
                }
                x += 22;
            }
            Widgets.EndScrollView();

            Widgets.Label(new Rect(0, 270, 290, 40), Translator.Translate("ThreatPoint"));
            Widgets.TextFieldNumeric(new Rect(300, 270, 320, 20), ref threatsFloat, ref threats, 0, 2000000);

            int defSize = stock.Count * 45;
            Rect scrollRectFact = new Rect(0, 320, 620, 235);
            Rect scrollVertRectFact = new Rect(0, 0, scrollRectFact.x, defSize);
            Widgets.DrawBox(new Rect(0, 320, 620, 240));
            Widgets.BeginScrollView(scrollRectFact, ref scroll, scrollVertRectFact);
            x = 0;
            for (int i = 0; i < stock.Count; i++)
            {
                if (i >= stock.Count)
                    break;

                Thing good = stock[i];

                Widgets.DrawBoxSolid(new Rect(5, x, 615, 40), new Color(0, 0, 0, 0.75f));
                Widgets.Label(new Rect(5, x, 240, 40), good.Label);
                int.TryParse(Widgets.TextField(new Rect(245, x, 215, 40), good.stackCount.ToString()), out good.stackCount);
                if (Widgets.ButtonText(new Rect(510, x, 110, 40), Translator.Translate("DeleteGood")))
                {
                    DeleteGood(good);
                }
                x += 44;
            }
            Widgets.EndScrollView();

            if (Widgets.ButtonText(new Rect(0, 565, 630, 20), Translator.Translate("CreateNewItemToStash")))
            {
                editor.Show(stock);
            }

            Widgets.Label(new Rect(0, 600, 630, 20), $"Selected tile ID: {Find.WorldSelector.selectedTile}");

            if (Widgets.ButtonText(new Rect(0, 625, 630, 20), Translator.Translate("AddNewItemStash")))
            {
                AddItemStash();
            }
        }

        private void AddItemStash()
        {
            if (Find.WorldSelector.selectedTile == -1 || selectedFaction == null || parts.Count == 0)
                return;

            Site site;
            if (threatsFloat > 0f)
                site = SiteMaker.MakeSite(core, parts, Find.WorldSelector.selectedTile, selectedFaction, threatPoints: threatsFloat);
            else
                site = SiteMaker.MakeSite(core, parts, Find.WorldSelector.selectedTile, selectedFaction);

            site.sitePartsKnown = true;

            foreach (var thing in stock)
                site.GetComponent<ItemStashContentsComp>().contents.TryAdd(thing);

            Find.WorldObjects.Add(site);
        }

        private void DeleteGood(Thing thing)
        {
            stock.Remove(thing);
        }
    }

}
