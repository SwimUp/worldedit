using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Verse;
using WorldEdit.Interfaces;

namespace WorldEdit.Editor
{
    internal class FactionCreator : EditWindow, IFWindow
    {
        private class RelativeMenu : EditWindow, IFWindow
        {
            private Vector2 scrollPositionFact = Vector2.zero;
            private RimWorld.FactionManager rimFactionManager;

            public override Vector2 InitialSize => new Vector2(300, 200);

            public RelativeMenu()
            {
                resizeable = false;

                rimFactionManager = Find.FactionManager;
            }

            public override void DoWindowContents(Rect inRect)
            {
                Text.Font = GameFont.Small;

                Widgets.Label(new Rect(80, 5, 200, 20), Translator.Translate("AddRelativeTitle"));
                int factionDefSize = rimFactionManager.AllFactionsListForReading.Count * 25;
                Rect scrollRectFact = new Rect(10, 30, 290, inRect.height - 60);
                Rect scrollVertRectFact = new Rect(0, 0, scrollRectFact.x, factionDefSize);
                Widgets.BeginScrollView(scrollRectFact, ref scrollPositionFact, scrollVertRectFact);

                int yButtonPos = 5;
                foreach (Faction f in rimFactionManager.AllFactionsListForReading)
                {
                    if (Widgets.ButtonText(new Rect(0, yButtonPos, 260, 20), f.Name))
                    {
                        TryAddRelative(f);
                    }
                    yButtonPos += 22;
                }
                Widgets.EndScrollView();

                if (Widgets.ButtonText(new Rect(10, inRect.height - 20, 260, 20), Translator.Translate("Cancel")))
                {
                    Close();
                }
            }

            private void TryAddRelative(Faction f)
            {
                FactionCreator creator = MainMenu.Editor.factionEditor.factionManager.factionCreator;
                Faction faction = creator.newFaction;
                if (faction == null)
                    return;

                foreach(FactionRelation rel in creator.newFactionRelation)
                {
                    if (rel.other == f)
                        return;
                }

                creator.newFaction.TryMakeInitialRelationsWith(f);

                //f.TryMakeInitialRelationsWith(creator.newFaction);

                creator.UpdateFactionInfo();
            }

            public void Show()
            {
                if (Find.WindowStack.IsOpen(typeof(RelativeMenu)))
                {
                    Log.Message("Currntly open...");
                }
                else
                {
                    Find.WindowStack.Add(this);
                }
            }
        }

        public override Vector2 InitialSize => new Vector2(740, 450);

        private RelativeMenu relativeMenu;

        private FactionDef selectedFaction = null;
        public Faction newFaction = null;
        public List<FactionRelation> selectedRelative = new List<FactionRelation>();

        private Vector2 scrollPositionFact = Vector2.zero;
        private Vector2 scrollFieldsPos = Vector2.zero;
        private Vector2 scrollRel = Vector2.zero;

        private List<FactionDef> avaliableFactions;
        private List<FactionRelation> newFactionRelation;
        private FloatRange color;

        private string tempGoodwill;

        public FactionCreator()
        {
            resizeable = false;

            avaliableFactions = DefDatabase<FactionDef>.AllDefs.ToList();
            relativeMenu = new RelativeMenu();
        }

