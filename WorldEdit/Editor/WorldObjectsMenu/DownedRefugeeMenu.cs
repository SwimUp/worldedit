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
    internal class DownedRefugeeMenu : FWindow
    {
        public override Vector2 InitialSize => new Vector2(500, 590); //635
        private Vector2 scroll = Vector2.zero;
        private Vector2 scroll2 = Vector2.zero;

        private List<SitePartDef> parts = new List<SitePartDef>();

        public string time = string.Empty;

        private string threats = string.Empty;
        private float threatsFloat = 0f;

        private List<Pawn> pawnList = new List<Pawn>();

        private Faction selectedFaction = null;

        private bool edit = false;
        private Site editSite = null;

        public DownedRefugeeMenu()
        {
            resizeable = false;
            edit = false;
        }

        public DownedRefugeeMenu(Site s)
        {
            resizeable = false;
            edit = true;

            editSite = s;

            selectedFaction = editSite.Faction;

            parts.Clear();
            foreach (var p in editSite.parts)
            {
                parts.Add(p.def);
            }

            threatsFloat = editSite.desiredThreatPoints;

            var pawns = editSite.GetComponent<DownedRefugeeComp>();
            pawnList.Clear();
            foreach(var p in pawns.pawn)
            {
                pawnList.Add(p);
            }

            time = (editSite.GetComponent<TimeoutComp>().TicksLeft / 60000).ToString();
        }
        public override void DoWindowContents(Rect inRect)
        {
            Text.Font = GameFont.Small;

            Widgets.Label(new Rect(0, 0, 290, 20), Translator.Translate("DownedRefugeeMenuTitle"));

            Widgets.Label(new Rect(0, 40, 400, 20), Translator.Translate("ThreatTypes"));
            int size2 = DefDatabase<SitePartDef>.AllDefsListForReading.Count * 25;
            Rect scrollRectFact = new Rect(0, 65, 485, 200);
            Rect scrollVertRectFact = new Rect(0, 0, scrollRectFact.x, size2);
            Widgets.BeginScrollView(scrollRectFact, ref scroll, scrollVertRectFact);
            int x = 0;
            foreach (var sitePart in DefDatabase<SitePartDef>.AllDefsListForReading)
            {
                if (Widgets.RadioButtonLabeled(new Rect(0, x, 480, 20), sitePart.defName, parts.Contains(sitePart)))
                {
                    if (parts.Contains(sitePart))
                        parts.Remove(sitePart);
                    else
                        parts.Add(sitePart);
                }
                x += 22;
            }
            Widgets.EndScrollView();

            if(Widgets.ButtonText(new Rect(0, 240, 490, 20), Translator.Translate("OpenPawnMenu")))
            {
                DrawPawnMenu();
            }
            Widgets.Label(new Rect(0, 270, 490, 20), $"Pawns: {pawnList.Count}");

            Widgets.Label(new Rect(0, 300, 490, 20), Translator.Translate("DurationTimeDaysLump"));
            time = Widgets.TextField(new Rect(0, 300, 480, 20), time);

            Widgets.Label(new Rect(0, 330, 100, 40), Translator.Translate("ThreatPoint"));
            Widgets.TextFieldNumeric(new Rect(110, 330, 370, 20), ref threatsFloat, ref threats, 0, 2000000);

            Widgets.Label(new Rect(0, 365, 290, 20), Translator.Translate("FactionForSort"));
            int factionDefSize = Find.FactionManager.AllFactionsListForReading.Count * 25;
            Rect scrollRectFact3 = new Rect(0, 390, 490, 110);
            Rect scrollVertRectFact3 = new Rect(0, 0, scrollRectFact3.x, factionDefSize);
            Widgets.BeginScrollView(scrollRectFact3, ref scroll2, scrollVertRectFact3);
            x = 0;
            foreach (var spawnedFaction in Find.FactionManager.AllFactionsListForReading)
            {
                if (Widgets.ButtonText(new Rect(0, x, 485, 20), spawnedFaction.Name))
                {
                    selectedFaction = spawnedFaction;
                }
                x += 22;
            }
            Widgets.EndScrollView();

            Widgets.Label(new Rect(0, 510, 290, 20), $"Selected tile ID: {Find.WorldSelector.selectedTile}");

            if (Widgets.ButtonText(new Rect(0, 550, 490, 20), Translator.Translate("AddNewDownedRefugee")))
            {
                AddNewDownedRefugee();
            }
        }

        private void AddNewDownedRefugee()
        {
            if (pawnList.Count == 0)
            {
                Messages.Message($"Select minimum 1 pawn", MessageTypeDefOf.NeutralEvent, false);
                return;
            }

            if(edit)
            {
                editSite.parts.Clear();
                foreach (var p in parts)
                    editSite.parts.Add(new SitePart(editSite, p, p.Worker.GenerateDefaultParams(threatsFloat, editSite.Tile, editSite.Faction)));

                var pawns = editSite.GetComponent<DownedRefugeeComp>();
                pawns.pawn.Clear();

                foreach (var p in pawnList)
                {
                    pawns.pawn.TryAdd(p);
                }

                if (!string.IsNullOrEmpty(time) && int.TryParse(time, out int t))
                {
                    editSite.GetComponent<TimeoutComp>().StartTimeout(t * 60000);
                }
                else
                {
                    editSite.GetComponent<TimeoutComp>().StartTimeout(-1);
                }

                if (selectedFaction != null)
                    editSite.SetFaction(selectedFaction);

                Messages.Message($"Success", MessageTypeDefOf.NeutralEvent, false);

                return;
            }

            if (Find.WorldSelector.selectedTile == -1)
            {
                Messages.Message($"Select tile", MessageTypeDefOf.NeutralEvent, false);
                return;
            }

            if(selectedFaction == null)
            {
                selectedFaction = Find.FactionManager.AllFactionsListForReading.RandomElement();
            }

            Site site = SiteMaker.MakeSite(parts, Find.WorldSelector.selectedTile, selectedFaction, threatPoints: threatsFloat);
            site.sitePartsKnown = true;

            var comp = site.GetComponent<DownedRefugeeComp>();
            foreach(var p in pawnList)
            {
                comp.pawn.TryAdd(p);
            }

            if (!string.IsNullOrEmpty(time) && int.TryParse(time, out int timeInt))
            {
                site.GetComponent<TimeoutComp>().StartTimeout(timeInt * 60000);
            }

            Find.WorldObjects.Add(site);
        }

        private void DrawPawnMenu()
        {
            Find.WindowStack.Add(new PawnMenu(pawnList));
        }
    }

}
