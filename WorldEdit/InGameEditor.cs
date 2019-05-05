using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Verse;
using WorldEdit.Editor;

namespace WorldEdit
{
    public enum SetType : byte
    {
        temp = 0,
        evel,
        rain,
        swamp
    }

    public sealed class InGameEditor : EditWindow
    {
        public override Vector2 InitialSize => new Vector2(600, 600);

        private Vector2 mainScrollPosition = Vector2.zero;
        private Vector2 scrollPosition = Vector2.zero;
        private Vector2 scrollPositionFact = Vector2.zero;

        /// <summary>
        /// Список биомов
        /// </summary>
        private List<BiomeDef> avaliableBiomes { get; set; }
        /// <summary>
        /// Выбранный биом
        /// </summary>
        internal BiomeDef selectedBiome = null;
        /// <summary>
        /// Выбарнные горы
        /// </summary>
        private Hilliness selectedHillness = Hilliness.Flat;

        /// <summary>
        /// Немедленное обновление тайлов
        /// </summary>
        private bool updateImmediately = false;

        /// <summary>
        /// Список слоёв (Ключ - имя класса)
        /// </summary>
        public Dictionary<string, WorldLayer> Layers;
        /// <summary>
        /// Список подслоев у слоя (Ключ - имя класс слоя)
        /// </summary>
        public Dictionary<string, List<LayerSubMesh>> LayersSubMeshes;

        /// <summary>
        /// WorldUpdater
        /// </summary>
        internal WorldUpdater WorldUpdater;

        /// <summary>
        /// Редактор дорог и рек
        /// </summary>
        internal RoadAndRiversEditor roadEditor;

        /// <summary>
        /// Окно со списком всех слоёв для обновления конкретного
        /// </summary>
        internal LayersWindow layersWindow;

        /// <summary>
        /// Редактор фракций и поселений
        /// </summary>
        internal FactionMenu factionEditor;

        /// <summary>
        /// Редактор глобальных объектов
        /// </summary>
        internal WorldObjectsEditor worldObjectsEditor;

        internal IntRange brushRadius = new IntRange();
        internal bool brushEnabled = false;

        public InGameEditor()
        {
            resizeable = false;

            avaliableBiomes = DefDatabase<BiomeDef>.AllDefs.ToList();

            FieldInfo fieldlayers = typeof(WorldRenderer).GetField("layers", BindingFlags.NonPublic | BindingFlags.Instance);
            FieldInfo fieldMeshes = typeof(WorldLayer).GetField("subMeshes", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
                | BindingFlags.Static);
            var tempLayers = fieldlayers.GetValue(Find.World.renderer) as List<WorldLayer>;
            Layers = new Dictionary<string, WorldLayer>(tempLayers.Count);
            LayersSubMeshes = new Dictionary<string, List<LayerSubMesh>>(tempLayers.Count);
            foreach (var layer in tempLayers)
            {
                Layers.Add(layer.GetType().Name, layer);
                List<LayerSubMesh> meshes = fieldMeshes.GetValue(layer) as List<LayerSubMesh>;
                LayersSubMeshes.Add(layer.GetType().Name, meshes);
            }

            roadEditor = new RoadAndRiversEditor();
            WorldUpdater = WorldEditor.WorldUpdater;
            layersWindow = new LayersWindow();
            factionEditor = new FactionMenu();
            worldObjectsEditor = new WorldObjectsEditor();
        }

        public void Reset()
        {
            selectedBiome = null;

            Find.WorldSelector.ClearSelection();
        }

        public void GetList(List<int> offsets, List<int> values, int listIndex, List<int> outList, int radius)
        {
            outList.Clear();
            outList.Add(listIndex);

            for (int r = 0; r < radius; r++)
            {
                int size = outList.Count;
                for (int l = 0; l < size; l++)
                {
                    listIndex = outList[l];

                    int num = offsets[listIndex];
                    int num2 = values.Count;
                    if (listIndex + +1 < offsets.Count)
                    {
                        num2 = offsets[listIndex + 1];
                    }
                    for (int i = num; i < num2; i++)
                    {
                        outList.Add(values[i]);
                    }
                }
            }
        }

