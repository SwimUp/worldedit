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
    internal class FactionEditor : EditWindow, IFWindow
    {
        public override Vector2 InitialSize => new Vector2(500, 500);

        private Faction selectedFaction = null;
        private Vector2 scrollFieldsPos = Vector2.zero;
        private Vector2 scrollRel = Vector2.zero;

        private List<FactionRelation> factionRelation;
        private string[] factionGoodwillBuff;

        private FloatRange color;
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

            Widgets.Label(new Rect(180, 0, 300, 20), Translator.Translate("FactionEditTitle"));

            Rect scrollRectGlobalFact = new Rect(10, 40, 490, inRect.height - 60);
            Rect scrollVertRectGlobalFact = new Rect(0, 0, scrollRectGlobalFact.x, 600);
            Widgets.BeginScrollView(scrollRectGlobalFact, ref scrollFieldsPos, scrollVertRectGlobalFact);
            Widgets.Label(new Rect(0, 5, 330, 30), $"{Translator.Translate("FactionDefName")} {selectedFaction.def.label}");

            Widgets.Label(new Rect(0, 35, 150, 30), Translator.Translate("FactionName"));
            selectedFaction.Name = Widgets.TextField(new Rect(160, 5, 180, 30), selectedFaction.Name);

            Widgets.Label(new Rect(0, 65, 150, 30), Translator.Translate("FactionDefeated"));
            if (selectedFaction.defeated)
            {
                if (Widgets.ButtonText(new Rect(160, 65, 180, 30), Translator.Translate("isDefeatedYES")))
                {
                    selectedFaction.defeated = false;
                }
            }
            else
            {
                if (Widgets.ButtonText(new Rect(160, 65, 180, 30), Translator.Translate("isDefeatedNO")))
                {
                    selectedFaction.defeated = true;
                }
            }

            Widgets.Label(new Rect(0, 105, 180, 30), Translator.Translate("FactionRelative"));
            int y = 15;
            int boxY = 5;
            Rect scrollRectRel = new Rect(0, 130, 370, 160);
            Rect scrollVertRectRel = new Rect(0, 0, scrollRectRel.x, factionRelation.Count * 145);
            Widgets.DrawBox(new Rect(0, 130, 350, 160));
            Widgets.BeginScrollView(scrollRectRel, ref scrollRel, scrollVertRectRel);
            for (int i = 0; i < factionRelation.Count; i++)
            //foreach (var rel in newFactionRelation)
            {
                FactionRelation rel = factionRelation[i];

                Widgets.DrawBox(new Rect(2, boxY, 340, 130));

                Widgets.Label(new Rect(5, y, 315, 30), $"{Translator.Translate("FactionInfoName")} {rel.other.Name}");

                y += 35;
                Widgets.Label(new Rect(5, y, 140, 30), Translator.Translate("FactionGoodness"));
                Widgets.TextFieldNumeric(new Rect(150, y, 130, 30), ref rel.goodwill, ref factionGoodwillBuff[i]);

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
                Widgets.EndScrollView();

                Widgets.Label(new Rect(0, 315, 180, 30), Translator.Translate("FactionMelanin"));
                float.TryParse(Widgets.TextField(new Rect(195, 315, 160, 30), selectedFaction.centralMelanin.ToString()), out selectedFaction.centralMelanin);

                Widgets.Label(new Rect(0, 385, 160, 30), Translator.Translate("ColorSpectrum"));
                Widgets.FloatRange(new Rect(195, 385, 130, 30), 42, ref color, 0, 1);
                if (selectedFaction.def.colorSpectrum != null)
                    Widgets.DrawBoxSolid(new Rect(165, 385, 20, 20), ColorsFromSpectrum.Get(selectedFaction.def.colorSpectrum, color.max));

                Widgets.Label(new Rect(0, 415, 180, 30), Translator.Translate("FactionLeaderName"));
                leaderName = Widgets.TextField(new Rect(195, 415, 160, 30), leaderName);

            }

            Widgets.EndScrollView();

            if (Widgets.ButtonText(new Rect(0, inRect.height - 40, 320, 20), Translator.Translate("SaveFaction")))
            {
                SaveFaction();
            }
        }

        private void InitFaction(Faction faction)
        {
            selectedFaction = faction;

            FieldInfo relations = typeof(Faction).GetField("relations", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            factionRelation = relations.GetValue(selectedFaction) as List<FactionRelation>;
            for (int i = 0; i < factionRelation.Count; i++)
                factionGoodwillBuff[i] = factionRelation[i].goodwill.ToString();
        }

        private void SaveFaction()
        {
            Close();
        }

        public void Show()
        {
            if (Find.WindowStack.IsOpen(typeof(FactionEditor)))
            {
                Log.Message("Currntly open...");
            }
            else
            {
                Find.WindowStack.Add(this);
            }
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
