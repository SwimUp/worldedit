using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using WorldEdit.Interfaces;

namespace WorldEdit.Editor
{
    internal sealed class RoadAndRiversEditor : FWindow
    {
        public override Vector2 InitialSize => new Vector2(450, 500);
        private Vector2 scrollPosition = Vector2.zero;
        private Vector2 riverScrollPosition = Vector2.zero;
        private Vector2 mainScrollPosition = Vector2.zero;

        private bool PaintMode = false;

        /// <summary>
        /// Список доступных типов дорог
        /// </summary>
        private List<RoadDef> avaliableRoads;

        /// <summary>
        /// Выбранный тип дорог
        /// </summary>
        private RoadDef selectedRoad = null;

        /// <summary>
        /// Список доступных типов рек
        /// </summary>
        private List<RiverDef> avaliableRivers;

        /// <summary>
        /// Выбраннаый тип рек
        /// </summary>
        private RiverDef selectedRiver = null;

        private string roadId1 = string.Empty;
        private string roadId2 = string.Empty;
        private string riverId1 = string.Empty;
        private string riverId2 = string.Empty;

        private WorldUpdater worldUpdater;

        public RoadAndRiversEditor()
        {
            resizeable = false;
            worldUpdater = WorldEditor.WorldUpdater;

            avaliableRoads = DefDatabase<RoadDef>.AllDefs.ToList();
            avaliableRivers = DefDatabase<RiverDef>.AllDefs.ToList();
        }