        public override void WindowUpdate()
        {
            int tileID = Find.WorldSelector.selectedTile;
            if (tileID >= 0)
            {
                if (brushEnabled)
                {
                    List<int> neightbors = new List<int>();
                    GetList(Find.WorldGrid.tileIDToNeighbors_offsets, Find.WorldGrid.tileIDToNeighbors_values, tileID, neightbors, brushRadius.max);
                    foreach (var s in neightbors)
                    {
                        Tile tile = Find.WorldGrid[s];

                        if ((tile.biome == selectedBiome) && (tile.hilliness == selectedHillness))
                            return;

                        if (selectedBiome != null)
                        {
                            if (selectedBiome != tile.biome)
                            {
                                tile.biome = selectedBiome;

                                if (selectedBiome == BiomeDefOf.Ocean || selectedBiome == BiomeDefOf.Lake)
                                {
                                    tile.elevation = -400f;
                                }

                                if (updateImmediately)
                                {
                                    WorldUpdater.RenderSingleTile(neightbors, tile.biome.DrawMaterial, LayersSubMeshes["WorldLayer_Terrain"]);
                                }
                            }
                        }

                        if (selectedHillness != Hilliness.Undefined)
                        {
                            if (tile.hilliness != selectedHillness)
                            {
                                tile.hilliness = selectedHillness;
                                if (updateImmediately)
                                {
                                    WorldUpdater.RenderSingleHill(neightbors, LayersSubMeshes["WorldLayer_Hills"]);
                                }
                            }
                        }
                    }
                }
                else
                {
                    Tile tile = Find.WorldGrid[tileID];
                    if (tile != null)
                    {
                        if ((tile.biome == selectedBiome) && (tile.hilliness == selectedHillness))
                        {
                            return;
                        }
                            

                        if (selectedBiome != null)
                        {
                            if (selectedBiome != tile.biome)
                            {

                                tile.biome = selectedBiome;

                                if (selectedBiome == BiomeDefOf.Ocean || selectedBiome == BiomeDefOf.Lake)
                                {
                                    tile.elevation = -400f;
                                }

                                if (updateImmediately)
                                {
                                    WorldUpdater.RenderSingleTile(tileID, tile.biome.DrawMaterial, LayersSubMeshes["WorldLayer_Terrain"]);
                                }
                            }
                        }

                        if (selectedHillness != Hilliness.Undefined)
                        {
                            if (tile.hilliness != selectedHillness)
                            {
                                tile.hilliness = selectedHillness;
                                if (updateImmediately)
                                {
                                    WorldUpdater.RenderSingleHill(tileID, LayersSubMeshes["WorldLayer_Hills"]);
                                }
                            }
                        }
                    }
                }
            }
        }

