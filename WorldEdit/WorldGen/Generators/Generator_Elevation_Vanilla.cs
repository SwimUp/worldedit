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
    public class Generator_Elevation_Vanilla : Generator
    {
        [Unsaved]
        private ModuleBase noiseElevation;

        public float FreqMultiplier = 1f;

        public FloatRange ElevationRange = new FloatRange(-500f, 5000f);

        public override GeneratorMode Mode => GeneratorMode.Elevation;

        public override GeneratorType Type => GeneratorType.Vanilla;

        public override string Description => Translator.Translate($"{GetType().Name}");

        public override string Title => Translator.Translate($"{GetType().Name}_title");

        public override void RunGenerator()
        {
            SetupElevationNoise();

            for (int i = 0; i < Find.WorldGrid.TilesCount; i++)
                GenerateFor(i);

            Messages.Message("Done", MessageTypeDefOf.NeutralEvent, false);
        }

        private void GenerateFor(int tileID)
        {
            Tile tile = Find.WorldGrid[tileID];
            Vector3 tileCenter = Find.WorldGrid.GetTileCenter(tileID);
            tile.elevation = noiseElevation.GetValue(tileCenter);
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

    }
}
