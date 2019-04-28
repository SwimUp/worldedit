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
    internal class PreciousLumpMenu : FWindow
    {
        public override Vector2 InitialSize => new Vector2(310, 290);
        private ThingDef resource = null;

        private SiteCoreDef core = SiteCoreDefOf.PreciousLump;
        private SitePartDef part = SitePartDefOf.AmbushHidden;

        public string time = string.Empty;

        private string threats = string.Empty;
        private float threatsFloat = 0f;

        public PreciousLumpMenu()
        {
            resizeable = false;

            resource = DefDatabase<ThingDef>.AllDefsListForReading.Where(def => def.mineable).RandomElement();
        }
        public override void DoWindowContents(Rect inRect)
        {
            Text.Font = GameFont.Small;

            Widgets.Label(new Rect(0, 0, 290, 20), Translator.Translate("PreciousLumpMenuTitle"));

            Widgets.Label(new Rect(0, 40, 80, 20), Translator.Translate("ResourceType"));
            if (Widgets.ButtonText(new Rect(90, 40, 200, 20), resource.label))
            {
                List<FloatMenuOption> list = new List<FloatMenuOption>();
                foreach (var thing in DefDatabase<ThingDef>.AllDefsListForReading)
                {
                    if (thing.mineable)
                    {
                        list.Add(new FloatMenuOption(thing.defName, delegate
                        {
                            resource = thing;
                        }));
                    }
                }
                Find.WindowStack.Add(new FloatMenu(list));
            }

            Widgets.Label(new Rect(0, 70, 290, 20), Translator.Translate("DurationTimeDaysLump"));
            time = Widgets.TextField(new Rect(0, 95, 280, 20), time);

            Widgets.Label(new Rect(0, 125, 80, 20), Translator.Translate("ThreatType"));
            if (Widgets.ButtonText(new Rect(90, 125, 200, 20), part.defName))
            {
                List<FloatMenuOption> list = new List<FloatMenuOption>();
                foreach (var p in DefDatabase<SitePartDef>.AllDefsListForReading)
                {
                        list.Add(new FloatMenuOption(p.defName, delegate
                        {
                            part = p;
                        }));
                }
                Find.WindowStack.Add(new FloatMenu(list));
            }

            Widgets.Label(new Rect(0, 155, 100, 40), Translator.Translate("ThreatPoint"));
            Widgets.TextFieldNumeric(new Rect(110, 155, 170, 20), ref threatsFloat, ref threats, 0, 2000000);

            Widgets.Label(new Rect(0, 210, 290, 20), $"Selected tile ID: {Find.WorldSelector.selectedTile}");

            if (Widgets.ButtonText(new Rect(0, 240, 290, 20), Translator.Translate("AddNewItemStash")))
            {
                AddLump();
            }
        }

        private void AddLump()
        {
            if (resource == null)
                return;

            if (Find.WorldSelector.selectedTile == -1)
                return;

            Site site = SiteMaker.TryMakeSite(core, Gen.YieldSingle(part), Find.WorldSelector.selectedTile, threatPoints: threatsFloat);
            site.sitePartsKnown = true;
            site.core.parms.preciousLumpResources = resource;

            if (int.TryParse(time, out int timeInt))
            {
                site.GetComponent<TimeoutComp>().StartTimeout(timeInt * 60000);
            }

            Find.WorldObjects.Add(site);
        }
    }

}
