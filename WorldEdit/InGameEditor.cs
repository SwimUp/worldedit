using Harmony;
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
using WorldEdit.Editor;

namespace WorldEdit
{
    [HarmonyPatch("PostOpen"), HarmonyPatch(typeof(Page_SelectStartingSite))]
    internal class WorldGridHook
    {
        public static void Postfix()
        {
            if(MainMenu.isEdit)
            {
                MainMenu.WorldUpdater = new WorldUpdater();

                MainMenu.Editor = new InGameEditor();
                Find.WindowStack.Add(MainMenu.Editor);
            }
        }
    }

    public class InGameEditor : EditWindow
    {
        private Vector2 mainScrollPosition = Vector2.zero;
        private Vector2 scrollPosition = Vector2.zero;
        private Vector2 scrollPositionFact = Vector2.zero;
        private List<BiomeDef> avaliableBiomes { get; set; }
        public override Vector2 InitialSize => new Vector2(600, 600);

        private BiomeDef selectedBiome = null;
        private Hilliness selectedHillness = Hilliness.Flat;
        private int temperature = 20;
        private float elevation = 300f;
        private float rainfall = 400f;
        private float swampiness = 0f;
        private bool updateImmediately = false;

        public string fieldValue = string.Empty;
        public string fieldRainfallValue = string.Empty;
        public string fieldElevationValue = string.Empty;
        public string fieldSwampinessValue = string.Empty;

        public Dictionary<string, WorldLayer> Layers;
        public Dictionary<string, List<LayerSubMesh>> LayersSubMeshes;
        private WorldLayer layerSingleTile = null;
        private WorldLayer layerHills = null;

        internal WorldUpdater WorldUpdater;
        private RoadAndRiversEditor roadEditor;
        private LayersWindow layersWindow;
        internal FactionMenu factionEditor;

        private bool useTemperature = false;
        private bool useEvelation = false;
        private bool useRainfall = false;
        private bool useSwampiness = false;

        public InGameEditor()
        {
            avaliableBiomes = new List<BiomeDef>(DefDatabase<BiomeDef>.DefCount);
            foreach (BiomeDef biome in DefDatabase<BiomeDef>.AllDefs)
            {
                avaliableBiomes.Add(biome);
            }

            resizeable = false;

            FieldInfo fieldlayers = typeof(WorldRenderer).GetField("layers", BindingFlags.NonPublic | BindingFlags.Instance);
            FieldInfo fieldMeshes = typeof(WorldLayer).GetField("subMeshes", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
                | BindingFlags.Static);
            var tempLayers = fieldlayers.GetValue(Find.World.renderer) as List<WorldLayer>;
            Layers = new Dictionary<string, WorldLayer>(tempLayers.Count);
            LayersSubMeshes = new Dictionary<string, List<LayerSubMesh>>(tempLayers.Count);
            foreach (var layer in tempLayers)
            {
                Log.Message($"FIELD: {(fieldMeshes.GetValue(layer) as List<LayerSubMesh>).Count}");

                Layers.Add(layer.GetType().Name, layer);
                List<LayerSubMesh> meshes = fieldMeshes.GetValue(layer) as List<LayerSubMesh>;
                LayersSubMeshes.Add(layer.GetType().Name, meshes);
            }

            layerHills = Layers["WorldLayer_Hills"];
            layerSingleTile = Layers["WorldLayer_CurrentMapTile"];

            roadEditor = new RoadAndRiversEditor();
            WorldUpdater = MainMenu.WorldUpdater;
            layersWindow = new LayersWindow();
            factionEditor = new FactionMenu();
        }

        public void Reset()
        {
            selectedBiome = null;

            Find.WorldSelector.ClearSelection();
        }

