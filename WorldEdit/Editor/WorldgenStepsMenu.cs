using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using WorldEdit.Interfaces;
using WorldEdit.WorldGen;

namespace WorldEdit.Editor
{
    internal class WorldGenStepsMenu : FWindow
    {
        public override Vector2 InitialSize => new Vector2(300, 500);

        public WorldGenStepsMenu()
        {
            resizeable = false;
        }

        public override void DoWindowContents(Rect inRect)
        {
            Text.Font = GameFont.Small;

            Widgets.Label(new Rect(0, 0, 280, 20), Translator.Translate("WorldGenStepsTitle"));
            WidgetRow row1 = new WidgetRow(0, 25, UIDirection.RightThenDown, 290);
            if(row1.ButtonText(Translator.Translate("RunTemperatureGenerator"), Translator.Translate("RunTemperatureGeneratorInfo")))
            {
                WorldEditor.TerrainManager.Run(GeneratorMode.Temperature);
            }
            if (row1.ButtonText(Translator.Translate("RunSwampGenerator"), Translator.Translate("RunSwampGeneratorInfo")))
            {
                WorldEditor.TerrainManager.Run(GeneratorMode.Swampiness);
            }
            if (row1.ButtonText(Translator.Translate("RunRainGenerator"), Translator.Translate("RunRainGeneratorInfo")))
            {
                WorldEditor.TerrainManager.Run(GeneratorMode.Rainfall);
            }
            if (row1.ButtonText(Translator.Translate("RunElevGenerator"), Translator.Translate("RunElevGeneratorInfo")))
            {
                WorldEditor.TerrainManager.Run(GeneratorMode.Elevation);
            }
            if (row1.ButtonText(Translator.Translate("RunHillGenerator"), Translator.Translate("RunHillGeneratorInfo")))
            {
                WorldEditor.TerrainManager.Run(GeneratorMode.Hilliness);
                WorldEditor.WorldUpdater.UpdateLayer(WorldEditor.Editor.Layers["WorldLayer_Hills"]);
            }
            if (row1.ButtonText(Translator.Translate("RunBiomeGenerator"), Translator.Translate("RunBiomeGeneratorInfo")))
            {
                WorldEditor.TerrainManager.Run(GeneratorMode.Biome);
                WorldEditor.WorldUpdater.UpdateMap();
            }
            if (row1.ButtonText(Translator.Translate("RunFullGenerator"), Translator.Translate("RunFullGenerator")))
            {
                WorldEditor.TerrainManager.Run(GeneratorMode.Full);
                WorldEditor.WorldUpdater.UpdateMap();
            }
        }
    }
}