        public override void DoWindowContents(Rect inRect)
        {
            Text.Font = GameFont.Small;

            WidgetRow row = new WidgetRow(0, 0, UIDirection.RightThenDown, 580);
            if(row.ButtonText(Translator.Translate("UpdateAllTiles"), Translator.Translate("UpdateAllTilesInfo")))
            {
                LongEventHandler.QueueLongEvent(delegate
                {
                    WorldUpdater.UpdateMap();
                }, "Updating layers...", doAsynchronously: false, null);
            }
            if (row.ButtonText(Translator.Translate("UpdateCustomLayer"), Translator.Translate("UpdateCustomLayerInfo")))
            {
                layersWindow.Show();
            }
            if (updateImmediately)
            {
                if (row.ButtonText(Translator.Translate("UpdateImmediatelyON"), Translator.Translate("UpdateImmediatelyInfo")))
                {
                    updateImmediately = false;
                }
            }
            else
            {
                if (row.ButtonText(Translator.Translate("UpdateImmediatelyOFF"), Translator.Translate("UpdateImmediatelyInfo")))
                {
                    updateImmediately = true;
                }
            }
            if (row.ButtonText(Translator.Translate("Utilities")))
            {
                Find.WindowStack.Add(new UtilsMenu());
            }
            if (row.ButtonText(Translator.Translate("SaveWorldTemplate")))
            {
                Find.WindowStack.Add(new WorldTemplateManager());
            }

            int size = 900;
            Rect mainScrollRect = new Rect(0, 25, 600, 600);
            Rect mainScrollVertRect = new Rect(0, 0, mainScrollRect.x, size);
            Widgets.BeginScrollView(mainScrollRect, ref mainScrollPosition, mainScrollVertRect);

            Rect group1 = new Rect(0, 20, inRect.width / 2, size);
            GUI.BeginGroup(group1);
            Widgets.Label(new Rect(0, 5, 50, 20), Translator.Translate("Biome"));
            int biomeDefSize = avaliableBiomes.Count * 25;
            float group1Height = inRect.height - 360;
            Rect scrollRect = new Rect(0, 25, 250, group1Height);
            Rect scrollVertRect = new Rect(0, 0, scrollRect.x, biomeDefSize);
            Widgets.BeginScrollView(scrollRect, ref scrollPosition, scrollVertRect);
            int yButtonPos = 5;
            Text.Font = GameFont.Small;
            if (Widgets.ButtonText(new Rect(0, yButtonPos, 230, 20), Translator.Translate("NoText")))
            {
                selectedBiome = null;
                Messages.Message($"Biome: none", MessageTypeDefOf.NeutralEvent, false);
            }
            yButtonPos += 25;
            foreach (BiomeDef def in avaliableBiomes)
            {
                if(Widgets.ButtonText(new Rect(0, yButtonPos, 230, 20), def.label))
                {
                    selectedBiome = def;
                    Messages.Message($"Biome: {selectedBiome.defName}", MessageTypeDefOf.NeutralEvent, false);
                }
                yButtonPos += 22;
            }
            Widgets.EndScrollView();

            yButtonPos = 265;
            if (Widgets.ButtonText(new Rect(0, yButtonPos, 250, 20), Translator.Translate("ClearAllHillnses")))
            {
                ClearAllHillnes();
            }

            yButtonPos = 290;
            if (Widgets.ButtonText(new Rect(0, yButtonPos, 250, 20), Translator.Translate("SetTileToAllMap")))
            {
                SetBiomeToAllTiles();
            }

            yButtonPos = 320;
            foreach (Hilliness hillnes in Enum.GetValues(typeof(Hilliness)))
            {
                if (Widgets.RadioButtonLabeled(new Rect(0, yButtonPos, 250, 20), hillnes.ToString(), hillnes == selectedHillness))
                {
                    selectedHillness = hillnes;
                }
                yButtonPos += 20;
            }
            yButtonPos += 10;
            if (Widgets.ButtonText(new Rect(0, yButtonPos, 250, 20), Translator.Translate("SetHillnesToAllMap")))
            {
                SetHillnesToAllMap();
            }
            yButtonPos += 30;
            if (Widgets.ButtonText(new Rect(0, yButtonPos, 250, 20), Translator.Translate("SetHillnesToAllBiome")))
            {
                SetHillnesToAllBiome();
            }

            GUI.EndGroup();

            float group2Width = 270;
            Rect group2 = new Rect(group2Width, 20, inRect.width / 2, inRect.height);
            GUI.BeginGroup(group2);
            yButtonPos = 25;
            if(Widgets.RadioButtonLabeled(new Rect(0, yButtonPos, 250, 20), $"{Translator.Translate("BrushEnable")} - {brushRadius.max}", brushEnabled))
            {
                brushEnabled = !brushEnabled;
            }
            yButtonPos += 25;
            Widgets.IntRange(new Rect(0, yButtonPos, 250, 20), 3424354, ref brushRadius, 0, 3, Translator.Translate("BrushSettings"));
            yButtonPos += 50;
            if(Widgets.ButtonText(new Rect(0, yButtonPos, 250, 20), Translator.Translate("TemperatureMenuTitle")))
            {
                Find.WindowStack.Add(new TileParametersMenu(this));
            }
            yButtonPos += 50;
            if (Widgets.ButtonText(new Rect(0, yButtonPos, 250, 20), Translator.Translate("FactionEditor")))
            {
                factionEditor.Show();
            }
            Widgets.Label(new Rect(255, yButtonPos, 35, 20), $"{Settings.FactionHotKey}");

            yButtonPos += 25;
            if (Widgets.ButtonText(new Rect(0, yButtonPos, 250, 20), Translator.Translate("FactionDiagramm")))
            {
                Find.WindowStack.Add(new Dialog_FactionDuringLanding());
            }

            yButtonPos += 35;
            if (Widgets.ButtonText(new Rect(0, yButtonPos, 250, 20), Translator.Translate("RoadAndRiverEditor")))
            {
                roadEditor.Show();
            }
            Widgets.Label(new Rect(255, yButtonPos, 35, 20), $"{Settings.RiversAndRoadsHotKey}");

            yButtonPos += 35;
            if (Widgets.ButtonText(new Rect(0, yButtonPos, 250, 20), Translator.Translate("WorldFeaturesEditor")))
            {
                worldObjectsEditor.Show();
            }
            Widgets.Label(new Rect(255, yButtonPos, 35, 20), $"{Settings.WorldObjectHotKey}");

            GUI.EndGroup();

            Widgets.EndScrollView();
        }

