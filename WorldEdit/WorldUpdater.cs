﻿using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using Verse;

namespace WorldEdit
{
    internal class WorldUpdater
    {
        private readonly FloatRange BaseSizeRange = new FloatRange(0.9f, 1.1f);
        private readonly IntVec2 TexturesInAtlas = new IntVec2(2, 2);
        private readonly FloatRange BasePosOffsetRange_SmallHills = new FloatRange(0f, 0.37f);
        private readonly FloatRange BasePosOffsetRange_LargeHills = new FloatRange(0f, 0.2f);
        private readonly FloatRange BasePosOffsetRange_Mountains = new FloatRange(0f, 0.08f);
        private readonly FloatRange BasePosOffsetRange_ImpassableMountains = new FloatRange(0f, 0.08f);

        public void UpdateMap()
        {
            Find.World.renderer.RegenerateAllLayersNow();
        }

        public void UpdateLayer(WorldLayer layer)
        {
            Log.Message($"LAYER: {layer.GetType().Name}");
            layer.RegenerateNow();
        }

        public void RenderSingleTile(int tileID, Material drawMaterial, List<LayerSubMesh> subMeshes, WorldLayer layer)
        {
            LayerSubMesh subMesh = GetSubMesh(drawMaterial, subMeshes);
            
            List<Vector3> verts = new List<Vector3>();
            Find.WorldGrid.GetTileVertices(tileID, verts);

            int count = subMesh.verts.Count;
            int i = 0;
            for (int count2 = verts.Count; i < count2; i++)
            {
                subMesh.verts.Add(verts[i] + verts[i].normalized * 0.012f);
                subMesh.uvs.Add((GenGeo.RegularPolygonVertexPosition(count2, i) + Vector2.one) / 2f);
                if (i < count2 - 2)
                {
                    subMesh.tris.Add(count + i + 2);
                    subMesh.tris.Add(count + i + 1);
                    subMesh.tris.Add(count);
                }
            }
            FinalizeMesh(MeshParts.All, subMesh);
            
        }
        internal void FinalizeMesh(MeshParts tags, List<LayerSubMesh> subMeshes)
        {
            for (int i = 0; i < subMeshes.Count; i++)
            {
                if (subMeshes[i].verts.Count > 0)
                {
                    subMeshes[i].FinalizeMesh(tags);
                }
            }
        }

        internal void FinalizeMesh(MeshParts tags, LayerSubMesh subMesh)
        {
            if (subMesh.verts.Count > 0)
            {
                subMesh.FinalizeMesh(tags);
            }
        }

        protected LayerSubMesh GetSubMesh(Material material, List<LayerSubMesh> subMeshes)
        {
            int subMeshIndex;
            return GetSubMesh(material, subMeshes, out subMeshIndex);
        }
        protected LayerSubMesh GetSubMesh(Material material, List<LayerSubMesh> subMeshes, out int subMeshIndex)
        {
            for (int i = 0; i < subMeshes.Count; i++)
            {
                LayerSubMesh layerSubMesh = subMeshes[i];
                if (layerSubMesh.material == material && layerSubMesh.verts.Count < 40000)
                {
                    subMeshIndex = i;
                    return layerSubMesh;
                }
            }

            Mesh mesh = new Mesh();
            LayerSubMesh layerSubMesh2 = new LayerSubMesh(mesh, material);
            subMeshIndex = subMeshes.Count;
            subMeshes.Add(layerSubMesh2);
            return layerSubMesh2;
        }

        public void RenderSingleHill(int tileID, List<LayerSubMesh> subMeshes)
        {
            WorldGrid grid = Find.WorldGrid;
            int tilesCount = grid.TilesCount;
            Tile tile = grid[tileID];
            Material material = WorldMaterials.SmallHills;
            FloatRange floatRange = BasePosOffsetRange_SmallHills;
            switch (tile.hilliness)
            {
                case Hilliness.SmallHills:
                    material = WorldMaterials.SmallHills;
                    floatRange = BasePosOffsetRange_SmallHills;
                    break;
                case Hilliness.LargeHills:
                    material = WorldMaterials.LargeHills;
                    floatRange = BasePosOffsetRange_LargeHills;
                    break;
                case Hilliness.Mountainous:
                    material = WorldMaterials.Mountains;
                    floatRange = BasePosOffsetRange_Mountains;
                    break;
                case Hilliness.Impassable:
                    material = WorldMaterials.ImpassableMountains;
                    floatRange = BasePosOffsetRange_ImpassableMountains;
                    break;
            }
            LayerSubMesh subMesh = GetSubMesh(material, subMeshes);
            Vector3 tileCenter = grid.GetTileCenter(tileID);
            Vector3 posForTangents = tileCenter;
            float magnitude = tileCenter.magnitude;
            tileCenter = (tileCenter + Rand.UnitVector3 * floatRange.RandomInRange * grid.averageTileSize).normalized * magnitude;
            WorldRendererUtility.PrintQuadTangentialToPlanet(tileCenter, posForTangents, BaseSizeRange.RandomInRange * grid.averageTileSize, 0.005f, subMesh, counterClockwise: false, randomizeRotation: true, printUVs: false);
            IntVec2 texturesInAtlas = TexturesInAtlas;
            int indexX = Rand.Range(0, texturesInAtlas.x);
            IntVec2 texturesInAtlas2 = TexturesInAtlas;
            int indexY = Rand.Range(0, texturesInAtlas2.z);
            IntVec2 texturesInAtlas3 = TexturesInAtlas;
            int x = texturesInAtlas3.x;
            IntVec2 texturesInAtlas4 = TexturesInAtlas;
            WorldRendererUtility.PrintTextureAtlasUVs(indexX, indexY, x, texturesInAtlas4.z, subMesh);

            FinalizeMesh(MeshParts.All, subMeshes);
        }

        private void ClearSubMeshes(MeshParts parts, List<LayerSubMesh> subMeshes)
        {
            for (int i = 0; i < subMeshes.Count; i++)
            {
                subMeshes[i].Clear(parts);
            }
        }
    }
}