        public override void DoWindowContents(Rect inRect)
        {
            Text.Font = GameFont.Small;

            Rect mainScrollRect = new Rect(0, 20, inRect.width, inRect.height);
            Rect mainScrollVertRect = new Rect(0, 0, mainScrollRect.x, inRect.width);
            Widgets.BeginScrollView(mainScrollRect, ref mainScrollPosition, mainScrollVertRect);

            Rect group1 = new Rect(0, 0, inRect.width, inRect.height);
            if (Widgets.ButtonText(new Rect(0, 0, 200, 20), Translator.Translate("DeleteSingleRoad")))
            {
                DeleteRoad();
            }
            if (Widgets.ButtonText(new Rect(0, 25, 200, 20), Translator.Translate("DeleteRangeRoads")))
            {
                DeleteRangeRoad();
            }
            if (Widgets.ButtonText(new Rect(210, 0, 200, 20), Translator.Translate("DeleteSingleRiver")))
            {
                DeleteRiver();
            }
            if (Widgets.ButtonText(new Rect(210, 25, 200, 20), Translator.Translate("DeleteRangeRivers")))
            {
                DeleteRangeRivers();
            }
            if (Widgets.ButtonText(new Rect(0, 50, 200, 20), Translator.Translate("DeleteAllRoads")))
            {
                DeleteAllRoads();
            }
            if (Widgets.ButtonText(new Rect(210, 50, 200, 20), Translator.Translate("DeleteAllRivers")))
            {
                DeleteAllRivers();
            }

            if (Widgets.ButtonText(new Rect(0, 80, 200, 20), Translator.Translate("NoText")))
            {
                selectedRoad = null;
                Messages.Message($"Selected road: None", MessageTypeDefOf.NeutralEvent, false);
            }
            int roadListLength = avaliableRoads.Count * 25;
            Rect roadScrollRect = new Rect(0, 110, 200, 170);
            Rect roadScrollVertRect = new Rect(0, 0, roadScrollRect.x, roadListLength);
            int yButtonPos = 0;
            Widgets.BeginScrollView(roadScrollRect, ref scrollPosition, roadScrollVertRect);
            foreach (var road in avaliableRoads)
            {
                if (Widgets.ButtonText(new Rect(0, yButtonPos, 200, 20), road.label))
                {
                    selectedRoad = road;
                    Messages.Message($"Selected road: {selectedRoad.LabelCap}", MessageTypeDefOf.NeutralEvent, false);

                    if (selectedRiver != null)
                        PaintMode = false;
                }
                yButtonPos += 22;
            }
            Widgets.EndScrollView();
            yButtonPos = 295;
            Widgets.Label(new Rect(0, yButtonPos, 50, 20), Translator.Translate("StartTileLabel"));
            roadId1 = Widgets.TextField(new Rect(65, yButtonPos, 130, 20), roadId1);
            yButtonPos += 30;
            Widgets.Label(new Rect(0, yButtonPos, 50, 20), Translator.Translate("EndTileLabel"));
            roadId2 = Widgets.TextField(new Rect(65, yButtonPos, 130, 20), roadId2);
            yButtonPos += 30;
            if (Widgets.ButtonText(new Rect(0, yButtonPos, 200, 20), Translator.Translate("CreateRoadLabel")))
            {
                CreateRoad();
            }

            if (Widgets.ButtonText(new Rect(210, 80, 200, 20), Translator.Translate("NoText")))
            {
                selectedRiver = null;
                Messages.Message($"Selected river: None", MessageTypeDefOf.NeutralEvent, false);
            }
            yButtonPos = 110;
            int riverListLength = avaliableRivers.Count * 25;
            Rect riverScrollRect = new Rect(210, 110, 200, 170);
            Rect riverScrollVertRect = new Rect(0, 0, riverScrollRect.x, riverListLength);
            Widgets.BeginScrollView(riverScrollRect, ref riverScrollPosition, riverScrollRect);
            foreach (var river in avaliableRivers)
            {
                if (Widgets.ButtonText(new Rect(210, yButtonPos, 200, 20), river.label))
                {
                    selectedRiver = river;
                    Messages.Message($"Selected river: {selectedRiver.LabelCap}", MessageTypeDefOf.NeutralEvent, false);

                    if (selectedRoad != null)
                        PaintMode = false;
                }
                yButtonPos += 22;
            }
            Widgets.EndScrollView();
            yButtonPos = 295;
            Widgets.Label(new Rect(210, yButtonPos, 50, 20), Translator.Translate("StartTileLabel"));
            riverId1 = Widgets.TextField(new Rect(265, yButtonPos, 130, 20), riverId1);
            yButtonPos += 30;
            Widgets.Label(new Rect(210, yButtonPos, 50, 20), Translator.Translate("EndTileLabel"));
            riverId2 = Widgets.TextField(new Rect(265, yButtonPos, 130, 20), riverId2);
            yButtonPos += 30;
            if (Widgets.ButtonText(new Rect(210, yButtonPos, 200, 20), Translator.Translate("CreateRiverLael")))
            {
                CreateRiver();
            }
            yButtonPos += 30;

            if (Widgets.ButtonText(new Rect(210, yButtonPos, 200, 20), Translator.Translate("SetSingleRiver")))
            {
                SetSingleRiver();
            }

            yButtonPos += 30;

            Widgets.Label(new Rect(0, yButtonPos, 200, 20), $"Selected tile ID: {Find.WorldSelector.selectedTile}");
            if (Widgets.RadioButtonLabeled(new Rect(210, yButtonPos, 200, 20), Translator.Translate("PaintMode"), PaintMode))
            {
                TurnPaintMode();
            }

            Widgets.EndScrollView();
        }

        private void SetSingleRiver()
        {
            if (selectedRiver == null)
            {
                Messages.Message($"Select river type", MessageTypeDefOf.NeutralEvent, false);
                return;
            }

            int tileID = Find.WorldSelector.selectedTile;

            if(tileID < 0)
            {
                Messages.Message($"Select tile", MessageTypeDefOf.NeutralEvent, false);
                return;
            }

            Find.WorldGrid.OverlayRiver(tileID, tileID, selectedRiver);
        }

        private void TurnPaintMode()
        {
            if (selectedRiver != null && selectedRoad != null)
            {
                Messages.Message("Choose one thing: roads or rivers", MessageTypeDefOf.NeutralEvent, false);
                return;
            }

            PaintMode = !PaintMode;

            Messages.Message("Paint mode active: click left mouse to select start point, second mouse to draw", MessageTypeDefOf.NeutralEvent, false);
        }

