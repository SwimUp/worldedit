using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace WorldEdit
{
    internal class CustomRock : IExposable
    {
        public int Tile = 0;
        public List<ThingDef> Rocks = new List<ThingDef>();
        public bool Caves = false;

        public CustomRock()
        {
        }

        public CustomRock(int tile, List<ThingDef> things, bool caves)
        {
            Tile = tile;
            Rocks = new List<ThingDef>(things);
            Caves = caves;
        }

        public void SetRocksList(List<ThingDef> list) => Rocks = new List<ThingDef>(list);

        public void ExposeData()
        {
            Scribe_Values.Look(ref Tile, "tileID", -1);
            Scribe_Values.Look(ref Caves, "Caves", false);
            Scribe_Collections.Look(ref Rocks, "rocks", LookMode.Def);
        }
    }
    internal class CustomNaturalRocks : GameComponent
    {
        public static Dictionary<int, CustomRock> ResourceData = new Dictionary<int, CustomRock>();

        public CustomNaturalRocks()
        {
        }

        public CustomNaturalRocks(Game game)
        {
        }

        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_Collections.Look(ref ResourceData, "overrideNaturalRocksIds", LookMode.Value, LookMode.Deep);
        }
    }
}
