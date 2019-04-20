using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using Verse;
using WorldEdit.Interfaces;

namespace WorldEdit.Editor
{
    internal sealed class FactionEditor : FWindow
    {
        public override Vector2 InitialSize => new Vector2(410, 530);
        private Vector2 scrollFieldsPos = Vector2.zero;
        private Vector2 scrollRel = Vector2.zero;

        /// <summary>
        /// Выбранная фракция
        /// </summary>
        private Faction selectedFaction = null;

        /// <summary>
        /// Отношений с фракциями
        /// </summary>
        private List<FactionRelation> factionRelation;
        private string[] factionGoodwillBuff = null;

        /// <summary>
        /// Цвет фракции
        /// </summary>
        private FloatRange color;

        /// <summary>
        /// Имя лидера
        /// </summary>
        private string leaderName;

        public FactionEditor()
        {
            resizeable = false;
        }

        public override void DoWindowContents(Rect inRect)
        {
            if (selectedFaction == null)
                return;

            Text.Font = GameFont.Small;

            Widgets.Label(new Rect(160, 0, 300, 20), Translator.Translate("FactionEditTitle"));

            Rect scrollRectGlobalFact = new Rect(0, 30, 390, inRect.height - 30);
            Rect scrollVertRectGlobalFact = new Rect(0, 0, scrollRectGlobalFact.x, 600);
            Widgets.BeginScrollView(scrollRectGlobalFact, ref scrollFieldsPos, scrollVertRectGlobalFact);
            if (selectedFaction != null)
            {
                Widgets.Label(new Rect(0, 5, 330, 30), $"{Translator.Translate("FactionDefName")} {selectedFaction.def.label}");

                Widgets.Label(new Rect(0, 35, 150, 30), Translator.Translate("FactionName"));
                selectedFaction.Name = Widgets.TextField(new Rect(160, 35, 180, 30), selectedFaction.Name);

                Widgets.Label(new Rect(0, 73, 150, 30), Translator.Translate("FactionDefeated"));
                if (selectedFaction.defeated)
                {
                    if (Widgets.ButtonText(new Rect(160, 73, 180, 30), Translator.Translate("isDefeatedYES")))
                    {
                        selectedFaction.defeated = false;
                    }
                }
                else
                {
                    if (Widgets.ButtonText(new Rect(160, 73, 180, 30), Translator.Translate("isDefeatedNO")))
                    {
                        selectedFaction.defeated = true;
                    }
                }

                Widgets.Label(new Rect(0, 105, 180, 30), Translator.Translate("FactionRelative"));
                int y = 15;
                int boxY = 5;
                Rect scrollRectRel = new Rect(0, 130, 370, 160);
                Rect scrollVertRectRel = new Rect(0, 0, scrollRectRel.x, factionRelation.Count * 140);
                Widgets.DrawBox(new Rect(0, 130, 350, 160));
                Widgets.BeginScrollView(scrollRectRel, ref scrollRel, scrollVertRectRel);
                for (int i = 0; i < factionRelation.Count; i++)
                {
                    FactionRelation rel = factionRelation[i];

                    Widgets.DrawBox(new Rect(2, boxY, 340, 130));

                    Widgets.Label(new Rect(5, y, 315, 30), $"{Translator.Translate("FactionInfoName")} {rel.other.Name}");

                    y += 35;
                    Widgets.Label(new Rect(5, y, 140, 30), Translator.Translate("FactionGoodness"));
                    Widgets.TextFieldNumeric(new Rect(150, y, 130, 30), ref rel.goodwill, ref factionGoodwillBuff[i], -10000000000f);

                    y += 35;
                    switch (rel.kind)
                    {
                        case FactionRelationKind.Ally:
                            {
                                if (Widgets.ButtonText(new Rect(5, y, 180, 30), rel.kind.GetLabel()))
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
                float.TryParse(Widgets.TextField(new Rect(195, 315, 160, 30), selectedFaction.centralMelanin.ToString()), out selectedFaction.centralMelanin);

                Widgets.Label(new Rect(0, 360, 160, 30), Translator.Translate("ColorSpectrum"));
                Widgets.FloatRange(new Rect(195, 360, 130, 30), 42, ref color, 0, 1);
                if (selectedFaction.def.colorSpectrum != null)
                    Widgets.DrawBoxSolid(new Rect(165, 360, 20, 20), ColorsFromSpectrum.Get(selectedFaction.def.colorSpectrum, color.max));

                if (selectedFaction.leader != null)
                {
                    Widgets.Label(new Rect(0, 400, 120, 30), Translator.Translate("FactionLeaderName"));
                    leaderName = Widgets.TextField(new Rect(135, 400, 220, 30), leaderName);
                }

            }
            Widgets.EndScrollView();

            if (Widgets.ButtonText(new Rect(0, inRect.height - 20, 400, 20), Translator.Translate("SaveFaction")))
            {
                SaveFaction();
            }
        }

        private void InitFaction(Faction faction)
        {
            selectedFaction = faction;

            FieldInfo relations = typeof(Faction).GetField("relations", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            factionRelation = relations.GetValue(selectedFaction) as List<FactionRelation>;

            factionGoodwillBuff = new string[factionRelation.Count];

            for (int i = 0; i < factionRelation.Count; i++)
                factionGoodwillBuff[i] = factionRelation[i].goodwill.ToString();

            if(selectedFaction.leader != null)
                leaderName = selectedFaction.leader.Name.ToStringFull;

        }

        private void SaveFaction()
        {
            Close();
        }

        public void Show(Faction faction)
        {
            if (Find.WindowStack.IsOpen(typeof(FactionEditor)))
            {
                Log.Message("Currntly open...");
            }
            else
            {
                InitFaction(faction);
                Find.WindowStack.Add(this);
            }
        }
    }
}
