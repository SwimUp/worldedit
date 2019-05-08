using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using Verse;
using WorldEdit.Interfaces;
using WorldEdit.WorldGen;
using WorldEdit.WorldGen.Generators;

namespace WorldEdit.Editor
{
    internal class WorldGenStepsMenu : FWindow
    {
        public override Vector2 InitialSize => new Vector2(630, 500);

        public Vector2 scroll = Vector2.zero;
        public Vector2 scroll2 = Vector2.zero;

        private GeneratorMode mode;
        private Generator generator = null;

        public WorldGenStepsMenu()
        {
            resizeable = false;

            mode = TerrainManager.Generators.Keys.RandomElement();
            generator = TerrainManager.Generators[mode].RandomElement();
        }

        public override void DoWindowContents(Rect inRect)
        {
            Text.Font = GameFont.Small;

            Widgets.Label(new Rect(0, 0, 280, 20), Translator.Translate("WorldGenStepsTitle"));

            if(Widgets.ButtonText(new Rect(0, 30, 200, 20), Translator.Translate("UseGenerator")))
            {
                if (generator != null)
                    generator.RunGenerator();
            }

            Widgets.Label(new Rect(0, 60, 80, 20), Translator.Translate("GenMode"));
            if(Widgets.ButtonText(new Rect(90, 60, 150, 20), Translator.Translate($"{mode}_title")))
            {
                List<FloatMenuOption> list = new List<FloatMenuOption>();
                foreach (GeneratorMode mode in Enum.GetValues(typeof(GeneratorMode)))
                    list.Add(new FloatMenuOption(Translator.Translate($"{mode}_title"), delegate
                    {
                        this.mode = mode;
                        generator = TerrainManager.Generators[mode].FirstOrDefault();
                    }));
                Find.WindowStack.Add(new FloatMenu(list));
            }
            Widgets.Label(new Rect(265, 60, 80, 20), Translator.Translate("GenType"));
            if (Widgets.ButtonText(new Rect(310, 60, 280, 20), generator?.Title))
            {
                List<FloatMenuOption> list = new List<FloatMenuOption>();

                if (generator == null)
                {
                    list.Add(new FloatMenuOption("No generators", delegate
                    {
                        
                    }));
                }
                else
                {
                    foreach (var generator in TerrainManager.Generators[mode])
                        list.Add(new FloatMenuOption(generator.Title, delegate
                        {
                            this.generator = generator;
                        }));
                }
                Find.WindowStack.Add(new FloatMenu(list));
            }
            Widgets.LabelScrollable(new Rect(0, 90, 320, 300), generator?.Description, ref scroll);

            int paramSize = generator.Settings.Parameters.Count * 60;
            int x = 0;
            Rect scrollRectRel = new Rect(310, 90, 290, 300);
            Rect scrollVertRectRel = new Rect(0, 0, scrollRectRel.x, paramSize);
            Widgets.BeginScrollView(scrollRectRel, ref scroll2, scrollVertRectRel);
            foreach (var param in generator.Settings.Parameters)
            {
                Widgets.Label(new Rect(0, x, 110, 20), param.Name);
                TooltipHandler.TipRegion(new Rect(0, x, 270, 20), param.Description);
                if(param.Param.FieldType == typeof(bool))
                {
                    DrawBoolParam(param, ref x);
                }
                else if(param.Param.FieldType == typeof(FloatRange))
                {
                    DrawFloatParam(param, ref x);
                }
                else if (param.Param.FieldType == typeof(IntRange))
                {
                    DrawIntParam(param, ref x);
                }
                else if(param.Param.FieldType == typeof(SimpleCurve))
                {
                    DrawCurveParam(param, ref x);
                }
                else
                {
                    DrawNormalParam(param, ref x);
                }
                x += 25;
            }
            Widgets.EndScrollView();
        }

        private void DrawFloatParam(Parameter param, ref int x)
        {
            FloatRange range = (FloatRange)param.value;
            float.TryParse(Widgets.TextField(new Rect(120, x, 70, 20), range.min.ToString()), out range.min);
            float.TryParse(Widgets.TextField(new Rect(200, x, 70, 20), range.max.ToString()), out range.max);
            param.value = range;
        }

        private void DrawIntParam(Parameter param, ref int x)
        {
            IntRange range = (IntRange)param.value;
            int.TryParse(Widgets.TextField(new Rect(120, x, 70, 20), range.min.ToString()), out range.min);
            int.TryParse(Widgets.TextField(new Rect(200, x, 70, 20), range.max.ToString()), out range.max);
            param.value = range;
        }

        private void DrawNormalParam(Parameter param, ref int x)
        {
            param.value = Widgets.TextField(new Rect(120, x, 150, 20), param.value.ToString());
        }

        private void DrawCurveParam(Parameter param, ref int x)
        {
            SimpleCurve curv = (SimpleCurve)param.value;
            for(int i = 0; i < curv.Points.Count; i++)
            {
                CurvePoint p = curv.Points[i];

                if(float.TryParse(Widgets.TextField(new Rect(120, x, 70, 20), p.x.ToString()), out float xC)
                && float.TryParse(Widgets.TextField(new Rect(200, x, 70, 20), p.y.ToString()), out float yC))
                {
                    curv.Points[i] = new CurvePoint(xC, yC);
                }
                x += 25;
            }
        }

        private void DrawBoolParam(Parameter param, ref int x)
        {
            if (Widgets.RadioButtonLabeled(new Rect(120, x, 150, 20), "", bool.Parse(param.value.ToString())))
            {
                bool b = bool.Parse(param.value.ToString());
                b = !b;
                param.value = b;
            }
        }
    }
}
