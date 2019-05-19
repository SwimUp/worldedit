using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using UnityEngine;
using Verse;

namespace WorldEdit
{
    internal class CustomFactions : GameComponent
    {
        //Имя поселения
        //Его текстура
        public static Dictionary<string, IconDef> CustomIcons = new Dictionary<string, IconDef>();

        public CustomFactions()
        {
        }

        public CustomFactions(Game game)
        {
        }

        public static IconDef GetIcon(string settlementName)
        {
            if (!CustomIcons.Keys.Contains(settlementName))
                return null;

            return CustomIcons[settlementName];
        }

        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_Collections.Look(ref CustomIcons, "customIcons", LookMode.Value, LookMode.Def);
        }
    }
}
