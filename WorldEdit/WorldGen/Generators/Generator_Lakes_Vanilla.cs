using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace WorldEdit.WorldGen.Generators
{
    public class Generator_Lakes_Vanilla : Generator
    {
        public override GeneratorMode Mode => GeneratorMode.Lakes;

        public override GeneratorType Type => GeneratorType.Vanilla;

        public override string Description => Translator.Translate($"{GetType().Name}");

        public override string Title => Translator.Translate($"{GetType().Name}_title");

        public override void RunGenerator()
        {
            LongEventHandler.QueueLongEvent(delegate
            {
                GenerateLake();

            }, "Generating lakes...", doAsynchronously: false, null);
        }

        private void GenerateLake()
        {
            WorldGrid grid = Find.WorldGrid;
            bool[] touched = new bool[grid.TilesCount];
            List<int> oceanChunk = new List<int>();
            for (int i = 0; i < grid.TilesCount; i++)
            {
                if (touched[i] || grid[i].biome != BiomeDefOf.Ocean)
                {
                    continue;
                }
                Find.WorldFloodFiller.FloodFill(i, (int tid) => grid[tid].biome == BiomeDefOf.Ocean, delegate (int tid)
                {
                    oceanChunk.Add(tid);
                    touched[tid] = true;
                });
                if (oceanChunk.Count <= 15)
                {
                    for (int j = 0; j < oceanChunk.Count; j++)
                    {
                        grid[oceanChunk[j]].biome = BiomeDefOf.Lake;
                    }
                }
                oceanChunk.Clear();
            }
        }
    }
}
