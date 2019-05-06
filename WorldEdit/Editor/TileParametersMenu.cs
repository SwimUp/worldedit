using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using Verse;
using WorldEdit.Interfaces;

namespace WorldEdit.Editor
{
    class TileParametersMenu : FWindow
    {
        public override Vector2 InitialSize => new Vector2(540, 350);
        public Vector2 scroll1 = Vector2.zero;
        public Vector2 scrollPosition = Vector2.zero;

        private InGameEditor editor;

        private int tile = -1;

        /* Переменные для хранения температуры, осадков, высоты */
        internal string fieldValue = string.Empty;
        internal string fieldRainfallValue = string.Empty;
        internal string fieldElevationValue = string.Empty;
        internal string fieldSwampinessValue = string.Empty;
        /* ==================================================== */

        /* Переменные, указывающие, требуется ли обновить указанный параметр тайла */
        internal bool useTemperature = false;
        internal bool useEvelation = false;
        internal bool useRainfall = false;
        internal bool useSwampiness = false;
        /* ======================================================================= */

        /* Настройка температуры, осадков, высоты */
        internal float temperature = 20f;
        internal float elevation = 300f;
        internal float rainfall = 400f;
        internal float swampiness = 0f;
        /* ======================= */

        internal CustomRock customRockData = null;
        private List<ThingDef> rocks = new List<ThingDef>();
        internal bool updateRocks = false;

        public TileParametersMenu(InGameEditor editor)
        {
            resizeable = false;
            this.editor = editor;
        }

        public override void WindowUpdate()
        {
            int tileID = Find.WorldSelector.selectedTile;
            if (tileID >= 0)
            {
                if (editor.brushEnabled)
                {
                    List<int> neightbors = new List<int>();
                    editor.GetList(Find.WorldGrid.tileIDToNeighbors_offsets, Find.WorldGrid.tileIDToNeighbors_values, tileID, neightbors, editor.brushRadius.max);
                    foreach (var s in neightbors)
                    {
                        Tile tile = Find.WorldGrid[s];

                        if (updateRocks)
                        {
                            if (!CustomNaturalRocks.ResourceData.Keys.Contains(tileID))
                            {
                                customRockData.SetRocksList(rocks);
                                customRockData.Tile = tileID;
                                CustomNaturalRocks.ResourceData.Add(tileID, customRockData);
                            }
                        }

                        if ((tile.temperature == temperature) && (tile.rainfall == rainfall) && (tile.elevation == elevation)
                            && (tile.swampiness == swampiness))
                            continue;

                        if (useTemperature)
                            tile.temperature = temperature;

                        if (useEvelation)
                            tile.elevation = elevation;

                        if (useRainfall)
                            tile.rainfall = rainfall;

                        if (useSwampiness)
                            tile.swampiness = swampiness;
                    }
                }
                else
                {
                    Tile tile = Find.WorldGrid[tileID];
                    if (tile != null)
                    {
                        if ((tile.temperature == temperature) && (tile.rainfall == rainfall) && (tile.elevation == elevation)
                            && (tile.swampiness == swampiness))
                        {
                            return;
                        }

                        if (useTemperature)
                            tile.temperature = temperature;

                        if (useEvelation)
                            tile.elevation = elevation;

                        if (useRainfall)
                            tile.rainfall = rainfall;

                        if (useSwampiness)
                            tile.swampiness = swampiness;
                    }
                }
            }
        }

