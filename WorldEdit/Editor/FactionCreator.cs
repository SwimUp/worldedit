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
        public override Vector2 InitialSize => new Vector2(730, 450);

        private FactionDef selectedFaction = null;
        private Faction newFaction = null;
        private List<FactionRelation> selectedRelative = new List<FactionRelation>();

        private Vector2 scrollPositionFact = Vector2.zero;
        private Vector2 scrollFieldsPos = Vector2.zero;
        private Vector2 scrollRel = Vector2.zero;

        private List<FactionDef> avaliableFactions;
        private List<FactionRelation> newFactionRelation;
        private FloatRange color;

        public FactionCreator()
        {
            resizeable = false;

            avaliableFactions = DefDatabase<FactionDef>.AllDefs.ToList();
        }

        public override void DoWindowContents(Rect inRect)
        {
            Text.Font = GameFont.Small;

            Widgets.Label(new Rect(inRect.center.x, 0, 250, 20), Translator.Translate("FactionCreatorTitle"));
            WidgetRow row = new WidgetRow(0, 30);
            if (row.ButtonText(Translator.Translate("RandomizeFaction")))
            {
                newFaction = GenerateFaction();
            }
            if (row.ButtonText(Translator.Translate("ClearFaction")))
            {
                newFaction = null;
                selectedFaction = null;
            }

            int factionDefSize = avaliableFactions.Count * 25;
            Rect scrollRectFact = new Rect(0, 60, 320, inRect.height - 100);
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
                }
                yButtonPos += 22;
            }
            Widgets.EndScrollView();

            Rect scrollRectGlobalFact = new Rect(330, 50, 370, inRect.height - 20);
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
                Widgets.DrawBox(new Rect(0, 90, 370, 160));
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
                    int.TryParse(Widgets.TextField(new Rect(150, y, 130, 30), rel.goodwill.ToString()), out rel.goodwill);

                    y += 35;
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
                
                if(Widgets.ButtonText(new Rect(0, 260, 180, 30), Translator.Translate("AddNewRelative")))
                {

                }
                if (Widgets.ButtonText(new Rect(190, 260, 150, 30), Translator.Translate("DeleteRelative")))
                {
                    RemoveRelatives();
                }

                Widgets.Label(new Rect(0, 310, 180, 30), Translator.Translate("FactionMelanin"));
                float.TryParse(Widgets.TextField(new Rect(195, 310, 290, 30), newFaction.centralMelanin.ToString()), out newFaction.centralMelanin);

                Widgets.Label(new Rect(0, 350, 160, 30), Translator.Translate("ColorSpectrum"));
                Widgets.FloatRange(new Rect(195, 350, 130, 30), 42, ref color, 0, 1);
                Widgets.DrawBoxSolid(new Rect(165, 350, 20, 20), ColorsFromSpectrum.Get(newFaction.def.colorSpectrum, color.max));
            }
            Widgets.EndScrollView();

            if (Widgets.ButtonText(new Rect(0, inRect.height - 20, 320, 20), Translator.Translate("CreateNewFaction")))
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
            newFaction.colorFromSpectrum = color.max;

            Find.FactionManager.Add(newFaction);

        }

        private Faction GenerateFaction()
        {
            Faction faction = new Faction();
            FactionDef facDef = selectedFaction == null ? DefDatabase<FactionDef>.GetRandom() : selectedFaction;

            faction.def = facDef;
            faction.colorFromSpectrum = FactionGenerator.NewRandomColorFromSpectrum(faction);
            color.max = faction.colorFromSpectrum;
            if (!facDef.isPlayer)
            {
                if (facDef.fixedName != null)
                {
                    faction.Name = facDef.fixedName;
                }
                else
                {
                    faction.Name = NameGenerator.GenerateName(facDef.factionNameMaker, from fac in Find.FactionManager.AllFactionsVisible
                                                                                       select fac.Name);
                }
            }
            faction.centralMelanin = Rand.Value;
            
            foreach (Faction item in Find.FactionManager.AllFactionsListForReading)
            {
                faction.TryMakeInitialRelationsWith(item);
            }

            faction.GenerateNewLeader();

            FieldInfo relations = typeof(Faction).GetField("relations", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            newFactionRelation = relations.GetValue(faction) as List<FactionRelation>;

            return faction;
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
