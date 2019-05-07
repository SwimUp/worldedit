using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace WorldEdit.WorldGen.Generators
{
    public class Generator_AncientSites_Vanilla : Generator
    {
        public override string Description => Translator.Translate($"{GetType().Name}");

        public override string Title => Translator.Translate($"{GetType().Name}_title");

        public override GeneratorMode Mode => GeneratorMode.AncientSites;

        public override GeneratorType Type => GeneratorType.Vanilla;
        
        public FloatRange ancientSitesPer100kTiles;

        public override void RunGenerator()
        {
            GenerateAncientSites();

            Messages.Message("Done", MessageTypeDefOf.NeutralEvent, false);
        }

        private void GenerateAncientSites()
        {
            int num = GenMath.RoundRandom((float)Find.WorldGrid.TilesCount / 100000f * ancientSitesPer100kTiles.RandomInRange);
            for (int i = 0; i < num; i++)
            {
                Find.World.genData.ancientSites.Add(TileFinder.RandomSettlementTileFor(null));
            }
        }
    }
}
