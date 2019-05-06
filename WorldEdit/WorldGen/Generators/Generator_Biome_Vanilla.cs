using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace WorldEdit.WorldGen.Generators
{
    public class Generator_Biome_Vanilla : Generator
    {
        public override GeneratorMode Mode => GeneratorMode.Biome;

        public override GeneratorType Type => GeneratorType.Vanilla;

        public override string Description => Translator.Translate($"{GetType().Name}");

        public override string Title => Translator.Translate($"{GetType().Name}_title");

        public override void RunGenerator()
        {
            for (int i = 0; i < Find.WorldGrid.TilesCount; i++)
                GenerateFor(i);

            WorldEditor.WorldUpdater.UpdateMap();
        }

        private void GenerateFor(int tileID)
        {
            Tile tile = Find.WorldGrid[tileID];
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
    }
}