        public override void WindowUpdate()
        {
            int tileID = Find.WorldSelector.selectedTile;
            if (tileID >= 0)
            {
                Tile tile = Find.WorldGrid[tileID];
                if (tile != null)
                {
                    if ((tile.biome == selectedBiome) && (tile.hilliness == selectedHillness)
                        && (tile.temperature == temperature) && (tile.rainfall == rainfall) && (tile.elevation == elevation)
                        && (tile.swampiness == swampiness))
                        return;

                    if (selectedBiome != null)
                    {
                        if (selectedBiome != tile.biome)
                        {
                            tile.biome = selectedBiome;

                            if (updateImmediately)
                            {
                                WorldUpdater.RenderSingleTile(tileID, tile.biome.DrawMaterial, LayersSubMeshes["WorldLayer_Terrain"], Layers["WorldLayer_Terrain"]);
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

                    if(useTemperature)
                        tile.temperature = temperature;

                    if(useEvelation)
                        tile.elevation = elevation;

                    if(useRainfall)
                        tile.rainfall = rainfall;

                    if(useSwampiness)
                        tile.swampiness = swampiness;

                    /*
                    if (updateImmediately)
                    {
                        WorldUpdater.RenderSingleTile(tileID, tile.biome.DrawMaterial, LayersSubMeshes["WorldLayer_Hills"], Layers["WorldLayer_Hills"]);
                        WorldUpdater.RenderSingleTile(tileID, tile.biome.DrawMaterial, LayersSubMeshes["WorldLayer_CurrentMapTile"], Layers["WorldLayer_CurrentMapTile"]);
                    }
                    */
                }
            }
        }

        public override void DoWindowContents(Rect inRect)
        {
            Text.Font = GameFont.Small;

            Rect mainScrollRect = new Rect(0, 0, inRect.width, inRect.height);
            Rect mainScrollVertRect = new Rect(0, 0, mainScrollRect.x, inRect.width);
            Widgets.BeginScrollView(mainScrollRect, ref mainScrollPosition, mainScrollVertRect);

            WidgetRow row = new WidgetRow(0, 0, UIDirection.RightThenDown);
            if(row.ButtonText(Translator.Translate("UpdateAllTiles"), Translator.Translate("UpdateAllTilesInfo")))
            {
                WorldUpdater.UpdateMap();
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

            Rect group1 = new Rect(0, 20, inRect.width / 2, inRect.height);
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
            }
            yButtonPos += 25;
            foreach (BiomeDef def in avaliableBiomes)
            {
                if(Widgets.ButtonText(new Rect(0, yButtonPos, 230, 20), def.label))
                {
                    selectedBiome = def;
                }
                yButtonPos += 22;
            }
            Widgets.EndScrollView();

            /*
            yButtonPos = 270;
            if (Widgets.ButtonText(new Rect(0, yButtonPos, 150, 20), "Edit material"))
            {
                materialEditor.Show();
            }
            */

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
            Widgets.Label(new Rect(0, yButtonPos, 100, 20), Translator.Translate("Temperature"));
            fieldValue = Widgets.TextField(new Rect(100, yButtonPos, 100, 20), fieldValue);
            if(int.TryParse(fieldValue, out int temperature))
            {
                this.temperature = temperature;
            }
            if(Widgets.RadioButtonLabeled(new Rect(200, yButtonPos, 50, 20), "", useTemperature == true))
            {
                useTemperature = !useTemperature;
            }

            yButtonPos += 25;
            Widgets.Label(new Rect(0, yButtonPos, 100, 20), Translator.Translate("Evelation"));
            fieldElevationValue = Widgets.TextField(new Rect(100, yButtonPos, 100, 20), fieldElevationValue);
            if (float.TryParse(fieldElevationValue, out float evel))
            {
                this.elevation = evel;
            }
            if (Widgets.RadioButtonLabeled(new Rect(200, yButtonPos, 50, 20), "", useEvelation == true))
            {
                useEvelation = !useEvelation;
            }

            yButtonPos += 25;
            Widgets.Label(new Rect(0, yButtonPos, 100, 20), Translator.Translate("Rainfall"));
            fieldRainfallValue = Widgets.TextField(new Rect(100, yButtonPos, 100, 20), fieldRainfallValue);
            if (float.TryParse(fieldRainfallValue, out float rain))
            {
                this.rainfall = rain;
            }
            if (Widgets.RadioButtonLabeled(new Rect(200, yButtonPos, 50, 20), "", useRainfall == true))
            {
                useRainfall = !useRainfall;
            }

            yButtonPos += 25;
            Widgets.Label(new Rect(0, yButtonPos, 100, 20), Translator.Translate("Swampiness"));
            fieldSwampinessValue = Widgets.TextField(new Rect(100, yButtonPos, 100, 20), fieldSwampinessValue);
            if (float.TryParse(fieldSwampinessValue, out float swa))
            {
                this.swampiness = swa;
            }
            if (Widgets.RadioButtonLabeled(new Rect(200, yButtonPos, 50, 20), "", useSwampiness == true))
            {
                useSwampiness = !useSwampiness;
            }


            GUI.EndGroup();

            float group2Width = 270;
            Rect group2 = new Rect(group2Width, 20, inRect.width / 2, inRect.height);
            GUI.BeginGroup(group2);
            yButtonPos  = 25;
            if (Widgets.ButtonText(new Rect(0, yButtonPos, 190, 20), Translator.Translate("FactionEditor")))
            {
                factionEditor.Show();
            }

            yButtonPos += 35;
            if (Widgets.ButtonText(new Rect(0, yButtonPos, 190, 20), Translator.Translate("RoadAndRiverEditor")))
            {
                roadEditor.Show();
            }

            GUI.EndGroup();

            Widgets.EndScrollView();
        }

        internal static object GetInstanceField(Type type, object instance, string fieldName)
        {
            BindingFlags bindFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
                | BindingFlags.Static;
            FieldInfo field = type.GetField(fieldName, bindFlags);
            return field.GetValue(instance);
        }

        internal static object GetInstanceProperty(Type type, object instance, string propertyName)
        {
            BindingFlags bindFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
                | BindingFlags.Static;
            PropertyInfo property = type.GetProperty(propertyName, bindFlags);
            return property.GetValue(instance, null);
        }
    }
}
