using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using Verse;
using WorldEdit.Interfaces;

namespace WorldEdit.Editor
{
    public sealed class WorldObjectsEditor : FWindow
    {
        public override Vector2 InitialSize => new Vector2(630, 450);
        private Vector2 scrollPosition = Vector2.zero;
        private Vector2 scrollPositionWb = Vector2.zero;

        /// <summary>
        /// Выбарнная надпись
        /// </summary>
        private WorldFeature selectedFeature = null;
        /// <summary>
        /// Редактор надписей
        /// </summary>
        private WorldFeatureCreator featureCreator;


        private WorldObjectsCreator objectsCreator = null;
        private WorldObject selectedObject = null;
        private List<WorldObject> allObjects => Find.WorldObjects.AllWorldObjects.Where(o => o.def != WorldObjectDefOf.Settlement).ToList();

        public WorldObjectsEditor()
        {
            resizeable = false;
            featureCreator = new WorldFeatureCreator();
            objectsCreator = new WorldObjectsCreator();
        }

        public override void DoWindowContents(Rect inRect)
        {
            Text.Font = GameFont.Small;

            WorldPrintsMenu();
            WorldObjectsMenu();
        }

        private void WorldPrintsMenu()
        {
            Widgets.Label(new Rect(0, 0, 300, 20), Translator.Translate("WorldPrintsTitle"));

            int size1 = Find.WorldFeatures.features.Count * 30;
            Rect scrollRectFact = new Rect(0, 50, 300, 280);
            Rect scrollVertRectFact = new Rect(0, 0, scrollRectFact.x, size1);
            Widgets.BeginScrollView(scrollRectFact, ref scrollPosition, scrollVertRectFact);
            int x = 0;
            foreach (var feat in Find.WorldFeatures.features)
            {
                if (Widgets.ButtonText(new Rect(0, x, 290, 20), feat.name))
                {
                    selectedFeature = feat;
                }
                x += 22;
            }
            Widgets.EndScrollView();

            if (Widgets.ButtonText(new Rect(0, 340, 300, 20), Translator.Translate("CreateNewFeature")))
            {
                featureCreator.Show();
            }

            if (Widgets.ButtonText(new Rect(0, 365, 300, 20), Translator.Translate("DeleteFeature")))
            {
                DeleteFeature();
            }

            if (Widgets.ButtonText(new Rect(0, 390, 300, 20), Translator.Translate("RemoveAllFeatures")))
            {
                RemoveAllFeatures();
            }
        }

        private void WorldObjectsMenu()
        {
            Widgets.Label(new Rect(310, 0, 300, 20), Translator.Translate("WorldObjectsTitle"));

            int size = allObjects.Count * 30;
            Rect scrollRectFact = new Rect(310, 50, 300, 280);
            Rect scrollVertRectFact = new Rect(0, 0, scrollRectFact.x, size);
            Widgets.BeginScrollView(scrollRectFact, ref scrollPositionWb, scrollVertRectFact);
            int x = 0;
            foreach (var obj in allObjects)
            {
                if (Widgets.ButtonText(new Rect(0, x, 290, 20), $"[{obj.Tile}] {obj.def.defName}"))
                {
                    selectedObject = obj;
                }
                x += 22;
            }
            Widgets.EndScrollView();

            if (Widgets.ButtonText(new Rect(310, 340, 300, 20), Translator.Translate("CreateNewObject")))
            {
                objectsCreator.Show();
            }

            if (Widgets.ButtonText(new Rect(310, 365, 300, 20), Translator.Translate("DeleteObject")))
            {
                DeleteWorldObject();
            }

            if (Widgets.ButtonText(new Rect(310, 390, 300, 20), Translator.Translate("RemoveAllObjects")))
            {
                RemoveAllObjects();
            }
        }

        private void DeleteWorldObject()
        {
            if (selectedObject == null)
                return;

            Find.WorldObjects.Remove(selectedObject);
        }

        private void RemoveAllObjects()
        {
            for(int i = 0; i < allObjects.Count; i++)
            {
                Find.WorldObjects.Remove(allObjects[i]);
            }
        }

        private void RemoveAllFeatures()
        {
            WorldGrid grid = Find.WorldGrid;

            foreach(var feature in Find.WorldFeatures.features)
            {
                foreach(var tileID in feature.Tiles)
                {
                    grid[tileID].feature = null;
                }
            }

            Find.WorldFeatures.features.Clear();

            Find.WorldFeatures.textsCreated = false;

            Find.WorldFeatures.UpdateFeatures();
        }

        private void DeleteFeature()
        {
            if (selectedFeature == null)
                return;

            WorldGrid grid = Find.WorldGrid;
            foreach(var t in selectedFeature.Tiles)
            {
                if (grid[t].feature == selectedFeature)
                    grid[t].feature = null;
            }

            Find.WorldFeatures.features.Remove(selectedFeature);

            Find.WorldFeatures.textsCreated = false;
            Find.WorldFeatures.UpdateFeatures();

            selectedFeature = null;
        }
    }
}
