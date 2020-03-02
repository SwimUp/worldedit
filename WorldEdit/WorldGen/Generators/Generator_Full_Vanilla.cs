using RimWorld;
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
    public class Generator_Full_Vanilla : Generator
    {
        public override string Description => Translator.Translate($"{GetType().Name}");

        public override string Title => Translator.Translate($"{GetType().Name}_title");

        [Unsaved]
        private ModuleBase noiseElevation;

        [Unsaved]
        private ModuleBase noiseTemperatureOffset;

        [Unsaved]
        private ModuleBase noiseRainfall;

        [Unsaved]
        private ModuleBase noiseSwampiness;

        [Unsaved]
        private ModuleBase noiseMountainLines;

        [Unsaved]
        private ModuleBase noiseHillsPatchesMicro;

        [Unsaved]
        private ModuleBase noiseHillsPatchesMacro;

        public FloatRange SwampinessMaxElevation = new FloatRange(650f, 750f);

        public FloatRange SwampinessMinRainfall = new FloatRange(725f, 900f);

        public FloatRange ElevationRange = new FloatRange(-500f, 5000f);

        public SimpleCurve AvgTempByLatitudeCurve = new SimpleCurve
        {
            new CurvePoint(0f, 30f),
            new CurvePoint(0.1f, 29f),
            new CurvePoint(0.5f, 7f),
            new CurvePoint(1f, -37f)
        };

        public SimpleCurve SimpleCurveRainfall = new SimpleCurve()
        {
            new CurvePoint(0f, 1.12f),
            new CurvePoint(25f, 0.94f),
            new CurvePoint(45f, 0.7f),
            new CurvePoint(70f, 0.3f),
            new CurvePoint(80f, 0.05f),
            new CurvePoint(90f, 0.05f)
        };

        public float FreqMultiplier = 1f;

        public override GeneratorMode Mode => GeneratorMode.Full;

        public override GeneratorType Type => GeneratorType.Vanilla;

        public Generator_Full_Vanilla()
        {
            Settings.AddParam(GetType().GetField("SwampinessMaxElevation"), SwampinessMaxElevation);
            Settings.AddParam(GetType().GetField("SwampinessMinRainfall"), SwampinessMinRainfall);
            Settings.AddParam(GetType().GetField("ElevationRange"), ElevationRange);
            Settings.AddParam(GetType().GetField("FreqMultiplier"), FreqMultiplier);
            Settings.AddParam(GetType().GetField("AvgTempByLatitudeCurve"), AvgTempByLatitudeCurve);
            Settings.AddParam(GetType().GetField("SimpleCurveRainfall"), SimpleCurveRainfall);
        }

        public override void RunGenerator()
        {
            Setup();

            LongEventHandler.QueueLongEvent(delegate
            {
                SetupElevationNoise();
                SetupTemperatureOffsetNoise();
                SetupRainfallNoise();
                SetupHillinessNoise();
                SetupSwampinessNoise();

                for (int i = 0; i < Find.WorldGrid.TilesCount; i++)
                {
                    GenerateTileFor(i);
                }

                TerrainManager.Generators[GeneratorMode.Features].Where(gen => gen.Type == GeneratorType.Vanilla).FirstOrDefault().RunGenerator();
                TerrainManager.Generators[GeneratorMode.Rivers].Where(gen => gen.Type == GeneratorType.Vanilla).FirstOrDefault().RunGenerator();
                TerrainManager.Generators[GeneratorMode.Roads].Where(gen => gen.Type == GeneratorType.Vanilla).FirstOrDefault().RunGenerator();

                WorldEditor.WorldUpdater.UpdateMap();
            }, "Generating...", doAsynchronously: false, null);
        }

        private void GenerateTileFor(int tileID)
        {
            Tile tile = Find.WorldGrid[tileID];
            Vector3 tileCenter = Find.WorldGrid.GetTileCenter(tileID);
            tile.elevation = noiseElevation.GetValue(tileCenter);
            float value = noiseMountainLines.GetValue(tileCenter);
            if (value > 0.235f || tile.elevation <= 0f)
            {
                if (tile.elevation > 0f && noiseHillsPatchesMicro.GetValue(tileCenter) > 0.46f && noiseHillsPatchesMacro.GetValue(tileCenter) > -0.3f)
                {
                    if (Rand.Bool)
                    {
                        tile.hilliness = Hilliness.SmallHills;
                    }
                    else
                    {
                        tile.hilliness = Hilliness.LargeHills;
                    }
                }
                else
                {
                    tile.hilliness = Hilliness.Flat;
                }
            }
            else if (value > 0.12f)
            {
                switch (Rand.Range(0, 4))
                {
                    case 0:
                        tile.hilliness = Hilliness.Flat;
                        break;
                    case 1:
                        tile.hilliness = Hilliness.SmallHills;
                        break;
                    case 2:
                        tile.hilliness = Hilliness.LargeHills;
                        break;
                    case 3:
                        tile.hilliness = Hilliness.Mountainous;
                        break;
                }
            }
            else if (value > 0.0363f)
            {
                tile.hilliness = Hilliness.Mountainous;
            }
            else
            {
                tile.hilliness = Hilliness.Impassable;
            }
            Vector2 vector = Find.WorldGrid.LongLatOf(tileID);
            float num = BaseTemperatureAtLatitude(vector.y);
            num -= TemperatureReductionAtElevation(tile.elevation);
            num += noiseTemperatureOffset.GetValue(tileCenter);
            SimpleCurve temperatureCurve = Find.World.info.overallTemperature.GetTemperatureCurve();
            if (temperatureCurve != null)
            {
                num = temperatureCurve.Evaluate(num);
            }
            tile.temperature = num;
            tile.rainfall = noiseRainfall.GetValue(tileCenter);
            if (float.IsNaN(tile.rainfall))
            {
                float value2 = noiseRainfall.GetValue(tileCenter);
                Log.ErrorOnce(value2 + " rain bad at " + tileID, 694822);
            }
            if (tile.hilliness == Hilliness.Flat || tile.hilliness == Hilliness.SmallHills)
            {
                tile.swampiness = noiseSwampiness.GetValue(tileCenter);
            }
            tile.biome = BiomeFrom(tile, tileID);
        }

        private BiomeDef BiomeFrom(Tile ws, int tileID)
        {
            List<BiomeDef> allDefsListForReading = DefDatabase<BiomeDef>.AllDefsListForReading;
            BiomeDef biomeDef = null;
            float num = 0f;
            for (int i = 0; i < allDefsListForReading.Count; i++)
            {
                BiomeDef biomeDef2 = allDefsListForReading[i];
                if (biomeDef2.implemented)
                {
                    float score = biomeDef2.Worker.GetScore(ws, tileID);
                    if (score > num || biomeDef == null)
                    {
                        biomeDef = biomeDef2;
                        num = score;
                    }
                }
            }
            return biomeDef;
        }

        private float BaseTemperatureAtLatitude(float lat)
        {
            float x = Mathf.Abs(lat) / 90f;
            return AvgTempByLatitudeCurve.Evaluate(x);
        }

        private float TemperatureReductionAtElevation(float elev)
        {
            if (elev < 250f)
            {
                return 0f;
            }
            float t = (elev - 250f) / 4750f;
            return Mathf.Lerp(0f, 40f, t);
        }

        private void SetupElevationNoise()
        {
            float freqMultiplier = FreqMultiplier;
            ModuleBase lhs = new Perlin(0.035f * freqMultiplier, 2.0, 0.40000000596046448, 6, Rand.Range(0, int.MaxValue), QualityMode.High);
            ModuleBase lhs2 = new RidgedMultifractal(0.012f * freqMultiplier, 2.0, 6, Rand.Range(0, int.MaxValue), QualityMode.High);
            ModuleBase input = new Perlin(0.12f * freqMultiplier, 2.0, 0.5, 5, Rand.Range(0, int.MaxValue), QualityMode.High);
            ModuleBase moduleBase = new Perlin(0.01f * freqMultiplier, 2.0, 0.5, 5, Rand.Range(0, int.MaxValue), QualityMode.High);
            float num;
            if (Find.World.PlanetCoverage < 0.55f)
            {
                ModuleBase input2 = new DistanceFromPlanetViewCenter(Find.WorldGrid.viewCenter, Find.WorldGrid.viewAngle, invert: true);
                input2 = new ScaleBias(2.0, -1.0, input2);
                moduleBase = new Blend(moduleBase, input2, new Const(0.40000000596046448));
                num = Rand.Range(-0.4f, -0.35f);
            }
            else
            {
                num = Rand.Range(0.15f, 0.25f);
            }
            input = new ScaleBias(0.5, 0.5, input);
            lhs2 = new Multiply(lhs2, input);
            float num2 = Rand.Range(0.4f, 0.6f);
            noiseElevation = new Blend(lhs, lhs2, new Const(num2));
            noiseElevation = new Blend(noiseElevation, moduleBase, new Const(num));
            if (Find.World.PlanetCoverage < 0.9999f)
            {
                noiseElevation = new ConvertToIsland(Find.WorldGrid.viewCenter, Find.WorldGrid.viewAngle, noiseElevation);
            }
            noiseElevation = new ScaleBias(0.5, 0.5, noiseElevation);
            noiseElevation = new Power(noiseElevation, new Const(3.0));
            double scale = ElevationRange.Span;
            FloatRange elevationRange = ElevationRange;
            noiseElevation = new ScaleBias(scale, elevationRange.min, noiseElevation);
        }

        private void SetupTemperatureOffsetNoise()
        {
            float freqMultiplier = FreqMultiplier;
            noiseTemperatureOffset = new Perlin(0.018f * freqMultiplier, 2.0, 0.5, 6, Rand.Range(0, int.MaxValue), QualityMode.High);
            noiseTemperatureOffset = new Multiply(noiseTemperatureOffset, new Const(4.0));
        }

        private void SetupRainfallNoise()
        {
            float freqMultiplier = FreqMultiplier;
            ModuleBase input = new Perlin(0.015f * freqMultiplier, 2.0, 0.5, 6, Rand.Range(0, int.MaxValue), QualityMode.High);
            input = new ScaleBias(0.5, 0.5, input);
            ModuleBase moduleBase = new AbsLatitudeCurve(SimpleCurveRainfall, 100f);
            noiseRainfall = new Multiply(input, moduleBase);
            float num = 0.000222222225f;
            float num2 = -500f * num;
            ModuleBase input2 = new ScaleBias(num, num2, noiseElevation);
            input2 = new ScaleBias(-1.0, 1.0, input2);
            input2 = new Clamp(0.0, 1.0, input2);

            noiseRainfall = new Multiply(noiseRainfall, input2);
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

        private void SetupHillinessNoise()
        {
            float freqMultiplier = FreqMultiplier;
            noiseMountainLines = new Perlin(0.025f * freqMultiplier, 2.0, 0.5, 6, Rand.Range(0, int.MaxValue), QualityMode.High);
            ModuleBase module = new Perlin(0.06f * freqMultiplier, 2.0, 0.5, 6, Rand.Range(0, int.MaxValue), QualityMode.High);
            noiseMountainLines = new Abs(noiseMountainLines);
            noiseMountainLines = new OneMinus(noiseMountainLines);
            module = new Filter(module, -0.3f, 1f);
            noiseMountainLines = new Multiply(noiseMountainLines, module);
            noiseMountainLines = new OneMinus(noiseMountainLines);
            noiseHillsPatchesMacro = new Perlin(0.032f * freqMultiplier, 2.0, 0.5, 5, Rand.Range(0, int.MaxValue), QualityMode.Medium);
            noiseHillsPatchesMicro = new Perlin(0.19f * freqMultiplier, 2.0, 0.5, 6, Rand.Range(0, int.MaxValue), QualityMode.High);
        }

        private void SetupSwampinessNoise()
        {
            float freqMultiplier = FreqMultiplier;
            ModuleBase input = new Perlin(0.09f * freqMultiplier, 2.0, 0.40000000596046448, 6, Rand.Range(0, int.MaxValue), QualityMode.High);
            ModuleBase input2 = new RidgedMultifractal(0.025f * freqMultiplier, 2.0, 6, Rand.Range(0, int.MaxValue), QualityMode.High);
            input = new ScaleBias(0.5, 0.5, input);
            input2 = new ScaleBias(0.5, 0.5, input2);
            noiseSwampiness = new Multiply(input, input2);
            ModuleBase module = noiseElevation;
            FloatRange swampinessMaxElevation = SwampinessMaxElevation;
            float max = swampinessMaxElevation.max;
            FloatRange swampinessMaxElevation2 = SwampinessMaxElevation;
            InverseLerp rhs = new InverseLerp(module, max, swampinessMaxElevation2.min);
            noiseSwampiness = new Multiply(noiseSwampiness, rhs);
            ModuleBase module2 = noiseRainfall;
            FloatRange swampinessMinRainfall = SwampinessMinRainfall;
            float min = swampinessMinRainfall.min;
            FloatRange swampinessMinRainfall2 = SwampinessMinRainfall;
            InverseLerp rhs2 = new InverseLerp(module2, min, swampinessMinRainfall2.max);
            noiseSwampiness = new Multiply(noiseSwampiness, rhs2);
        }
    }
}
