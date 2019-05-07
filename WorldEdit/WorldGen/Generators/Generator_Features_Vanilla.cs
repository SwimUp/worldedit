using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace WorldEdit.WorldGen.Generators
{
    public class Generator_Features_Vanilla : Generator
    {
        public override string Title => Translator.Translate($"{GetType().Name}_title");

        public override string Description => Translator.Translate($"{GetType().Name}");

        public override GeneratorMode Mode => GeneratorMode.Features;

        public override GeneratorType Type => GeneratorType.Vanilla;

        public override void RunGenerator()
        {
            LongEventHandler.QueueLongEvent(delegate
            {
                GenerateFeatures();
            }, "Generating features...", doAsynchronously: false, null);
        }

        private void GenerateFeatures()
        {
            for (int i = 0; i < Find.WorldGrid.TilesCount; i++)
            {
                Tile tile = Find.WorldGrid[i];
                tile.feature = null;
            }
            Find.WorldFeatures.features = new List<WorldFeature>();

            IOrderedEnumerable<FeatureDef> orderedEnumerable = from x in DefDatabase<FeatureDef>.AllDefsListForReading
                                                               orderby x.order, x.index
                                                               select x;
            foreach (FeatureDef item in orderedEnumerable)
            {
                try
                {
                    item.Worker.GenerateWhereAppropriate();
                }
                catch (Exception ex)
                {
                    Log.Error("Could not generate world features of def " + item + ": " + ex);
                }
            }

            Find.WorldFeatures.textsCreated = false;
            Find.WorldFeatures.UpdateFeatures();
        }
    }
}
