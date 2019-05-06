using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.Noise;
using WorldEdit.WorldGen.Generators;

namespace WorldEdit.WorldGen
{
    public enum GeneratorMode : byte
    {
        Temperature = 0,
        Elevation,
        Rainfall,
        Swampiness,
        Hilliness,
        Biome,
        Full
    };

    public enum GeneratorType : byte
    {
        Vanilla = 1,
        WorldEdit = 2
    };

    internal class TerrainManager
    {
        public void Run(Generator generator)
        {
            LongEventHandler.QueueLongEvent(delegate
            {
                generator.RunGenerator();
            }, "Generating...", doAsynchronously: false, null);
        }
    }
}