        public override void WindowUpdate()
        {
            if (PaintMode)
            {
                if (GenWorld.MouseTile() < 0)
                    return;

                if (selectedRiver != null)
                {
                    if (Input.GetKeyDown(KeyCode.Mouse0))
                    {
                        riverId1 = GenWorld.MouseTile().ToString();
                        Messages.Message($"Start river: {riverId1}, select second point", MessageTypeDefOf.NeutralEvent, false);
                    }

                    if (Input.GetKeyDown(KeyCode.Mouse1))
                    {
                        riverId2 = GenWorld.MouseTile().ToString();
                        Messages.Message($"End river: {riverId2}", MessageTypeDefOf.NeutralEvent, false);

                        TryPrintRiver();
                    }
                }

                if (selectedRoad != null)
                {
                    if (Input.GetKeyDown(KeyCode.Mouse0))
                    {
                        roadId1 = GenWorld.MouseTile().ToString();
                        Messages.Message($"Start road: {roadId1}, select second point", MessageTypeDefOf.NeutralEvent, false);
                    }

                    if (Input.GetKeyDown(KeyCode.Mouse1))
                    {
                        roadId2 = GenWorld.MouseTile().ToString();
                        Messages.Message($"End road: {roadId2}", MessageTypeDefOf.NeutralEvent, false);

                        TryPrintRoad();
                    }
                }
            }
        }

        private void TryPrintRiver()
        {
            if (!int.TryParse(riverId1, out int tile1ID) || !int.TryParse(riverId2, out int tile2ID))
            {
                Messages.Message($"Select correct tile Ids (not -1 and not null)", MessageTypeDefOf.NeutralEvent, false);
                return;
            }

            if (tile1ID < 0)
            {
                Messages.Message($"Start tile is -1", MessageTypeDefOf.NeutralEvent, false);
                return;
            }

            if (tile2ID < 0)
            {
                Messages.Message($"End tile is -1", MessageTypeDefOf.NeutralEvent, false);
                return;
            }

            PrintRiver(tile1ID, tile2ID);
        }

        private void TryPrintRoad()
        {
            if (!int.TryParse(roadId1, out int tile1ID) || !int.TryParse(roadId2, out int tile2ID))
            {
                Messages.Message($"Select correct tile Ids (not -1 and not null)", MessageTypeDefOf.NeutralEvent, false);
                return;
            }

            if (tile1ID < 0)
            {
                Messages.Message($"Start tile is -1", MessageTypeDefOf.NeutralEvent, false);
                return;
            }

            if (tile2ID < 0)
            {
                Messages.Message($"End tile is -1", MessageTypeDefOf.NeutralEvent, false);
                return;
            }

            PrintRoad(tile1ID, tile2ID);
        }


        private void DeleteAllRoads()
        {
            for (int i = 0; i < Find.WorldGrid.TilesCount; i++)
            {
                Tile tile = Find.WorldGrid[i];

                tile.potentialRoads = null;
            }

            worldUpdater.UpdateLayer(WorldEditor.Editor.Layers["WorldLayer_Roads"]);

            Messages.Message($"All roads has been removed", MessageTypeDefOf.NeutralEvent, false);
        }

        private void DeleteAllRivers()
        {
            for (int i = 0; i < Find.WorldGrid.TilesCount; i++)
            {
                Tile tile = Find.WorldGrid[i];

                tile.potentialRivers = null;
            }

            worldUpdater.UpdateLayer(WorldEditor.Editor.Layers["WorldLayer_Rivers"]);

            Messages.Message($"All rivers has been removed", MessageTypeDefOf.NeutralEvent, false);
        }

