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
        public override Vector2 InitialSize => new Vector2(450, 400);
        private Vector2 scrollPosition = Vector2.zero;
        private Vector2 riverScrollPosition = Vector2.zero;
        private Vector2 mainScrollPosition = Vector2.zero;

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

            Rect mainScrollRect = new Rect(0, 0, inRect.width, inRect.height);
            Rect mainScrollVertRect = new Rect(0, 0, mainScrollRect.x, inRect.width);
            Widgets.BeginScrollView(mainScrollRect, ref mainScrollPosition, mainScrollVertRect);

            Rect group1 = new Rect(0, 0, inRect.width, inRect.height);
            if(Widgets.ButtonText(new Rect(0, 0, 200, 20), Translator.Translate("DeleteSingleRoad")))
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

            int roadListLength = avaliableRoads.Count * 25;
            Rect roadScrollRect = new Rect(0, 80, 200, 200);
            Rect roadScrollVertRect = new Rect(0, 0, roadScrollRect.x, roadListLength);
            int yButtonPos = 0;
            Widgets.BeginScrollView(roadScrollRect, ref scrollPosition, roadScrollVertRect);
            foreach(var road in avaliableRoads)
            {
                if(Widgets.ButtonText(new Rect(0, yButtonPos, 200, 20), road.label))
                {
                    selectedRoad = road;
                }
                yButtonPos += 22;
            }
            Widgets.EndScrollView();
            yButtonPos = 295;
            Widgets.Label(new Rect(0, yButtonPos, 50, 20), Translator.Translate("StartTileLabel"));
            roadId1 = Widgets.TextField(new Rect(65, yButtonPos, 50, 20), roadId1);
            yButtonPos += 30;
            Widgets.Label(new Rect(0, yButtonPos, 50, 20), Translator.Translate("EndTileLabel"));
            roadId2 = Widgets.TextField(new Rect(65, yButtonPos, 50, 20), roadId2);
            yButtonPos += 30;
            if (Widgets.ButtonText(new Rect(0, yButtonPos, 200, 20), Translator.Translate("CreateRoadLabel")))
            {
                CreateRoad();
            }

            yButtonPos = 80;
            int riverListLength = avaliableRivers.Count * 25;
            Rect riverScrollRect = new Rect(210, 80, 200, 200);
            Rect riverScrollVertRect = new Rect(0, 0, riverScrollRect.x, riverListLength);
            Widgets.BeginScrollView(riverScrollRect, ref riverScrollPosition, riverScrollRect);
            foreach (var river in avaliableRivers)
            {
                if (Widgets.ButtonText(new Rect(210, yButtonPos, 200, 20), river.label))
                {
                    selectedRiver = river;
                }
                yButtonPos += 22;
            }
            Widgets.EndScrollView();
            yButtonPos = 295;
            Widgets.Label(new Rect(210, yButtonPos, 50, 20), Translator.Translate("StartTileLabel"));
            riverId1 = Widgets.TextField(new Rect(265, yButtonPos, 50, 20), riverId1);
            yButtonPos += 30;
            Widgets.Label(new Rect(210, yButtonPos, 50, 20), Translator.Translate("EndTileLabel"));
            riverId2 = Widgets.TextField(new Rect(265, yButtonPos, 50, 20), riverId2);
            yButtonPos += 30;
            if (Widgets.ButtonText(new Rect(210, yButtonPos, 200, 20), Translator.Translate("CreateRiverLael")))
            {
                CreateRiver();
            }
            yButtonPos += 30;
            Widgets.Label(new Rect(0, yButtonPos, 430, 20), $"Selected tile ID: {Find.WorldSelector.selectedTile}");

            Widgets.EndScrollView();

        }

        private void DeleteAllRoads()
        {
            for(int i = 0; i < Find.WorldGrid.TilesCount; i++)
            {
                Tile tile = Find.WorldGrid[i];

                 tile.potentialRoads = null;
            }

            worldUpdater.UpdateLayer(WorldEditor.Editor.Layers["WorldLayer_Roads"]);
        }

        private void DeleteAllRivers()
        {
            for (int i = 0; i < Find.WorldGrid.TilesCount; i++)
            {
                Tile tile = Find.WorldGrid[i];

                tile.potentialRivers = null;
            }

            worldUpdater.UpdateLayer(WorldEditor.Editor.Layers["WorldLayer_Rivers"]);
        }

        private void CreateRoad()
        {
            if (selectedRoad == null)
                return;

            if (!int.TryParse(roadId1, out int tile1ID) || !int.TryParse(roadId2, out int tile2ID))
                return;

            WorldGrid worldGrid = Find.WorldGrid;
            var path = Find.WorldPathFinder.FindPath(tile1ID, tile2ID, null);
            for (int i = 0; i < path.NodesLeftCount - 1; i++)
            {
                worldGrid.OverlayRoad(path.Peek(i), path.Peek(i + 1), selectedRoad);
            }

            worldUpdater.UpdateLayer(WorldEditor.Editor.Layers["WorldLayer_Roads"]);
        }
        private void CreateRiver()
        {
            if (selectedRiver == null)
                return;

            if (!int.TryParse(riverId1, out int tile1ID) || !int.TryParse(riverId2, out int tile2ID))
                return;

            WorldGrid worldGrid = Find.WorldGrid;
            var path = Find.WorldPathFinder.FindPath(tile1ID, tile2ID, null);
            for (int i = 0; i < path.NodesLeftCount - 1; i++)
            {
                worldGrid.OverlayRiver(path.Peek(i), path.Peek(i + 1), selectedRiver);
            }

            worldUpdater.UpdateLayer(WorldEditor.Editor.Layers["WorldLayer_Rivers"]);
        }
        private void DeleteRoad()
        {
            int tileID = Find.WorldSelector.selectedTile;
            if (tileID < 0)
                return;

            Tile tile = Find.WorldGrid[tileID];
            if (tile.potentialRoads == null || tile.potentialRoads.Count == 0)
                return;

            tile.potentialRoads = null;

            worldUpdater.UpdateLayer(WorldEditor.Editor.Layers["WorldLayer_Roads"]);
        }

        private void DeleteRangeRoad()
        {
            if (!int.TryParse(roadId1, out int tile1ID) || !int.TryParse(roadId2, out int tile2ID))
                return;

            WorldGrid worldGrid = Find.WorldGrid;
            var path = Find.WorldPathFinder.FindPath(tile1ID, tile2ID, null);
            for (int i = 0; i < path.NodesLeftCount - 1; i++)
            {
                Tile tile = worldGrid[path.Peek(i)];
                tile.potentialRoads = null;
            }

            worldUpdater.UpdateLayer(WorldEditor.Editor.Layers["WorldLayer_Roads"]);
        }

        private void DeleteRiver()
        {
            int tileID = Find.WorldSelector.selectedTile;
            if (tileID < 0)
                return;

            Tile tile = Find.WorldGrid[tileID];
            if (tile.potentialRivers == null || tile.potentialRivers.Count == 0)
                return;

            tile.potentialRivers = null;

            worldUpdater.UpdateLayer(WorldEditor.Editor.Layers["WorldLayer_Rivers"]);
        }

        private void DeleteRangeRivers()
        {
            if (!int.TryParse(riverId1, out int tile1ID) || !int.TryParse(riverId2, out int tile2ID))
                return;

            WorldGrid worldGrid = Find.WorldGrid;
            var path = Find.WorldPathFinder.FindPath(tile1ID, tile2ID, null);
            for (int i = 0; i < path.NodesLeftCount - 1; i++)
            {
                Tile tile = worldGrid[path.Peek(i)];
                tile.potentialRivers = null;
            }

            worldUpdater.UpdateLayer(WorldEditor.Editor.Layers["WorldLayer_Rivers"]);
        }
    }
}
