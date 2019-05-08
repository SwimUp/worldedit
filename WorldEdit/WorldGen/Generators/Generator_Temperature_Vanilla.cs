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
    public class Generator_Temperature_Vanilla : Generator
    {
        public override string Description => Translator.Translate($"{GetType().Name}");

        public override string Title => Translator.Translate($"{GetType().Name}_title");

        [Unsaved]
        private ModuleBase noiseTemperatureOffset;

        public float FreqMultiplier = 1f;

        public SimpleCurve AvgTempByLatitudeCurve = new SimpleCurve
        {
            new CurvePoint(0f, 30f),
            new CurvePoint(0.1f, 29f),
            new CurvePoint(0.5f, 7f),
            new CurvePoint(1f, -37f)
        };

        public override GeneratorMode Mode => GeneratorMode.Temperature;
        public override GeneratorType Type => GeneratorType.Vanilla;

        public Generator_Temperature_Vanilla()
        {
            Settings.AddParam(GetType().GetField("FreqMultiplier"), FreqMultiplier);
            Settings.AddParam(GetType().GetField("AvgTempByLatitudeCurve"), AvgTempByLatitudeCurve);
        }

        public override void RunGenerator()
        {
            Setup();

            Log.Message($"TEMP: {AvgTempByLatitudeCurve[0].x}");

            SetupTemperatureOffsetNoise();

            for (int i = 0; i < Find.WorldGrid.TilesCount; i++)
                GenerateFor(i);

            Messages.Message("Done", MessageTypeDefOf.NeutralEvent, false);
        }

        private void SetupTemperatureOffsetNoise()
        {
            float freqMultiplier = FreqMultiplier;
            noiseTemperatureOffset = new Perlin(0.018f * freqMultiplier, 2.0, 0.5, 6, Rand.Range(0, int.MaxValue), QualityMode.High);
            noiseTemperatureOffset = new Multiply(noiseTemperatureOffset, new Const(4.0));
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

        private void GenerateFor(int tileID)
        {
            Tile tile = Find.WorldGrid[tileID];
            Vector3 tileCenter = Find.WorldGrid.GetTileCenter(tileID);
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
        }
    }
}
