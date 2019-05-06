using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.Noise;

namespace WorldEdit.WorldGen.Generators
{
    public class Generator_Rainfall_Vanilla : Generator
    {
        public override string Description => Translator.Translate($"{GetType().Name}");

        public override string Title => Translator.Translate($"{GetType().Name}_title");

        [Unsaved]
        private ModuleBase noiseRainfall;

        public float FreqMultiplier => 1f;

        public override GeneratorMode Mode => GeneratorMode.Rainfall;

        public override GeneratorType Type => GeneratorType.Vanilla;

        public SimpleCurve SimpleCurve = new SimpleCurve()
        {
            new CurvePoint(0f, 1.12f),
            new CurvePoint(25f, 0.94f),
            new CurvePoint(45f, 0.7f),
            new CurvePoint(70f, 0.3f),
            new CurvePoint(80f, 0.05f),
            new CurvePoint(90f, 0.05f)
        };
        
        public override void RunGenerator()
        {
            SetupRainfallNoise();

            for (int i = 0; i < Find.WorldGrid.TilesCount; i++)
                GenerateFor(i);
        }

        private void GenerateFor(int tileID)
        {
            Tile tile = Find.WorldGrid[tileID];
            Vector3 tileCenter = Find.WorldGrid.GetTileCenter(tileID);
            tile.rainfall = noiseRainfall.GetValue(tileCenter);
        }

        private void SetupRainfallNoise()
        {
            float freqMultiplier = FreqMultiplier;
            ModuleBase input = new Perlin(0.015f * freqMultiplier, 2.0, 0.5, 6, Rand.Range(0, int.MaxValue), QualityMode.High);
            input = new ScaleBias(0.5, 0.5, input);
            ModuleBase moduleBase = new AbsLatitudeCurve(SimpleCurve, 100f);
            noiseRainfall = new Multiply(input, moduleBase);
            Func<double, double> processor = delegate (double val)
            {
                if (val < 0.0)
                {
                    val = 0.0;
                }
                if (val < 0.12)
                {
                    val = (val + 0.12) / 2.0;
                    if (val < 0.03)
                    {
                        val = (val + 0.03) / 2.0;
                    }
                }
                return val;
            };
            noiseRainfall = new Arbitrary(noiseRainfall, processor);
            noiseRainfall = new Power(noiseRainfall, new Const(1.5));
            noiseRainfall = new Clamp(0.0, 999.0, noiseRainfall);
            noiseRainfall = new ScaleBias(4000.0, 0.0, noiseRainfall);
            SimpleCurve rainfallCurve = Find.World.info.overallRainfall.GetRainfallCurve();
            if (rainfallCurve != null)
            {
                noiseRainfall = new CurveSimple(noiseRainfall, rainfallCurve);
            }
        }
    }
}