        public override void DoWindowContents(Rect inRect)
        {
            Text.Font = GameFont.Small;

            Widgets.Label(new Rect(inRect.center.x - 60, 0, 250, 20), Translator.Translate("FactionCreatorTitle"));
            WidgetRow row = new WidgetRow(330, 25);
            if (row.ButtonText(Translator.Translate("RandomizeFaction")))
            {
                newFaction = GenerateFaction();
            }
            if(row.ButtonText(Translator.Translate("RandomizeName")))
            {
                GenerateName(newFaction);
            }
            if (row.ButtonText(Translator.Translate("ClearFaction")))
            {
                newFaction = null;
                selectedFaction = null;
            }

            int factionDefSize = avaliableFactions.Count * 25;
            Rect scrollRectFact = new Rect(0, 25, 320, inRect.height - 100);
            Rect scrollVertRectFact = new Rect(0, 0, scrollRectFact.x, factionDefSize);
            Widgets.BeginScrollView(scrollRectFact, ref scrollPositionFact, scrollVertRectFact);

            int yButtonPos = 5;
            if (Widgets.ButtonText(new Rect(0, yButtonPos, 320, 20), Translator.Translate("NoText")))
            {
                selectedFaction = null;
            }
            yButtonPos += 25;
            foreach (FactionDef def in avaliableFactions)
            {
                if (Widgets.ButtonText(new Rect(0, yButtonPos, 320, 20), def.label))
                {
                    selectedFaction = def;
                    newFaction = GenerateFaction();
                }
                yButtonPos += 22;
            }
            Widgets.EndScrollView();

            Rect scrollRectGlobalFact = new Rect(330, 50, 380, inRect.height - 20);
            Rect scrollVertRectGlobalFact = new Rect(0, 0, scrollRectGlobalFact.x, inRect.height - 10);
            Widgets.BeginScrollView(scrollRectGlobalFact, ref scrollFieldsPos, scrollVertRectGlobalFact);
            if (newFaction != null)
            {
                Widgets.Label(new Rect(0, 5, 150, 30), Translator.Translate("FactionName"));
                newFaction.Name = Widgets.TextField(new Rect(160, 5, 180, 30), newFaction.Name);

                Widgets.Label(new Rect(0, 40, 150, 30), Translator.Translate("FactionDefeated"));
                if (newFaction.defeated)
                {
                    if (Widgets.ButtonText(new Rect(160, 40, 180, 30), Translator.Translate("isDefeatedYES")))
                    {
                        newFaction.defeated = false;
                    }
                }
                else
                {
                    if (Widgets.ButtonText(new Rect(160, 40, 180, 30), Translator.Translate("isDefeatedNO")))
                    {
                        newFaction.defeated = true;
                    }
                }

                Widgets.Label(new Rect(0, 70, 180, 30), Translator.Translate("FactionRelative"));
                int y = 15;
                int boxY = 5;
                Rect scrollRectRel = new Rect(0, 90, 370, 160);
                Rect scrollVertRectRel = new Rect(0, 0, scrollRectRel.x, newFactionRelation.Count * 145);
                Widgets.DrawBox(new Rect(0, 90, 350, 160));
                Widgets.BeginScrollView(scrollRectRel, ref scrollRel, scrollVertRectRel);
                foreach (var rel in newFactionRelation)
                {
                    Widgets.DrawBox(new Rect(2, boxY, 340, 130));

                    if(Widgets.RadioButtonLabeled(new Rect(315, y, 20, 20), "", selectedRelative.Contains(rel)))
                    {
                        if (selectedRelative.Contains(rel))
                        {
                            selectedRelative.Remove(rel);
                        }
                        else
                        {
                            selectedRelative.Add(rel);
                        }
                    }

                    Widgets.Label(new Rect(5, y, 315, 30), $"{Translator.Translate("FactionInfoName")} {rel.other.Name}");

                    y += 35;
                    Widgets.Label(new Rect(5, y, 140, 30), Translator.Translate("FactionGoodness"));
                    tempGoodwill = Widgets.TextField(new Rect(150, y, 130, 30), tempGoodwill);
                    int.TryParse(tempGoodwill, out rel.goodwill);
                    Log.Message($"Goodwill: {rel.goodwill}");
                    //int.TryParse(Widgets.TextField(new Rect(150, y, 130, 30), rel.goodwill.ToString()), out rel.goodwill);

                    y += 35;
                    Log.Message(rel.kind.GetLabel());
                    switch(rel.kind)
                    {
                        case FactionRelationKind.Ally:
                            {
                                if(Widgets.ButtonText(new Rect(5, y, 180, 30), rel.kind.GetLabel()))
                                {
                                    rel.kind = FactionRelationKind.Neutral;
                                }
                                break;
                            }
                        case FactionRelationKind.Neutral:
                            {
                                if (Widgets.ButtonText(new Rect(5, y, 180, 30), rel.kind.GetLabel()))
                                {
                                    rel.kind = FactionRelationKind.Hostile;
                                }
                                break;
                            }
                        case FactionRelationKind.Hostile:
                            {
                                if (Widgets.ButtonText(new Rect(5, y, 180, 30), rel.kind.GetLabel()))
                                {
                                    rel.kind = FactionRelationKind.Ally;
                                }
                                break;
                            }
                    }

                    boxY += 140;
                    y = boxY + 10;
                }
                Widgets.EndScrollView();
                
                if(Widgets.ButtonText(new Rect(0, 260, 100, 30), Translator.Translate("AddNewRelative")))
                {
                    relativeMenu.Show();
                }
                if (Widgets.ButtonText(new Rect(110, 260, 150, 30), Translator.Translate("GenerateRelative")))
                {
                    //GenerateName(newFaction);
                }
                if (Widgets.ButtonText(new Rect(270, 260, 100, 30), Translator.Translate("DeleteRelative")))
                {
                    RemoveRelatives();
                }

                Widgets.Label(new Rect(0, 310, 180, 30), Translator.Translate("FactionMelanin"));
                float.TryParse(Widgets.TextField(new Rect(195, 310, 160, 30), newFaction.centralMelanin.ToString()), out newFaction.centralMelanin);

                Widgets.Label(new Rect(0, 350, 160, 30), Translator.Translate("ColorSpectrum"));
                Widgets.FloatRange(new Rect(195, 350, 130, 30), 42, ref color, 0, 1);
                if(newFaction.def.colorSpectrum != null)
                    Widgets.DrawBoxSolid(new Rect(165, 350, 20, 20), ColorsFromSpectrum.Get(newFaction.def.colorSpectrum, color.max));


            }
            Widgets.EndScrollView();

            if (Widgets.ButtonText(new Rect(0, inRect.height - 30, 320, 20), Translator.Translate("CreateNewFaction")))
            {
                CreateFaction();
            }
        }

