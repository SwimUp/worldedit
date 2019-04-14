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

namespace WorldEdit.Editor
{
    internal class FactionEditor : EditWindow, IFWindow
    {
        public override Vector2 InitialSize => new Vector2(500, 600);

        private SettlementManager settlementManager;
        private FactionManager factionManager;

        internal FactionDef selectedFaction;
        internal Faction selectedGlobalFaction;

        private Vector2 scrollPositionFact = Vector2.zero;
        private Vector2 scrollGlobalPositionFact = Vector2.zero;

        private List<FactionDef> avaliableFactions;
        private List<Faction> avaliableGlobalFactions;

        private string customFactionName = string.Empty;
        private string customHistory = string.Empty;

        private bool useCustomColor = false;
        private FloatRange newColorR;
        private FloatRange newColorG;
        private FloatRange newColorB;
        public FactionEditor()
        {
            resizeable = false;

            avaliableFactions = new List<FactionDef>(DefDatabase<FactionDef>.DefCount);
            foreach (FactionDef faction in DefDatabase<FactionDef>.AllDefs)
            {
                avaliableFactions.Add(faction);
            }

            avaliableGlobalFactions = Find.FactionManager.AllFactions.ToList();

            settlementManager = new SettlementManager();
            factionManager = new FactionManager();
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

        public override void DoWindowContents(Rect inRect)
        {
            Text.Font = GameFont.Small;

            WidgetRow mainRow = new WidgetRow(0, 0);
            if(mainRow.ButtonText(Translator.Translate("FactionManager")))
            {
                factionManager.Show();
            }
            if (mainRow.ButtonText(Translator.Translate("SettlementManager")))
            {
                settlementManager.Show();
            }

            /*
            float factionPosition = inRect.height - 300;
            int yButtonPos = 0;
            int factionDefSize = avaliableFactions.Count * 25;
            Rect scrollRectFact = new Rect(0, 20, 190, factionPosition);
            Rect scrollVertRectFact = new Rect(0, 0, scrollRectFact.x, factionDefSize);
            Widgets.Label(new Rect(0, 0, 200, 20), Translator.Translate("AllFactionsList"));
            Widgets.BeginScrollView(scrollRectFact, ref scrollPositionFact, scrollVertRectFact);

            yButtonPos = 5;
            if (Widgets.ButtonText(new Rect(0, yButtonPos, 190, 20), Translator.Translate("NoText")))
            {
                selectedFaction = null;
            }
            yButtonPos += 25;
            foreach (FactionDef def in avaliableFactions)
            {
                if (Widgets.ButtonText(new Rect(0, yButtonPos, 190, 20), def.label))
                {
                    selectedFaction = def;
                    selectedGlobalFaction = null;
                }
                yButtonPos += 22;
            }

            Widgets.EndScrollView();

            Widgets.Label(new Rect(210, 0, 200, 20), Translator.Translate("AllSpawnedFactionsList"));
            int factionGlobalDefSize = avaliableGlobalFactions.Count * 25;
            Rect scrollRectGlobalFact = new Rect(210, 20, 210, factionPosition);
            Rect scrollVertRectGlobalFact = new Rect(0, 0, scrollRectGlobalFact.x, factionGlobalDefSize);
            Widgets.BeginScrollView(scrollRectGlobalFact, ref scrollGlobalPositionFact, scrollVertRectGlobalFact);

            yButtonPos = 5;
            if (Widgets.ButtonText(new Rect(0, yButtonPos, 190, 20), Translator.Translate("NoText")))
            {
                selectedGlobalFaction = null;
            }
            yButtonPos += 25;
            foreach (Faction fact in avaliableGlobalFactions)
            {
                if (Widgets.ButtonText(new Rect(0, yButtonPos, 190, 20), fact.Name))
                {
                    selectedGlobalFaction = fact;
                    selectedFaction = null;
                }
                yButtonPos += 22;
            }

            Widgets.EndScrollView();

            float startPos = factionPosition + 30;
            float nameFieldSize = (500 - 150);
            Widgets.Label(new Rect(0, startPos, 100, 30), Translator.Translate("CustomFactionName"));
            customFactionName = Widgets.TextField(new Rect(110, startPos, nameFieldSize, 22), customFactionName);

            /*
            startPos += 40;
            Widgets.Label(new Rect(0, startPos, 100, 30), Translator.Translate("CustomFactionLore"));
            customHistory = Widgets.TextField(new Rect(110, startPos, nameFieldSize, 22), customHistory);
            
            startPos += 40;
            if (Widgets.RadioButtonLabeled(new Rect(0, startPos, 140, 30), Translator.Translate("UseCustomColor"), useCustomColor == true))
            {
                useCustomColor = !useCustomColor;
            }
            Widgets.FloatRange(new Rect(150, startPos, 100, 30), 1222, ref newColorR, 0, 1, Translator.Translate("RedLabel"));
            Widgets.FloatRange(new Rect(250, startPos, 100, 30), 1221, ref newColorG, 0, 1, Translator.Translate("GreenLabel"));
            Widgets.FloatRange(new Rect(350, startPos, 100, 30), 1234, ref newColorB, 0, 1, Translator.Translate("BlueLabel"));

            if (Widgets.ButtonText(new Rect(100, 550, 190, 20), Translator.Translate("SetFraction")))
            {
                PrepareFaction();
            }
            */
        }

        private void PrepareFaction()
        {
            if (selectedFaction == null && selectedGlobalFaction == null)
                return;

            SetFaction();
        }

        private void SetFaction()
        {
            if (Find.WorldSelector.selectedTile < 0)
                return;

            bool generateNew = selectedFaction == null ? false : true;

            Faction faction = generateNew == false ? selectedGlobalFaction : FactionGenerator.NewGeneratedFaction(selectedFaction);
            if(generateNew == true)
                Find.World.factionManager.Add(faction);

            Settlement settlement = (Settlement)WorldObjectMaker.MakeWorldObject(WorldObjectDefOf.Settlement);
            settlement.SetFaction(faction);
            settlement.Tile = Find.WorldSelector.selectedTile;
            if (customFactionName.NullOrEmpty())
                settlement.Name = SettlementNameGenerator.GenerateSettlementName(settlement);
            else
                settlement.Name = customFactionName;

            Find.WorldObjects.Add(settlement);

            if (useCustomColor)
            {
                Log.Message($"R: {newColorR.max} : G: {newColorG.max} : B: {newColorB.max}");

                FieldInfo material = typeof(Settlement).GetField("cachedMat", BindingFlags.NonPublic | BindingFlags.Instance);
                if (material != null)
                {
                    Material mat = MaterialPool.MatFrom(faction.def.homeIconPath, ShaderDatabase.WorldOverlayTransparentLit, new Color(newColorR.max, newColorG.max, newColorB.max), WorldMaterials.WorldObjectRenderQueue);
                    material.SetValue(settlement, mat);
                }
            }
        }
    }
}
