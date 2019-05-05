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
   internal class ItemCreator : FWindow
    {
        public override Vector2 InitialSize => new Vector2(800, 520);
        private Vector2 scrollPosition = Vector2.zero;
        private Vector2 scrollPositionItems = Vector2.zero;
        private Vector2 scrollPositionStuff = Vector2.zero;

        private Vector2 scrollPositionFieldInfo = Vector2.zero;
        private Vector2 scrollPositionFieldInfo2 = Vector2.zero;

        private ThingCategoryDef category = null;
        private List<ThingDef> items = new List<ThingDef>();
        private List<ThingDef> stuffs = new List<ThingDef>();

        private ThingDef templateItem = null;
        private ThingDef templateStuff = null;

        private QualityCategory quality = QualityCategory.Normal;

        private ThingDef newItem = null;
        
        private FieldInfo currentField = null;
        private Dictionary<FieldInfo, string> fieldsData = new Dictionary<FieldInfo, string>();

        private string itemDefName = string.Empty;
        private bool canSeek = false;

        private FieldInfo parentField = null;

        public ItemCreator()
        {
            resizeable = false;

            category = DefDatabase<ThingCategoryDef>.GetRandom();

            UpdateItemsList();
        }

        public override void DoWindowContents(Rect inRect)
        {
            Text.Font = GameFont.Small;

            Widgets.Label(new Rect(0, 0, 450, 20), Translator.Translate("ItemCreateTitle"));
            if (Widgets.ButtonText(new Rect(260, 0, 300, 20), Translator.Translate("CreateItem")))
            {
                AddNewItem();
            }

            int defSize = 700;
            Rect scrollRectFact = new Rect(0, 50, 790, 490);
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
                        templateItem = thing;
                    }
                    x += 22;
                }
                Widgets.EndScrollView();

                if (templateItem != null && stuffs != null)
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
                            templateStuff = stuff;
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

            /*
            if (Widgets.ButtonText(new Rect(330, 30, 240, 20), Translator.Translate("GenerateItemTemplate")))
            {
                GenerateTemplate();
            }

            
            if (newItem != null && canSeek)
            {
                Widgets.Label(new Rect(330, 60, 440, 20), Translator.Translate("DefNameUnique"));
                itemDefName = Widgets.TextField(new Rect(330, 80, 440, 20), itemDefName);

                if (Widgets.ButtonText(new Rect(330, 110, 440, 20), currentField == null ? "Поле" : currentField.Name))
                {
                    List<FloatMenuOption> list = new List<FloatMenuOption>();
                    FieldInfo[] fields = newItem.GetType().GetFields();
                    foreach (var f in fields)
                    {
                        if (f.IsInitOnly || f.IsLiteral)
                            continue;

                        if (!fieldsData.ContainsKey(f))
                            continue;

                        list.Add(new FloatMenuOption(f.Name, delegate
                        {
                            currentField = f;
                        }));
                    }
                    Find.WindowStack.Add(new FloatMenu(list));
                }
                if (currentField != null)
                {
                    if (currentField.FieldType == typeof(string) || !currentField.FieldType.IsClass)
                    {
                        object v = currentField.GetValue(newItem);
                        if (v != null)
                            Widgets.LabelScrollable(new Rect(330, 135, 440, 50), $"{v}", ref scrollPositionFieldInfo);
                        else
                            Widgets.LabelScrollable(new Rect(330, 135, 440, 50), $"VALUE IS NULL", ref scrollPositionFieldInfo);

                        fieldsData[currentField] = Widgets.TextField(new Rect(330, 190, 440, 20), fieldsData[currentField]);
                    }
                    else
                    {
                        if (Widgets.ButtonText(new Rect(330, 135, 440, 20), parentField == null ? "Поле" : parentField.Name))
                        {
                            List<FloatMenuOption> list = new List<FloatMenuOption>();
                            FieldInfo[] fields = currentField.FieldType.GetFields();
                            foreach (var f in fields)
                            {
                                if (f.IsInitOnly || f.IsLiteral)
                                    continue;

                                list.Add(new FloatMenuOption(f.Name, delegate
                                {
                                    parentField = f;
                                }));
                            }
                            Find.WindowStack.Add(new FloatMenu(list));
                        }

                        if (parentField != null)
                        {
                            if (currentField.FieldType == typeof(string) || !parentField.FieldType.IsClass)
                            {
                                object v = parentField.GetValue(newItem);
                                if (v != null)
                                    Widgets.LabelScrollable(new Rect(330, 135, 440, 50), $"{v}", ref scrollPositionFieldInfo2);
                                else
                                    Widgets.LabelScrollable(new Rect(330, 135, 440, 50), $"VALUE IS NULL", ref scrollPositionFieldInfo2);

                            }
                        }
                    }

                    //Widgets.TextField(new Rect(330, 190, 440, 20), $"{v}");
                }
                
            }
            */
            Widgets.EndScrollView();
        }

        private void UpdateItemsList()
        {
            items = DefDatabase<ThingDef>.AllDefsListForReading.Where(thingDef => thingDef.IsWithinCategory(category)).ToList();
        }

        private void GenerateTemplate()
        {
            if (templateItem == null)
            {
                Messages.Message($"Select template item", MessageTypeDefOf.NeutralEvent, false);
                return;
            }

            if (templateItem.MadeFromStuff && templateStuff == null)
            {
                Messages.Message($"Select stuff", MessageTypeDefOf.NeutralEvent, false);
                return;
            }

            /*
            canSeek = false;
            
            newItem = new ThingDef();
            FieldInfo[] fields = newItem.GetType().GetFields();
            foreach (var f in fields)
            {
                object value = f.GetValue(templateItem);
                if (value == null)
                    continue;

                try
                {
                    f.SetValue(newItem, value);
                    fieldsData.Add(f, value.ToString());
                }
                catch { }
            }
            
            canSeek = true;
            */
        }

        private void UpdateItemInfo()
        {
            if(!templateItem.MadeFromStuff)
            {
                stuffs = null;
                templateStuff = null;

                return;
            }

            stuffs = DefDatabase<ThingDef>.AllDefsListForReading.Where(thingDef => thingDef.IsStuff).ToList();
        }

        private void AddNewItem()
        {
            if (string.IsNullOrEmpty(itemDefName))
                return;

            ThingDef d = DefDatabase<ThingDef>.GetNamed(itemDefName, false);
            if(d != null)
            {
                Log.Message("Give new name...");
                return;
            }

            newItem.defName = itemDefName;

            DefDatabase<ThingDef>.Add(newItem);
        }
    }
}
