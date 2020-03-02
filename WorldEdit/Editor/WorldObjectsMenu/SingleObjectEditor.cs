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
    internal class SingleObjectEditor : FWindow
    {
        public override Vector2 InitialSize => new Vector2(640, 490);
        private Vector2 scroll = Vector2.zero;
        private Vector2 scroll2 = Vector2.zero;

        private Faction selectedFaction = null;
        private string threats = string.Empty;
        private float threatsFloat = 0f;

        private SitePartDef part = SitePartDefOf.Outpost;

        private bool edit = false;
        private Site editSite = null;

        public SingleObjectEditor()
        {
            resizeable = false;

            edit = false;
        }
        public SingleObjectEditor(Site s)
        {
            resizeable = false;

            edit = true;
            editSite = s;

            part = editSite.parts[0].def;
            selectedFaction = editSite.Faction;

            threatsFloat = editSite.desiredThreatPoints;
        }

        public override void DoWindowContents(Rect inRect)
        {
            Text.Font = GameFont.Small;

            Widgets.Label(new Rect(0, 0, 390, 20), Translator.Translate("AddSingleObjectTitle"));

            Widgets.Label(new Rect(310, 40, 240, 20), Translator.Translate("SiteParts"));
            int size2 = DefDatabase<SitePartDef>.AllDefsListForReading.Count * 25;
            Rect scrollRectFact2 = new Rect(310, 60, 300, 200);
            Rect scrollVertRectFact2 = new Rect(0, 0, scrollRectFact2.x, size2);
            Widgets.BeginScrollView(scrollRectFact2, ref scroll2, scrollVertRectFact2);
            int x = 0;
            foreach (var sitePart in DefDatabase<SitePartDef>.AllDefsListForReading)
            {
                if (Widgets.RadioButtonLabeled(new Rect(0, x, 295, 20), sitePart.defName, part == sitePart))
                {
                    part = sitePart;
                }
                x += 22;
            }
            Widgets.EndScrollView();

            Widgets.Label(new Rect(0, 40, 390, 20), Translator.Translate("FactionOwner"));
            int factionDefSize = Find.FactionManager.AllFactionsListForReading.Count * 25;
            Rect scrollRectFact = new Rect(0, 60, 300, 200);
            Rect scrollVertRectFact = new Rect(0, 0, scrollRectFact.x, factionDefSize);
            Widgets.BeginScrollView(scrollRectFact, ref scroll, scrollVertRectFact);
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

            Widgets.Label(new Rect(0, 290, 350, 40), Translator.Translate("ThreatPoint"));
            Widgets.TextFieldNumeric(new Rect(360, 290, 260, 20), ref threatsFloat, ref threats, 0, 2000000);

            Widgets.Label(new Rect(0, 390, 600, 20), $"Selected tile ID: {Find.WorldSelector.selectedTile}");

            if (Widgets.ButtonText(new Rect(0, 450, 630, 20), Translator.Translate("AddNewSingleObject")))
            {
                AddSingleObject();
            }
        }

        private void AddSingleObject()
        {
            if (selectedFaction == null)
            {
                Messages.Message($"Select faction", MessageTypeDefOf.NeutralEvent, false);
                return;
            }

            if(edit)
            {
                editSite.parts.Clear();
                editSite.parts.Add(new SitePart(editSite, part, part.Worker.GenerateDefaultParams(threatsFloat, editSite.Tile, editSite.Faction)));

                editSite.SetFaction(selectedFaction);

                Messages.Message($"Success", MessageTypeDefOf.NeutralEvent, false);

                return;
            }

            if (Find.WorldSelector.selectedTile == -1)
            {
                Messages.Message($"Select tile", MessageTypeDefOf.NeutralEvent, false);
                return;
            }

            Site site;
            site = SiteMaker.MakeSite(part, Find.WorldSelector.selectedTile, selectedFaction, threatPoints: threatsFloat);

            site.sitePartsKnown = true;
            Find.WorldObjects.Add(site);
        }
    }
}