        private void CreateRoad()
        {
            if (selectedRoad == null)
            {
                Messages.Message($"Select road type", MessageTypeDefOf.NeutralEvent, false);
                return;
            }

            if (!int.TryParse(roadId1, out int tile1ID) || !int.TryParse(roadId2, out int tile2ID))
            {
                Messages.Message($"Enter correct tile Ids", MessageTypeDefOf.NeutralEvent, false);
                return;
            }

            if (tile1ID < 0)
            {
                Messages.Message($"Start tile is -1", MessageTypeDefOf.NeutralEvent, false);
                return;
            }

            if (tile2ID < 0)
            {
                Messages.Message($"End tile is -1", MessageTypeDefOf.NeutralEvent, false);
                return;
            }

            PrintRoad(tile1ID, tile2ID);
        }
        private void CreateRiver()
        {
            if (selectedRiver == null)
            {
                Messages.Message($"Select river type", MessageTypeDefOf.NeutralEvent, false);
                return;
            }

            if (!int.TryParse(riverId1, out int tile1ID) || !int.TryParse(riverId2, out int tile2ID))
            {
                Messages.Message($"Enter correct tile Ids", MessageTypeDefOf.NeutralEvent, false);
                return;
            }

            if (tile1ID < 0)
            {
                Messages.Message($"Start tile is -1", MessageTypeDefOf.NeutralEvent, false);
                return;
            }

            if (tile2ID < 0)
            {
                Messages.Message($"End tile is -1", MessageTypeDefOf.NeutralEvent, false);
                return;
            }

            PrintRiver(tile1ID, tile2ID);
        }
        private void DeleteRoad()
        {
            int tileID = Find.WorldSelector.selectedTile;
            if (tileID < 0)
            {
                Messages.Message($"Select tile", MessageTypeDefOf.NeutralEvent, false);
                return;
            }

            Tile tile = Find.WorldGrid[tileID];
            if (tile.potentialRoads == null || tile.potentialRoads.Count == 0)
            {
                Messages.Message($"No road(s) on selected tile", MessageTypeDefOf.NeutralEvent, false);
                return;
            }

            tile.potentialRoads = null;

            worldUpdater.UpdateLayer(WorldEditor.Editor.Layers["WorldLayer_Roads"]);

            Messages.Message($"Roads removed", MessageTypeDefOf.NeutralEvent, false);
        }

        private void DeleteRangeRoad()
        {
            if (!int.TryParse(roadId1, out int tile1ID) || !int.TryParse(roadId2, out int tile2ID))
            {
                Messages.Message($"Enter correct tile Ids", MessageTypeDefOf.NeutralEvent, false);
                return;
            }

            if (tile1ID < 0)
            {
                Messages.Message($"Start tile is -1", MessageTypeDefOf.NeutralEvent, false);
                return;
            }

            if (tile2ID < 0)
            {
                Messages.Message($"End tile is -1", MessageTypeDefOf.NeutralEvent, false);
                return;
            }

            DeleteRoad(tile1ID, tile2ID);
        }

        private void DeleteRiver()
        {
            int tileID = Find.WorldSelector.selectedTile;
            if (tileID < 0)
            {
                Messages.Message($"Select tile", MessageTypeDefOf.NeutralEvent, false);
                return;
            }

            Tile tile = Find.WorldGrid[tileID];
            if (tile.potentialRivers == null || tile.potentialRivers.Count == 0)
            {
                Messages.Message($"No river(s) on selected tile", MessageTypeDefOf.NeutralEvent, false);
                return;
            }

            tile.potentialRivers = null;

            worldUpdater.UpdateLayer(WorldEditor.Editor.Layers["WorldLayer_Rivers"]);

            Messages.Message($"Rivers removed", MessageTypeDefOf.NeutralEvent, false);
        }

        private void DeleteRangeRivers()
        {
            if (!int.TryParse(riverId1, out int tile1ID) || !int.TryParse(riverId2, out int tile2ID))
            {
                Messages.Message($"Enter correct tile Ids", MessageTypeDefOf.NeutralEvent, false);
                return;
            }

            if (tile1ID < 0)
            {
                Messages.Message($"Start tile is -1", MessageTypeDefOf.NeutralEvent, false);
                return;
            }

            if (tile2ID < 0)
            {
                Messages.Message($"End tile is -1", MessageTypeDefOf.NeutralEvent, false);
                return;
            }

            DeleteRoad(tile1ID, tile2ID);
        }

