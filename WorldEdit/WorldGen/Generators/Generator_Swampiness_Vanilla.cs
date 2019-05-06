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
    public class Generator_Swampiness_Vanilla : Generator
    {
        public override string Description => Translator.Translate($"{GetType().Name}");

        public override string Title => Translator.Translate($"{GetType().Name}_title");

        [Unsaved]
        private ModuleBase noiseSwampiness;

        public float FreqMultiplier => 1f;

        public override GeneratorMode Mode => GeneratorMode.Swampiness;

        public override GeneratorType Type => GeneratorType.Vanilla;

        public FloatRange SwampinessMaxElevation = new FloatRange(650f, 750f);

        public FloatRange SwampinessMinRainfall = new FloatRange(725f, 900f);

        public override void RunGenerator()
        {
            SetupSwampinessNoise();

            for (int i = 0; i < Find.WorldGrid.TilesCount; i++)
                GenerateFor(i);
        }

        private void GenerateFor(int tileID)
        {
            Tile tile = Find.WorldGrid[tileID];
            Vector3 tileCenter = Find.WorldGrid.GetTileCenter(tileID);
            if (tile.hilliness == Hilliness.Flat || tile.hilliness == Hilliness.SmallHills)
            {
                tile.swampiness = noiseSwampiness.GetValue(tileCenter);
            }
        }

        private void SetupSwampinessNoise()
        {
            float freqMultiplier = FreqMultiplier;
            ModuleBase input = new Perlin(0.09f * freqMultiplier, 2.0, 0.40000000596046448, 6, Rand.Range(0, int.MaxValue), QualityMode.High);
            ModuleBase input2 = new RidgedMultifractal(0.025f * freqMultiplier, 2.0, 6, Rand.Range(0, int.MaxValue), QualityMode.High);
            input = new ScaleBias(0.5, 0.5, input);
            input2 = new ScaleBias(0.5, 0.5, input2);
            noiseSwampiness = new Multiply(input, input2);
        }
    }
}