        private void RemoveRelatives()
        {
            if (selectedRelative == null)
                return;

            if (selectedRelative.Count <= 0)
                return;

            foreach(var rel in selectedRelative)
            {
                newFactionRelation.Remove(rel);
            }

            FieldInfo relations = typeof(Faction).GetField("relations", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            relations.SetValue(newFaction, newFactionRelation);
        }

        private void CreateFaction()
        {
            if (newFaction == null)
                return;

            newFaction.loadID = Find.UniqueIDsManager.GetNextFactionID();
            newFaction.randomKey = Rand.Range(0, int.MaxValue);
            newFaction.colorFromSpectrum = color.max;
            newFaction.GenerateNewLeader();

            FieldInfo relations = typeof(Faction).GetField("relations", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            List<FactionRelation> kkk = relations.GetValue(newFaction) as List<FactionRelation>;
            foreach (var s in kkk)
                Log.Message($"{s.other.Name}");

            Find.FactionManager.Add(newFaction);

        }

        private Faction GenerateFaction()
        {
            Faction faction = new Faction();
            FactionDef facDef = selectedFaction == null ? DefDatabase<FactionDef>.GetRandom() : selectedFaction;
            faction.def = facDef;
            faction.colorFromSpectrum = FactionGenerator.NewRandomColorFromSpectrum(faction);
            color.max = faction.colorFromSpectrum;

            foreach (Faction item in Find.FactionManager.AllFactionsListForReading)
            {
                faction.TryMakeInitialRelationsWith(item);
            }

            if (!facDef.isPlayer)
            {
                if (facDef.fixedName != null)
                {
                    faction.Name = facDef.fixedName;
                }
                else
                {
                    GenerateName(faction);
                }
            }
            faction.centralMelanin = Rand.Value;
            faction.GenerateNewLeader();
            FieldInfo relations = typeof(Faction).GetField("relations", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            newFactionRelation = relations.GetValue(faction) as List<FactionRelation>;

            return faction;
        }

        public void UpdateFactionInfo()
        {
            if (newFaction == null)
                return;

            FieldInfo relations = typeof(Faction).GetField("relations", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            newFactionRelation = relations.GetValue(newFaction) as List<FactionRelation>;
        }

        public void GenerateName(Faction faction)
        {
            if (newFaction == null)
                return;

            if (faction.def.factionNameMaker == null)
            {
                Faction f = (from fac in Find.FactionManager.AllFactionsVisible where fac.def.factionNameMaker != null select fac).RandomElement();
            }

            faction.Name = NameGenerator.GenerateName(faction.def.factionNameMaker, from fac in Find.FactionManager.AllFactionsVisible
                                                                               select fac.Name);
        }

        public void Show()
        {
            if (Find.WindowStack.IsOpen(typeof(FactionCreator)))
            {
                Log.Message("Currntly open...");
            }
            else
            {
                Find.WindowStack.Add(this);
            }
        }
    }
}