        public override void DoWindowContents(Rect inRect)
        {
            Text.Font = GameFont.Small;

            if (Find.WorldSelector.selectedTile != tile)
                UpdateTileInfo();

            Widgets.Label(new Rect(0, 0, 210, 20), Translator.Translate("TemperatureMenuTitle"));
            int yButtonPos = 25;
            Widgets.Label(new Rect(0, yButtonPos, 100, 20), Translator.Translate("Temperature"));
            fieldValue = Widgets.TextField(new Rect(100, yButtonPos, 100, 20), fieldValue);
            if (float.TryParse(fieldValue, out float temperature))
            {
                this.temperature = temperature;
            }
            if (Widgets.RadioButtonLabeled(new Rect(200, yButtonPos, 50, 20), "", useTemperature == true))
            {
                useTemperature = !useTemperature;
            }
            yButtonPos += 25;
            if (Widgets.ButtonText(new Rect(0, yButtonPos, 250, 20), Translator.Translate("SetToAllMap")))
            {
                SetToAllMap(SetType.temp);
            }
            yButtonPos += 25;
            if (Widgets.ButtonText(new Rect(0, yButtonPos, 250, 20), Translator.Translate("SetToAllBiome")))
            {
                SetToAllBiomes(SetType.temp);
            }

            yButtonPos += 25;
            Widgets.Label(new Rect(0, yButtonPos, 100, 20), Translator.Translate("Evelation"));
            fieldElevationValue = Widgets.TextField(new Rect(100, yButtonPos, 100, 20), fieldElevationValue);
            if (float.TryParse(fieldElevationValue, out float evel))
            {
                elevation = evel;
            }
            if (Widgets.RadioButtonLabeled(new Rect(200, yButtonPos, 50, 20), "", useEvelation == true))
            {
                useEvelation = !useEvelation;
            }
            yButtonPos += 25;
            if (Widgets.ButtonText(new Rect(0, yButtonPos, 250, 20), Translator.Translate("SetToAllMap")))
            {
                SetToAllMap(SetType.evel);
            }
            yButtonPos += 25;
            if (Widgets.ButtonText(new Rect(0, yButtonPos, 250, 20), Translator.Translate("SetToAllBiome")))
            {
                SetToAllBiomes(SetType.evel);
            }

            yButtonPos += 25;
            Widgets.Label(new Rect(0, yButtonPos, 100, 20), Translator.Translate("Rainfall"));
            fieldRainfallValue = Widgets.TextField(new Rect(100, yButtonPos, 100, 20), fieldRainfallValue);
            if (float.TryParse(fieldRainfallValue, out float rain))
            {
                rainfall = rain;
            }
            if (Widgets.RadioButtonLabeled(new Rect(200, yButtonPos, 50, 20), "", useRainfall == true))
            {
                useRainfall = !useRainfall;
            }
            yButtonPos += 25;
            if (Widgets.ButtonText(new Rect(0, yButtonPos, 250, 20), Translator.Translate("SetToAllMap")))
            {
                SetToAllMap(SetType.rain);
            }
            yButtonPos += 25;
            if (Widgets.ButtonText(new Rect(0, yButtonPos, 250, 20), Translator.Translate("SetToAllBiome")))
            {
                SetToAllBiomes(SetType.rain);
            }

            yButtonPos += 25;
            Widgets.Label(new Rect(0, yButtonPos, 100, 20), Translator.Translate("Swampiness"));
            fieldSwampinessValue = Widgets.TextField(new Rect(100, yButtonPos, 100, 20), fieldSwampinessValue);
            if (float.TryParse(fieldSwampinessValue, out float swa))
            {
                swampiness = swa;
            }
            if (Widgets.RadioButtonLabeled(new Rect(200, yButtonPos, 50, 20), "", useSwampiness == true))
            {
                useSwampiness = !useSwampiness;
            }
            yButtonPos += 25;
            if (Widgets.ButtonText(new Rect(0, yButtonPos, 250, 20), Translator.Translate("SetToAllMap")))
            {
                SetToAllMap(SetType.swamp);
            }
            yButtonPos += 25;
            if (Widgets.ButtonText(new Rect(0, yButtonPos, 250, 20), Translator.Translate("SetToAllBiome")))
            {
                SetToAllBiomes(SetType.swamp);
            }

            DoRightRow();
        }

        private void DoRightRow()
        {
            if (Find.WorldSelector.selectedTile < 0)
            {
                Widgets.Label(new Rect(270, 25, 240, 20), Translator.Translate("SelectTileTile"));
                return;
            }

            int y = 25;

            if (Widgets.ButtonText(new Rect(270, y, 200, 20), Translator.Translate("AddNewNaturalRock")))
            {
                List<FloatMenuOption> list = new List<FloatMenuOption>();
                foreach (var thing in (from d in DefDatabase<ThingDef>.AllDefs
                                       where d.category == ThingCategory.Building && d.building.isNaturalRock && !d.building.isResourceRock && !d.IsSmoothed
                                       select d))
                    list.Add(new FloatMenuOption(thing.LabelCap, delegate
                    {
                        if (updateRocks)
                        {
                            customRockData.Rocks.Add(thing);
                            rocks.Add(thing);
                        }
                        else
                        {
                            rocks.Add(thing);
                        }

                    }));
                Find.WindowStack.Add(new FloatMenu(list));
            }

            y += 25;

            int size = rocks.Count * 22;
            Rect scrollRectFact = new Rect(270, y, 240, 190);
            Rect scrollVertRectFact = new Rect(0, 0, scrollRectFact.x, size);
            Widgets.BeginScrollView(scrollRectFact, ref scrollPosition, scrollVertRectFact);
            y = 0;
            for (int i = 0; i < rocks.Count; i++)
            {
                ThingDef d = rocks[i];
                Widgets.Label(new Rect(0, y, 160, 20), d.LabelCap);
                if (Widgets.ButtonText(new Rect(170, y, 20, 20), "X"))
                {
                    rocks.Remove(d);
                }
                y += 22;
            }
            Widgets.EndScrollView();

            y = 240;

            if (Widgets.RadioButtonLabeled(new Rect(270, y, 240, 40), Translator.Translate("UpdateRockType"), updateRocks == true))
            {
                updateRocks = !updateRocks;
            }

            y += 45;

            if (Widgets.RadioButtonLabeled(new Rect(270, y, 240, 40), Translator.Translate("HasCaves"), customRockData.Caves == true))
            {
                customRockData.Caves = !customRockData.Caves;
            }
        }

