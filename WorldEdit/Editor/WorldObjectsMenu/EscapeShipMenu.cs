using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using WorldEdit.Interfaces;

namespace WorldEdit.Editor.WorldObjectsMenu
{
    internal class EscapeShipMenu : FWindow
    {
        public override Vector2 InitialSize => new Vector2(300, 320);

        private string time = "4";

        public EscapeShipMenu()
        {
            resizeable = false;
        }
        public override void DoWindowContents(Rect inRect)
        {
            Text.Font = GameFont.Small;

            Widgets.Label(new Rect(0, 0, 240, 20), Translator.Translate("AddEscapeShipPod"));

            Widgets.Label(new Rect(0, 30, 240, 20), Translator.Translate("DurationTimeDays"));
            time = Widgets.TextField(new Rect(0, 55, 240, 20), time);

            Widgets.Label(new Rect(0, 90, 240, 20), $"Selected tile ID: {Find.WorldSelector.selectedTile}");

            if (Widgets.ButtonText(new Rect(0, 275, 290, 20), Translator.Translate("AddEscapeShipPoint")))
            {
                AddEscapeShip();
            }
        }

        private void AddEscapeShip()
        {
            if (Find.WorldSelector.selectedTile == -1)
                return;

            if (!float.TryParse(time, out float timeFloat))
                return;

            WorldObject worldObject = WorldObjectMaker.MakeWorldObject(WorldObjectDefOf.EscapeShip);
            worldObject.Tile = Find.WorldSelector.selectedTile;
            worldObject.GetComponent<EnterCooldownComp>().Start(timeFloat);
            Find.WorldObjects.Add(worldObject);
        }
    }

}
