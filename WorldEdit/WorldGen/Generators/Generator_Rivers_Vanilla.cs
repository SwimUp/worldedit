using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace WorldEdit.WorldGen.Generators
{
    public class Generator_Rivers_Vanilla : Generator
    {
        public override GeneratorMode Mode => GeneratorMode.Rivers;

        public override GeneratorType Type => GeneratorType.Vanilla;

        public override string Description => Translator.Translate($"{GetType().Name}");

        public override string Title => Translator.Translate($"{GetType().Name}_title");

        public SimpleCurve ElevationChangeCost = new SimpleCurve
        {
            new CurvePoint(-1000f, 50f),
            new CurvePoint(-100f, 100f),
            new CurvePoint(0f, 400f),
            new CurvePoint(0f, 5000f),
            new CurvePoint(100f, 50000f),
            new CurvePoint(1000f, 50000f)
        };

        public bool DestroyRivers = true;

        public Generator_Rivers_Vanilla()
        {
            Settings.AddParam(GetType().GetField("DestroyRivers"), DestroyRivers);
            Settings.AddParam(GetType().GetField("ElevationChangeCost"), ElevationChangeCost);
        }

        public override void RunGenerator()
        {
            Setup();

            LongEventHandler.QueueLongEvent(delegate
            {
                if(DestroyRivers)
                    ClearRivers();

                GenerateRivers();

                WorldEditor.WorldUpdater.UpdateLayer(WorldEditor.Editor.Layers["WorldLayer_Rivers"]);

            }, "Generating rivers...", doAsynchronously: false, null);
        }

        private void ClearRivers()
        {
            for(int i = 0; i < Find.WorldGrid.TilesCount; i++)
            {
                Tile tile = Find.WorldGrid[i];

                tile.potentialRivers = null;
            }
        }

        private void GenerateRivers()
        {
            Find.WorldPathGrid.RecalculateAllPerceivedPathCosts();
            List<int> coastalWaterTiles = GetCoastalWaterTiles();
            if (coastalWaterTiles.Any())
            {
                List<int> neighbors = new List<int>();
                List<int>[] array = Find.WorldPathFinder.FloodPathsWithCostForTree(coastalWaterTiles, delegate (int st, int ed)
                {
                    Tile tile = Find.WorldGrid[ed];
                    Tile tile2 = Find.WorldGrid[st];
                    Find.WorldGrid.GetTileNeighbors(ed, neighbors);
                    int num = neighbors[0];
                    for (int j = 0; j < neighbors.Count; j++)
                    {
                        if (GetImpliedElevation(Find.WorldGrid[neighbors[j]]) < GetImpliedElevation(Find.WorldGrid[num]))
                        {
                            num = neighbors[j];
                        }
                    }
                    float num2 = 1f;
                    if (num != st)
                    {
                        num2 = 2f;
                    }
                    return Mathf.RoundToInt(num2 * ElevationChangeCost.Evaluate(GetImpliedElevation(tile2) - GetImpliedElevation(tile)));
                }, (int tid) => Find.WorldGrid[tid].WaterCovered);
                float[] flow = new float[array.Length];
                for (int i = 0; i < coastalWaterTiles.Count; i++)
                {
                    AccumulateFlow(flow, array, coastalWaterTiles[i]);
                    CreateRivers(flow, array, coastalWaterTiles[i]);
                }
            }
        }

        private float GetImpliedElevation(Tile tile)
        {
            float num = 0f;
            if (tile.hilliness == Hilliness.SmallHills)
            {
                num = 15f;
            }
            else if (tile.hilliness == Hilliness.LargeHills)
            {
                num = 250f;
            }
            else if (tile.hilliness == Hilliness.Mountainous)
            {
                num = 500f;
            }
            else if (tile.hilliness == Hilliness.Impassable)
            {
                num = 1000f;
            }
            return tile.elevation + num;
        }

        private List<int> GetCoastalWaterTiles()
        {
            List<int> list = new List<int>();
            List<int> list2 = new List<int>();
            for (int i = 0; i < Find.WorldGrid.TilesCount; i++)
            {
                Tile tile = Find.WorldGrid[i];
                if (tile.biome != BiomeDefOf.Ocean)
                {
                    continue;
                }
                Find.WorldGrid.GetTileNeighbors(i, list2);
                bool flag = false;
                for (int j = 0; j < list2.Count; j++)
                {
                    if (Find.WorldGrid[list2[j]].biome != BiomeDefOf.Ocean)
                    {
                        flag = true;
                        break;
                    }
                }
                if (flag)
                {
                    list.Add(i);
                }
            }
            return list;
        }

        private void AccumulateFlow(float[] flow, List<int>[] riverPaths, int index)
        {
            Tile tile = Find.WorldGrid[index];
            flow[index] += tile.rainfall;
            if (riverPaths[index] != null)
            {
                for (int i = 0; i < riverPaths[index].Count; i++)
                {
                    AccumulateFlow(flow, riverPaths, riverPaths[index][i]);
                    flow[index] += flow[riverPaths[index][i]];
                }
            }
            flow[index] = Mathf.Max(0f, flow[index] - CalculateTotalEvaporation(flow[index], tile.temperature));
        }

        private void CreateRivers(float[] flow, List<int>[] riverPaths, int index)
        {
            List<int> list = new List<int>();
            Find.WorldGrid.GetTileNeighbors(index, list);
            for (int i = 0; i < list.Count; i++)
            {
                float targetFlow = flow[list[i]];
                RiverDef riverDef = (from rd in DefDatabase<RiverDef>.AllDefs
                                     where rd.spawnFlowThreshold > 0 && (float)rd.spawnFlowThreshold <= targetFlow
                                     select rd).MaxByWithFallback((RiverDef rd) => rd.spawnFlowThreshold);
                if (riverDef != null && Rand.Value < riverDef.spawnChance)
                {
                    Find.WorldGrid.OverlayRiver(index, list[i], riverDef);
                    ExtendRiver(flow, riverPaths, list[i], riverDef);
                }
            }
        }

        private void ExtendRiver(float[] flow, List<int>[] riverPaths, int index, RiverDef incomingRiver)
        {
            if (riverPaths[index] != null)
            {
                int bestOutput = riverPaths[index].MaxBy((int ni) => flow[ni]);
                RiverDef riverDef = incomingRiver;
                while (riverDef != null && (float)riverDef.degradeThreshold > flow[bestOutput])
                {
                    riverDef = riverDef.degradeChild;
                }
                if (riverDef != null)
                {
                    Find.WorldGrid.OverlayRiver(index, bestOutput, riverDef);
                    ExtendRiver(flow, riverPaths, bestOutput, riverDef);
                }
                if (incomingRiver.branches != null)
                {
                    foreach (int alternateRiver in from ni in riverPaths[index]
                                                   where ni != bestOutput
                                                   select ni)
                    {
                        RiverDef.Branch branch2 = incomingRiver.branches.Where((RiverDef.Branch branch) => (float)branch.minFlow <= flow[alternateRiver]).MaxByWithFallback((RiverDef.Branch branch) => branch.minFlow);
                        if (branch2 != null && Rand.Value < branch2.chance)
                        {
                            Find.WorldGrid.OverlayRiver(index, alternateRiver, branch2.child);
                            ExtendRiver(flow, riverPaths, alternateRiver, branch2.child);
                        }
                    }
                }
            }
        }

        public float CalculateEvaporationConstant(float temperature)
        {
            float num = 0.61121f * Mathf.Exp((18.678f - temperature / 234.5f) * (temperature / (257.14f + temperature)));
            return num / (temperature + 273f);
        }

        public float CalculateRiverSurfaceArea(float flow)
        {
            return Mathf.Pow(flow, 0.5f);
        }

        public float CalculateEvaporativeArea(float flow)
        {
            return CalculateRiverSurfaceArea(flow);
        }

        public float CalculateTotalEvaporation(float flow, float temperature)
        {
            return CalculateEvaporationConstant(temperature) * CalculateEvaporativeArea(flow) * 250f;
        }
    }
}