        private void UpdateTileInfo()
        {
            tile = Find.WorldSelector.selectedTile;

            if (tile < 0)
                return;

            Tile t = Find.WorldGrid[tile];

            fieldValue = t.temperature.ToString();
            fieldSwampinessValue = t.swampiness.ToString();
            fieldRainfallValue = t.rainfall.ToString();
            fieldElevationValue = t.elevation.ToString();

            temperature = t.temperature;
            swampiness = t.swampiness;
            rainfall = t.rainfall;
            elevation = t.elevation;

            if (CustomNaturalRocks.ResourceData.Keys.Contains(tile))
            {
                customRockData = CustomNaturalRocks.ResourceData[tile];
            }
            else
            {
                customRockData = new CustomRock(tile, Find.World.NaturalRockTypesIn(tile).ToList(), Find.World.HasCaves(tile));
            }

            rocks = new List<ThingDef>(customRockData.Rocks);
        }

        private void SetToAllMap(SetType type)
        {
            WorldGrid grid = Find.WorldGrid;

            switch (type)
            {
                case SetType.temp:
                    {
                        grid.tiles.Where(tile => tile.biome != BiomeDefOf.Ocean && tile.biome != BiomeDefOf.Lake).ForEach(tile => tile.temperature = temperature);
                        Messages.Message($"Temperature {temperature} set to all map", MessageTypeDefOf.NeutralEvent, false);

                        break;
                    }
                case SetType.evel:
                    {
                        grid.tiles.Where(tile => tile.biome != BiomeDefOf.Ocean && tile.biome != BiomeDefOf.Lake).ForEach(tile => tile.elevation = elevation);
                        Messages.Message($"Elevation {elevation} set to all map", MessageTypeDefOf.NeutralEvent, false);

                        break;
                    }
                case SetType.rain:
                    {
                        grid.tiles.Where(tile => tile.biome != BiomeDefOf.Ocean && tile.biome != BiomeDefOf.Lake).ForEach(tile => tile.rainfall = rainfall);
                        Messages.Message($"Rainfall {rainfall} set to all map", MessageTypeDefOf.NeutralEvent, false);

                        break;
                    }
                case SetType.swamp:
                    {
                        grid.tiles.Where(tile => tile.biome != BiomeDefOf.Ocean && tile.biome != BiomeDefOf.Lake).ForEach(tile => tile.swampiness = swampiness);
                        Messages.Message($"Swampiness {swampiness} set to all map", MessageTypeDefOf.NeutralEvent, false);

                        break;
                    }
            }
        }

        private void SetToAllBiomes(SetType type)
        {
            if (editor.selectedBiome == null)
            {
                Messages.Message($"First choose a biome", MessageTypeDefOf.NeutralEvent, false);
                return;
            }

            WorldGrid grid = Find.WorldGrid;

            switch (type)
            {
                case SetType.temp:
                    {
                        grid.tiles.Where(tile => tile.biome == editor.selectedBiome).ForEach(tile => tile.temperature = temperature);
                        Messages.Message($"Temperature {temperature} set to all {editor.selectedBiome.defName}", MessageTypeDefOf.NeutralEvent, false);

                        break;
                    }
                case SetType.evel:
                    {
                        grid.tiles.Where(tile => tile.biome == editor.selectedBiome).ForEach(tile => tile.elevation = elevation);
                        Messages.Message($"Elevation {elevation} set to all {editor.selectedBiome.defName}", MessageTypeDefOf.NeutralEvent, false);

                        break;
                    }
                case SetType.rain:
                    {
                        grid.tiles.Where(tile => tile.biome == editor.selectedBiome).ForEach(tile => tile.rainfall = rainfall);
                        Messages.Message($"Rainfall {rainfall} set to all {editor.selectedBiome.defName}", MessageTypeDefOf.NeutralEvent, false);

                        break;
                    }
                case SetType.swamp:
                    {
                        grid.tiles.Where(tile => tile.biome == editor.selectedBiome).ForEach(tile => tile.swampiness = swampiness);
                        Messages.Message($"Swampiness {swampiness} set to all {editor.selectedBiome.defName}", MessageTypeDefOf.NeutralEvent, false);

                        break;
                    }
            }
        }
    }
}
