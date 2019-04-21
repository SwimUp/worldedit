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

namespace WorldEdit.Editor.Factions
{
   internal class ItemEditor : FWindow
    {
        public override Vector2 InitialSize => new Vector2(600, 520);
        private Vector2 scrollPosition = Vector2.zero;
        private Vector2 scrollPositionItems = Vector2.zero;
        private Vector2 scrollPositionStuff = Vector2.zero;

        private ThingCategoryDef category = null;
        private List<ThingDef> items = new List<ThingDef>();
        private List<ThingDef> stuffs = new List<ThingDef>();

        private ThingDef item = null;
        private ThingDef stuff = null;

        private List<Thing> stockList;

        private string stackBuffer = string.Empty;
        private int stackCount = 1;

        private QualityCategory quality = QualityCategory.Normal;

        public ItemEditor()
        {
            resizeable = false;

            category = DefDatabase<ThingCategoryDef>.GetRandom();

            UpdateItemsList();
        }

        public override void DoWindowContents(Rect inRect)
        {
            Text.Font = GameFont.Small;

            Widgets.Label(new Rect(0, 0, 450, 20), Translator.Translate("ItemListTitle"));
            if (Widgets.ButtonText(new Rect(260, 0, 300, 20), Translator.Translate("GenerateItem")))
            {
                GenerateItem();
            }

            int defSize = 500;
            Rect scrollRectFact = new Rect(0, 50, 640, 490);
            Rect scrollVertRectFact = new Rect(0, 0, scrollRectFact.x, defSize);
            Widgets.BeginScrollView(scrollRectFact, ref scrollPosition, scrollVertRectFact);
            Widgets.Label(new Rect(0, 0, 100, 20), Translator.Translate("Category"));
            if (Widgets.ButtonText(new Rect(110, 0, 200, 20), category.label))
            {
                List<FloatMenuOption> list = new List<FloatMenuOption>();
                foreach (var thing in DefDatabase<ThingCategoryDef>.AllDefsListForReading)
                    list.Add(new FloatMenuOption(thing.defName, delegate
                    {
                        category = thing;
                        UpdateItemsList();
                    }));
                Find.WindowStack.Add(new FloatMenu(list));
            }
            if (items != null)
            {
                int size = items.Count * 23;
                Rect scrollRectItems = new Rect(0, 40, 310, 200);
                Rect scrollVertRectItems = new Rect(0, 0, scrollRectItems.x, size);
                int x = 0;
                Widgets.BeginScrollView(scrollRectItems, ref scrollPositionItems, scrollVertRectItems);
                foreach (var thing in items)
                {
                    if (Widgets.ButtonText(new Rect(0, x, 310, 20), thing.label))
                    {
                        item = thing;
                        UpdateItemInfo();
                    }
                    x += 22;
                }
                Widgets.EndScrollView();

                if (item != null && stuffs != null)
                {
                    x = 0;
                    int size2 = stuffs.Count * 23;
                    Rect scrollRectStuff = new Rect(0, 260, 310, 200);
                    Rect scrollVertRectStuff = new Rect(0, 0, scrollRectStuff.x, size2);
                    Widgets.BeginScrollView(scrollRectStuff, ref scrollPositionStuff, scrollVertRectStuff);
                    foreach (var stuff in stuffs)
                    {
                        if (Widgets.ButtonText(new Rect(0, x, 310, 20), stuff.label))
                        {
                            this.stuff = stuff;
                        }
                        x += 22;
                    }
                    Widgets.EndScrollView();
                }

            }

            Widgets.Label(new Rect(330, 0, 100, 20), Translator.Translate("Quality"));
            if (Widgets.ButtonText(new Rect(435, 0, 140, 20), quality.GetLabel()))
            {
                List<FloatMenuOption> list = new List<FloatMenuOption>();
                foreach(QualityCategory qual in Enum.GetValues(typeof(QualityCategory)))
                {
                    list.Add(new FloatMenuOption(qual.GetLabel(), delegate
                    {
                        quality = qual;
                    }));
                }
                Find.WindowStack.Add(new FloatMenu(list));
            }

            Widgets.Label(new Rect(330, 30, 100, 20), Translator.Translate("StackCount"));
            Widgets.TextFieldNumeric(new Rect(435, 30, 140, 20), ref stackCount, ref stackBuffer, 0);

            Widgets.EndScrollView();
        }

        private void UpdateItemsList()
        {
            items = DefDatabase<ThingDef>.AllDefsListForReading.Where(thingDef => thingDef.IsWithinCategory(category)).ToList();
        }

        private void UpdateItemInfo()
        {
            if(!item.MadeFromStuff)
            {
                stuffs = null;
                stuff = null;

                return;
            }

            stuffs = DefDatabase<ThingDef>.AllDefsListForReading.Where(thingDef => thingDef.IsStuff).ToList();
            /*
            StatRequest request = StatRequest.For(item, item);

            stats = item.SpecialDisplayStats(request).ToList();
            */
        }

        private void GenerateItem()
        {
            if (item == null)
                return;

            /*
            ThingSetMakerParams parms = default(ThingSetMakerParams);
            parms.validator = x => x == item;

            var item2 = ThingSetMakerDefOf.TraderStock.root.Generate(parms).FirstOrDefault();

            var item2 = ThingMaker.MakeThing(item);
            */

            Thing thing = ThingMaker.MakeThing(item, stuff);
            thing.TryGetComp<CompQuality>()?.SetQuality(quality, ArtGenerationContext.Colony);
            if (thing.def.Minifiable)
            {
                thing = thing.MakeMinified();
            }
            thing.stackCount = stackCount;

            stockList.Add(thing);
        }

        public void Show(List<Thing> stock)
        {
            if (Find.WindowStack.IsOpen(this))
            {
                Log.Message("Currntly open...");
            }
            else
            {
                stockList = stock;
                Find.WindowStack.Add(this);
            }
        }
    }
}