        private void ClearAllHillnes()
        {
            WorldGrid grid = Find.WorldGrid;
            for(int i = 0; i < grid.TilesCount; i++)
            {
                Tile tile = grid[i];
                tile.hilliness = Hilliness.Flat;
            }

            WorldUpdater.UpdateLayer(Layers["WorldLayer_Hills"]);

            Messages.Message($"Hilliness removed", MessageTypeDefOf.NeutralEvent, false);
        }

        private void SetBiomeToAllTiles()
        {
            if (selectedBiome == null)
            {
                Messages.Message($"First choose a biome", MessageTypeDefOf.NeutralEvent, false);
                return;
            }

            WorldGrid grid = Find.WorldGrid;
            grid.tiles.Where(tile => tile.biome != BiomeDefOf.Ocean && tile.biome != BiomeDefOf.Lake).ForEach(tile => 
            {
                tile.biome = selectedBiome;
            });

            LongEventHandler.QueueLongEvent(delegate
            {
                LayersSubMeshes["WorldLayer_Terrain"].Clear();
                WorldUpdater.UpdateLayer(Layers["WorldLayer_Terrain"]);
            }, "Set biome on the whole map ...", doAsynchronously: false, null);
        }

        private void SetHillnesToAllMap()
        {
            if (selectedHillness == Hilliness.Undefined)
            {
                Messages.Message($"First choose a hilliness type", MessageTypeDefOf.NeutralEvent, false);
                return;
            }

            WorldGrid grid = Find.WorldGrid;
            grid.tiles.Where(tile => tile.biome != BiomeDefOf.Ocean && tile.biome != BiomeDefOf.Lake).ForEach(tile => tile.hilliness = selectedHillness);

            WorldUpdater.UpdateLayer(Layers["WorldLayer_Hills"]);
        }

        private void SetHillnesToAllBiome()
        {
            if (selectedHillness == Hilliness.Undefined)
            {
                Messages.Message($"First choose a hilliness type", MessageTypeDefOf.NeutralEvent, false);
                return;
            }

            if (selectedBiome == null)
            {
                Messages.Message($"Choose a biome", MessageTypeDefOf.NeutralEvent, false);
                return;
            }

            WorldGrid grid = Find.WorldGrid;
            grid.tiles.Where(tile => tile.biome == selectedBiome).ForEach(tile => tile.hilliness = selectedHillness);

            WorldUpdater.UpdateLayer(Layers["WorldLayer_Hills"]);
        }
    }
}
