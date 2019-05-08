using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Verse;

namespace WorldEdit.WorldGen.Generators
{
    public class Generator_Factions_Vanilla : Generator
    {
        public override string Description => Translator.Translate($"{GetType().Name}");

        public override string Title => Translator.Translate($"{GetType().Name}_title");

        public override GeneratorMode Mode => GeneratorMode.Faction;

        public override GeneratorType Type => GeneratorType.Vanilla;

        public FloatRange SettlementsPer100kTiles = new FloatRange(75f, 85f);

        public bool regenerate = true;

        public int FactionToGenerate = 6;

        public Generator_Factions_Vanilla()
        {
            Settings.AddParam(GetType().GetField("SettlementsPer100kTiles"), SettlementsPer100kTiles);
            Settings.AddParam(GetType().GetField("regenerate"), regenerate);
            Settings.AddParam(GetType().GetField("FactionToGenerate"), FactionToGenerate);
        }

        public override void RunGenerator()
        {
            Setup();

            LongEventHandler.QueueLongEvent(delegate
            {
                GenerateFactions();

            }, "Generating factions...", doAsynchronously: false, null);
        }

        private void RemoveAllFactions()
        {
            List<Settlement> settlements = new List<Settlement>(Find.WorldObjects.Settlements);

            foreach (var selectedFaction in Find.FactionManager.AllFactions)
                if (Find.WorldPawns.AllPawnsAliveOrDead.Contains(selectedFaction.leader))
                    Find.WorldPawns.RemoveAndDiscardPawnViaGC(selectedFaction.leader);

            foreach (var settlement in settlements)
            {
                Find.WorldObjects.Remove(settlement);
            }

            Faction[] factionsSave = new Faction[]
            {
                    Find.FactionManager.OfPlayer,
                    Find.FactionManager.OfMechanoids,
                    Find.FactionManager.OfInsects,
                    Find.FactionManager.OfAncients,
                    Find.FactionManager.OfAncientsHostile
            };

            Find.FactionManager.AllFactionsListForReading.Clear();

            foreach (var fac in factionsSave)
                Find.FactionManager.Add(fac);
        }

        private void GenerateFactions()
        {
            if (regenerate)
            {
                RemoveAllFactions();
            }

            for (int i = 0; i < FactionToGenerate; i++)
            {
                FactionDef facDef = (from fa in DefDatabase<FactionDef>.AllDefs
                                     where fa.canMakeRandomly && !fa.hidden && !fa.isPlayer select fa).RandomElement();
                Faction faction = NewGeneratedFaction(facDef);
                Find.FactionManager.Add(faction);
            }

            int num = GenMath.RoundRandom((float)Find.WorldGrid.TilesCount / 100000f * SettlementsPer100kTiles.RandomInRange);
            num -= Find.WorldObjects.Settlements.Count;
            for (int k = 0; k < num; k++)
            {
                Faction faction3 = (from x in Find.World.factionManager.AllFactionsListForReading
                                    where !x.def.isPlayer && !x.def.hidden
                                    select x).RandomElementByWeight((Faction x) => x.def.settlementGenerationWeight);
                Settlement settlement = (Settlement)WorldObjectMaker.MakeWorldObject(WorldObjectDefOf.Settlement);
                settlement.SetFaction(faction3);
                settlement.Tile = TileFinder.RandomSettlementTileFor(faction3);
                settlement.Name = SettlementNameGenerator.GenerateSettlementName(settlement);
                Find.WorldObjects.Add(settlement);
            }
        }

        private Faction NewGeneratedFaction(FactionDef facDef)
        {
            Faction faction = new Faction();
            faction.def = facDef;
            faction.loadID = Find.UniqueIDsManager.GetNextFactionID();
            faction.colorFromSpectrum = FactionGenerator.NewRandomColorFromSpectrum(faction);
            if (!facDef.isPlayer)
            {
                if (facDef.fixedName != null)
                {
                    faction.Name = facDef.fixedName;
                }
                else
                {
                    faction.Name = NameGenerator.GenerateName(facDef.factionNameMaker, from fac in Find.FactionManager.AllFactionsVisible
                                                                                       select fac.Name);
                }
            }
            faction.centralMelanin = Rand.Value;
            foreach (Faction item in Find.FactionManager.AllFactions)
            {
                faction.TryMakeInitialRelationsWith(item);
            }

            faction.GenerateNewLeader();

            if (!facDef.hidden && !facDef.isPlayer)
            {
                Settlement settlement = (Settlement)WorldObjectMaker.MakeWorldObject(WorldObjectDefOf.Settlement);
                settlement.SetFaction(faction);
                settlement.Tile = TileFinder.RandomSettlementTileFor(faction);
                settlement.Name = SettlementNameGenerator.GenerateSettlementName(settlement);
                Find.WorldObjects.Add(settlement);
            }

            return faction;
        }
    }
}