        private void DeleteRoad(int tile1ID, int tile2ID)
        {
            WorldGrid worldGrid = Find.WorldGrid;
            var path = Find.WorldPathFinder.FindPath(tile1ID, tile2ID, null);
            for (int i = 0; i < path.NodesLeftCount - 1; i++)
            {
                Tile tile = worldGrid[path.Peek(i)];
                tile.potentialRoads = null;
            }

            worldUpdater.UpdateLayer(WorldEditor.Editor.Layers["WorldLayer_Roads"]);

            Messages.Message($"Roads removed", MessageTypeDefOf.NeutralEvent, false);
        }

        private void DeleteRiver(int tile1ID, int tile2ID)
        {
            WorldGrid worldGrid = Find.WorldGrid;
            var path = Find.WorldPathFinder.FindPath(tile1ID, tile2ID, null);
            for (int i = 0; i < path.NodesLeftCount - 1; i++)
            {
                Tile tile = worldGrid[path.Peek(i)];
                tile.potentialRivers = null;
            }

            worldUpdater.UpdateLayer(WorldEditor.Editor.Layers["WorldLayer_Rivers"]);

            Messages.Message($"Rivers removed", MessageTypeDefOf.NeutralEvent, false);
        }

        private void PrintRoad(int tile1ID, int tile2ID)
        {
            WorldGrid worldGrid = Find.WorldGrid;
            var path = Find.WorldPathFinder.FindPath(tile1ID, tile2ID, null);
            for (int i = 0; i < path.NodesLeftCount - 1; i++)
            {
                worldGrid.OverlayRoad(path.Peek(i), path.Peek(i + 1), selectedRoad);
            }

            worldUpdater.UpdateLayer(WorldEditor.Editor.Layers["WorldLayer_Roads"]);

            Messages.Message($"Road created", MessageTypeDefOf.NeutralEvent, false);
        }

        private void PrintRiver(int tile1ID, int tile2ID)
        {
            WorldGrid worldGrid = Find.WorldGrid;
            var path = Find.WorldPathFinder.FindPath(tile1ID, tile2ID, null);

            Tile tile1 = Find.WorldGrid[tile1ID];
            if(tile1.biome == BiomeDefOf.Ocean || tile1.biome == BiomeDefOf.Lake)
            {
                path.NodesReversed.Add(tile1ID);
            }
            Tile tile2 = Find.WorldGrid[tile2ID];
            if (tile2.biome == BiomeDefOf.Ocean || tile1.biome == BiomeDefOf.Lake)
            {
                path.NodesReversed.Insert(0, tile2ID);
            }

            for (int i = 0; i < path.NodesLeftCount - 1; i++)
            {
                worldGrid.OverlayRiver(path.Peek(i), path.Peek(i + 1), selectedRiver);
            }
            
            /*
            List<int> outList = new List<int>();
            Find.WorldGrid.GetTileNeighbors(path.LastNode, outList);
            foreach (var t in outList)
            {
                Tile tile = Find.WorldGrid[t];

                if (tile.biome == BiomeDefOf.Ocean)
                {
                    worldGrid.OverlayRiver(path.LastNode, t, selectedRiver);
                    break;
                }
            }

            Find.WorldGrid.GetTileNeighbors(path.FirstNode, outList);
            foreach (var t in outList)
            {
                Tile tile = Find.WorldGrid[t];

                if (tile.biome == BiomeDefOf.Ocean)
                {
                    worldGrid.OverlayRiver(path.FirstNode, t, selectedRiver);
                    break;
                }
            }
            */

            worldUpdater.UpdateLayer(WorldEditor.Editor.Layers["WorldLayer_Rivers"]);

            Messages.Message($"River created", MessageTypeDefOf.NeutralEvent, false);
        }
    }
}
