using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
        Full,
        Features,
        Faction,
        Lakes,
        Roads,
        Rivers,
        AncientRoads,
        AncientSites
    };

    public enum GeneratorType : byte
    {
        Vanilla = 1,
        WorldEdit = 2
    };

    public static class TerrainManager
    {
        public static Dictionary<GeneratorMode, List<Generator>> Generators = new Dictionary<GeneratorMode, List<Generator>>();

        static TerrainManager()
        {
            Type basicType = typeof(Generator);
            IEnumerable<Type> list = Assembly.GetAssembly(basicType).GetTypes().Where(type => type.IsSubclassOf(basicType));
            List<Generator> gens = new List<Generator>();
            foreach (Type itm in list)
            {
                Generator instance = Activator.CreateInstance(itm) as Generator;
                gens.Add(instance);
            }

            foreach (GeneratorMode mode in Enum.GetValues(typeof(GeneratorMode)))
            {
                List<Generator> generatorsForType = gens.Where(gen => gen.Mode == mode).ToList();
                Generators.Add(mode, generatorsForType);
            }
        }

        public static void Run(Generator generator)
        {
            LongEventHandler.QueueLongEvent(delegate
            {
                generator.RunGenerator();
            }, "Generating...", doAsynchronously: false, null);
        }
    }
}
