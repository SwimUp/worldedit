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
    public class Generator_Hilliness_Vanilla : Generator
    {
        public override string Description => Translator.Translate($"{GetType().Name}");

        public override string Title => Translator.Translate($"{GetType().Name}_title");

        [Unsaved]
        private ModuleBase noiseMountainLines;

        [Unsaved]
        private ModuleBase noiseHillsPatchesMicro;

        [Unsaved]
        private ModuleBase noiseHillsPatchesMacro;

        public float FreqMultiplier => 1f;

        public override GeneratorMode Mode => GeneratorMode.Hilliness;

        public override GeneratorType Type => GeneratorType.Vanilla;

        public override void RunGenerator()
        {
            LongEventHandler.QueueLongEvent(delegate
            {
                SetupHillinessNoise();

                for (int i = 0; i < Find.WorldGrid.TilesCount; i++)
                    GenerateFor(i);

                WorldEditor.WorldUpdater.UpdateLayer(WorldEditor.Editor.Layers["WorldLayer_Hills"]);

            }, "Generating hilliness...", doAsynchronously: false, null);
        }

        private void GenerateFor(int tileID)
        {
            Tile tile = Find.WorldGrid[tileID];
            Vector3 tileCenter = Find.WorldGrid.GetTileCenter(tileID);
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
            NoiseDebugUI.StorePlanetNoise(noiseMountainLines, "noiseMountainLines");
            noiseHillsPatchesMacro = new Perlin(0.032f * freqMultiplier, 2.0, 0.5, 5, Rand.Range(0, int.MaxValue), QualityMode.Medium);
            noiseHillsPatchesMicro = new Perlin(0.19f * freqMultiplier, 2.0, 0.5, 6, Rand.Range(0, int.MaxValue), QualityMode.High);
        }
    }
}
