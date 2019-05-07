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
    public class Generator_Roads_Vanilla : Generator
    {
        public override GeneratorMode Mode => GeneratorMode.Roads;

        public override GeneratorType Type => GeneratorType.Vanilla;

        public override string Description => Translator.Translate($"{GetType().Name}");

        public override string Title => Translator.Translate($"{GetType().Name}_title");

        private struct Link
        {
            public float distance;

            public int indexA;

            public int indexB;
        }

        private class Connectedness
        {
            public Connectedness parent;

            public Connectedness Group()
            {
                if (parent == null)
                {
                    return this;
                }
                return parent.Group();
            }
        }

        private FloatRange ExtraRoadNodesPer100kTiles = new FloatRange(30f, 50f);

        private IntRange RoadDistanceFromSettlement = new IntRange(-4, 4);

        public string Seed = "ZULUL";

        public override void RunGenerator()
        {
            LongEventHandler.QueueLongEvent(delegate
            {
                ClearRoads();

                GenerateRoadEndpoints();
                Rand.PushState();
                Rand.Seed = GenText.StableStringHash(Seed);
                GenerateRoadNetwork();
                Rand.PopState();

                WorldEditor.WorldUpdater.UpdateLayer(WorldEditor.Editor.Layers["WorldLayer_Roads"]);

            }, "Generating roads...", doAsynchronously: false, null);
        }

        private void ClearRoads()
        {
            for (int i = 0; i < Find.WorldGrid.TilesCount; i++)
            {
                Tile tile = Find.WorldGrid[i];

                tile.potentialRoads = null;
            }
        }

        private void GenerateRoadEndpoints()
        {
            List<int> list = (from wo in Find.WorldObjects.AllWorldObjects
                              where Rand.Value > 0.05f
                              select wo.Tile).ToList();
            int num = GenMath.RoundRandom((float)Find.WorldGrid.TilesCount / 100000f * ExtraRoadNodesPer100kTiles.RandomInRange);
            for (int i = 0; i < num; i++)
            {
                list.Add(TileFinder.RandomSettlementTileFor(null));
            }
            List<int> list2 = new List<int>();
            for (int j = 0; j < list.Count; j++)
            {
                int num2 = Mathf.Max(0, RoadDistanceFromSettlement.RandomInRange);
                int num3 = list[j];
                for (int k = 0; k < num2; k++)
                {
                    Find.WorldGrid.GetTileNeighbors(num3, list2);
                    num3 = list2.RandomElement();
                }
                if (Find.WorldReachability.CanReach(list[j], num3))
                {
                    list[j] = num3;
                }
            }
            list = list.Distinct().ToList();
            Find.World.genData.roadNodes = list;
        }

        private void GenerateRoadNetwork()
        {
            Find.WorldPathGrid.RecalculateAllPerceivedPathCosts(0);
            List<Link> linkProspective = GenerateProspectiveLinks(Find.World.genData.roadNodes);
            List<Link> linkFinal = GenerateFinalLinks(linkProspective, Find.World.genData.roadNodes.Count);
            DrawLinksOnWorld(linkFinal, Find.World.genData.roadNodes);
        }

        private List<Link> GenerateProspectiveLinks(List<int> indexToTile)
        {
            Dictionary<int, int> tileToIndexLookup = new Dictionary<int, int>();
            for (int i = 0; i < indexToTile.Count; i++)
            {
                tileToIndexLookup[indexToTile[i]] = i;
            }
            List<Link> linkProspective = new List<Link>();
            List<int> list = new List<int>();
            int srcIndex;
            for (srcIndex = 0; srcIndex < indexToTile.Count; srcIndex++)
            {
                int srcTile = indexToTile[srcIndex];
                list.Clear();
                list.Add(srcTile);
                int found = 0;
                Find.WorldPathFinder.FloodPathsWithCost(list, (int src, int dst) => Caravan_PathFollower.CostToMove(3300, src, dst, null, perceivedStatic: true), null, delegate (int tile, float distance)
                {
                    if (tile != srcTile && tileToIndexLookup.ContainsKey(tile))
                    {
                        found++;
                        linkProspective.Add(new Link
                        {
                            distance = distance,
                            indexA = srcIndex,
                            indexB = tileToIndexLookup[tile]
                        });
                    }
                    return found >= 8;
                });
            }
            linkProspective.Sort((Link lhs, Link rhs) => lhs.distance.CompareTo(rhs.distance));
            return linkProspective;
        }

        private List<Link> GenerateFinalLinks(List<Link> linkProspective, int endpointCount)
        {
            List<Connectedness> list = new List<Connectedness>();
            for (int i = 0; i < endpointCount; i++)
            {
                list.Add(new Connectedness());
            }
            List<Link> list2 = new List<Link>();
            for (int j = 0; j < linkProspective.Count; j++)
            {
                Link prospective = linkProspective[j];
                if (list[prospective.indexA].Group() != list[prospective.indexB].Group() || (!(Rand.Value > 0.015f) && !list2.Any((Link link) => link.indexB == prospective.indexA && link.indexA == prospective.indexB)))
                {
                    if (Rand.Value > 0.1f)
                    {
                        list2.Add(prospective);
                    }
                    if (list[prospective.indexA].Group() != list[prospective.indexB].Group())
                    {
                        Connectedness parent = new Connectedness();
                        list[prospective.indexA].Group().parent = parent;
                        list[prospective.indexB].Group().parent = parent;
                    }
                }
            }
            return list2;
        }

        private void DrawLinksOnWorld(List<Link> linkFinal, List<int> indexToTile)
        {
            foreach (Link item in linkFinal)
            {
                Link current = item;
                WorldPath worldPath = Find.WorldPathFinder.FindPath(indexToTile[current.indexA], indexToTile[current.indexB], null);
                List<int> nodesReversed = worldPath.NodesReversed;
                RoadDef roadDef = (from rd in DefDatabase<RoadDef>.AllDefsListForReading
                                   where !rd.ancientOnly
                                   select rd).RandomElementWithFallback();
                for (int i = 0; i < nodesReversed.Count - 1; i++)
                {
                    Find.WorldGrid.OverlayRoad(nodesReversed[i], nodesReversed[i + 1], roadDef);
                }
                worldPath.ReleaseToPool();
            }
        }
    }
}
