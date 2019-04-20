using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Verse;
using WorldEdit.Interfaces;

namespace WorldEdit.Editor
{
    internal sealed class FactionCreator : FWindow
    {
        public override Vector2 InitialSize => new Vector2(740, 450);

        /// <summary>
        /// Выбранная фракция
        /// </summary>
        private FactionDef selectedFaction = null;

        /// <summary>
        /// Новая фракция
        /// </summary>
        public Faction newFaction = null;

        private Vector2 scrollPositionFact = Vector2.zero;
        private Vector2 scrollFieldsPos = Vector2.zero;
        private Vector2 scrollRel = Vector2.zero;

        /// <summary>
        /// Список доступных фракций
        /// </summary>
        private List<FactionDef> avaliableFactions;

        /// <summary>
        /// Отношений с фракциями
        /// </summary>
        private List<FactionRelation> newFactionRelation;

        /// <summary>
        /// Буфер для отношений(значений)
        /// </summary>
        private string[] newFactionGoodwillBuff;

        /// <summary>
        /// Цвет фракции
        /// </summary>
        private FloatRange color = new FloatRange();

        /// <summary>
        /// Имя лидера
        /// </summary>
        private string leaderName = string.Empty;

        public FactionCreator()
        {
            resizeable = false;

            var rimfactionManager = Find.FactionManager;

            avaliableFactions = DefDatabase<FactionDef>.AllDefs.Where(f => !f.isPlayer &&
            rimfactionManager.OfMechanoids.def != f && rimfactionManager.OfInsects.def != f &&
            rimfactionManager.OfAncientsHostile.def != f && rimfactionManager.OfAncients.def != f).ToList();
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

                    if(newFaction == null)
                        newFaction = GenerateFaction();
                }
                yButtonPos += 22;
            }
            Widgets.EndScrollView();

            Rect scrollRectGlobalFact = new Rect(330, 50, 380, inRect.height - 20);
            Rect scrollVertRectGlobalFact = new Rect(0, 0, scrollRectGlobalFact.x, 600);
            Widgets.BeginScrollView(scrollRectGlobalFact, ref scrollFieldsPos, scrollVertRectGlobalFact);
            if (newFaction != null)
            {
                Widgets.Label(new Rect(0, 5, 330, 30), $"{Translator.Translate("FactionDefName")} {selectedFaction.label}");

                Widgets.Label(new Rect(0, 35, 150, 30), Translator.Translate("FactionName"));
                newFaction.Name = Widgets.TextField(new Rect(160, 35, 180, 30), newFaction.Name);

                Widgets.Label(new Rect(0, 73, 150, 30), Translator.Translate("FactionDefeated"));
                if (newFaction.defeated)
                {
                    if (Widgets.ButtonText(new Rect(160, 73, 180, 30), Translator.Translate("isDefeatedYES")))
                    {
                        newFaction.defeated = false;
                    }
                }
                else
                {
                    if (Widgets.ButtonText(new Rect(160, 73, 180, 30), Translator.Translate("isDefeatedNO")))
                    {
                        newFaction.defeated = true;
                    }
                }

                Widgets.Label(new Rect(0, 105, 180, 30), Translator.Translate("FactionRelative"));
                int y = 15;
                int boxY = 5;
                Rect scrollRectRel = new Rect(0, 130, 370, 160);
                Rect scrollVertRectRel = new Rect(0, 0, scrollRectRel.x, newFactionRelation.Count * 140);
                Widgets.DrawBox(new Rect(0, 130, 350, 160));
                Widgets.BeginScrollView(scrollRectRel, ref scrollRel, scrollVertRectRel);
                for(int i = 0; i < newFactionRelation.Count; i++)
                {
                    FactionRelation rel = newFactionRelation[i];

                    Widgets.DrawBox(new Rect(2, boxY, 340, 130));

                    Widgets.Label(new Rect(5, y, 315, 30), $"{Translator.Translate("FactionInfoName")} {rel.other.Name}");

                    y += 35;
                    Widgets.Label(new Rect(5, y, 140, 30), Translator.Translate("FactionGoodness"));
                    Widgets.TextFieldNumeric(new Rect(150, y, 130, 30), ref rel.goodwill, ref newFactionGoodwillBuff[i], -10000000000f);

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
                
                Widgets.Label(new Rect(0, 315, 180, 30), Translator.Translate("FactionMelanin"));
                float.TryParse(Widgets.TextField(new Rect(195, 315, 160, 30), newFaction.centralMelanin.ToString()), out newFaction.centralMelanin);

                Widgets.Label(new Rect(0, 360, 160, 30), Translator.Translate("ColorSpectrum"));
                Widgets.FloatRange(new Rect(195, 360, 130, 30), 42, ref color, 0, 1);
                if(newFaction.def.colorSpectrum != null)
                    Widgets.DrawBoxSolid(new Rect(165, 360, 20, 20), ColorsFromSpectrum.Get(newFaction.def.colorSpectrum, color.max));

                Widgets.Label(new Rect(0, 400, 120, 30), Translator.Translate("FactionLeaderName"));
                leaderName = Widgets.TextField(new Rect(135, 400, 220, 30), leaderName);

            }
            Widgets.EndScrollView();

            if (Widgets.ButtonText(new Rect(0, inRect.height - 30, 320, 20), Translator.Translate("CreateNewFaction")))
            {
                CreateFaction();
            }
        }

        private void CreateFaction()
        {
            if (newFaction == null)
                return;

            newFaction.colorFromSpectrum = color.max;

            if(leaderName != null)
                newFaction.leader.Name = new NameSingle(leaderName, true);

            newFaction.loadID = Find.UniqueIDsManager.GetNextFactionID();

            Find.FactionManager.Add(newFaction);

            newFaction = null;
        }

        private Faction GenerateFaction()
        {
            if(newFaction != null)
            {
                Find.WorldPawns.RemoveAndDiscardPawnViaGC(newFaction.leader);
            }

            Faction faction = new Faction();
            FactionDef facDef = null;
            if(selectedFaction == null)
            {
                selectedFaction = avaliableFactions.RandomElement();
            }
            facDef = selectedFaction;
            faction.def = facDef;
            faction.colorFromSpectrum = FactionGenerator.NewRandomColorFromSpectrum(faction);
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
            foreach (Faction item in Find.FactionManager.AllFactions)
            {
                faction.TryMakeInitialRelationsWith(item);
            }
            faction.GenerateNewLeader();

            leaderName = faction.leader.Name.ToStringFull;

            FieldInfo relations = typeof(Faction).GetField("relations", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            newFactionRelation = relations.GetValue(faction) as List<FactionRelation>;
            newFactionGoodwillBuff = new string[newFactionRelation.Count];

            for (int i = 0; i < newFactionRelation.Count; i++)
                newFactionGoodwillBuff[i] = newFactionRelation[i].goodwill.ToString();

            return faction;
        }

        public void GenerateName(Faction faction)
        {
            if (newFaction == null)
                return;

            if (faction.def.factionNameMaker == null)
            {
                Faction f = (from fac in Find.FactionManager.AllFactionsVisible where fac.def.factionNameMaker != null select fac).RandomElement();
                faction = f;
            }

            faction.Name = NameGenerator.GenerateName(faction.def.factionNameMaker, from fac in Find.FactionManager.AllFactionsVisible
                                                                               select fac.Name);
        }

        public override void Close(bool doCloseSound = true)
        {
            base.Close(doCloseSound);

            newFaction = null;
            newFactionGoodwillBuff = null;
        }
    }
}
