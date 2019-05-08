using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using WorldEdit.Interfaces;

namespace WorldEdit.Editor.DefsEditor
{
    public class RoadDefEditor : FWindow
    {
        public override Vector2 InitialSize => new Vector2(300, 300);

        private RoadDef templateDef;
        private RoadDef newRoad;

        public RoadDefEditor()
        {
            resizeable = false;
            newRoad = new RoadDef();
        }

        public override void DoWindowContents(Rect inRect)
        {
            Widgets.Label(new Rect(0, 0, 200, 20), Translator.Translate("RoadDefEditorTitle"));

            Widgets.Label(new Rect(0, 30, 150, 20), Translator.Translate("TemplateRoadDef"));
            if (Widgets.ButtonText(new Rect(155, 30, 135, 20), templateDef?.LabelCap))
            {
                List<FloatMenuOption> list = new List<FloatMenuOption>();
                foreach (var road in DefDatabase<RoadDef>.AllDefsListForReading)
                    list.Add(new FloatMenuOption(road.LabelCap, delegate
                    {
                        SetTemplate(road);
                    }));
                Find.WindowStack.Add(new FloatMenu(list));
            }

            int y = 60;
            Widgets.Label(new Rect(0, y, 80, 20), Translator.Translate("DefName"));
            newRoad.defName = Widgets.TextField(new Rect(90, y, 200, 20), newRoad.defName);
            TooltipHandler.TipRegion(new Rect(0, y, 300, 20), Translator.Translate("DefNameHelp"));

            y += 25;
            Widgets.Label(new Rect(0, y, 80, 20), Translator.Translate("labelDef"));
            newRoad.label = Widgets.TextField(new Rect(90, y, 200, 20), newRoad.label);
            TooltipHandler.TipRegion(new Rect(0, y, 300, 20), Translator.Translate("labelDefHelp"));

            y += 25;
            Widgets.Label(new Rect(0, y, 80, 20), Translator.Translate("priorityDef"));
            int.TryParse(Widgets.TextField(new Rect(90, y, 200, 20), newRoad.priority.ToString()), out newRoad.priority);
            TooltipHandler.TipRegion(new Rect(0, y, 300, 20), Translator.Translate("priorityDefHelp"));

            y += 25;
            Widgets.Label(new Rect(0, y, 80, 20), Translator.Translate("movementCostMultiplierDef"));
            float.TryParse(Widgets.TextField(new Rect(90, y, 200, 20), newRoad.movementCostMultiplier.ToString()), out newRoad.movementCostMultiplier);
            TooltipHandler.TipRegion(new Rect(0, y, 300, 20), Translator.Translate("movementCostMultiplierDefHelp"));

            y += 25;
            Widgets.Label(new Rect(0, y, 80, 20), Translator.Translate("tilesPerSegmentDef"));
            int.TryParse(Widgets.TextField(new Rect(90, y, 200, 20), newRoad.tilesPerSegment.ToString()), out newRoad.tilesPerSegment);
            TooltipHandler.TipRegion(new Rect(0, y, 300, 20), Translator.Translate("tilesPerSegmentDefHelp"));

            Widgets.Label(new Rect(0, y, 80, 20), Translator.Translate("pathingModeDef"));
            if (Widgets.ButtonText(new Rect(90, y, 135, 20), newRoad.pathingMode.LabelCap))
            {
                List<FloatMenuOption> list = new List<FloatMenuOption>();
                foreach (var road in DefDatabase<RoadPathingDef>.AllDefsListForReading)
                    list.Add(new FloatMenuOption(road.LabelCap, delegate
                    {
                        newRoad.pathingMode = road;
                    }));
                Find.WindowStack.Add(new FloatMenu(list));
            }
            TooltipHandler.TipRegion(new Rect(0, y, 300, 20), Translator.Translate("pathingModeDefHelp"));
        }

        private void SetTemplate(RoadDef def)
        {
            templateDef = def;
        }
    }
}
