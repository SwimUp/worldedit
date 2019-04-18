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
    public class WorldObjectsEditor : EditWindow, IFWindow
    {
        private class CreateWorldFeature : EditWindow, IFWindow
        {
            public override Vector2 InitialSize => new Vector2(350, 150);

            private string featureName = string.Empty;

            private float rotate = 0f;
            private string rotateBuff = string.Empty;

            private float maxLength = 10f;
            private string maxLengthBuff = "1";

            private int leftRange = 0;
            private int rigthRange = 0;
            private string leftRangeBuff = string.Empty;
            private string rigthRangeBuff = string.Empty;

            public CreateWorldFeature()
            {
                resizeable = false;
            }

            public override void DoWindowContents(Rect inRect)
            {
                Text.Font = GameFont.Small;

                Widgets.Label(new Rect(0, 0, 330, 20), Translator.Translate("CreateNewFeature"));

                Widgets.Label(new Rect(0, 25, 120, 20), Translator.Translate("WorldFeatureName"));
                featureName = Widgets.TextField(new Rect(110, 25, 215, 20), featureName);

                Widgets.Label(new Rect(0, 50, 120, 20), Translator.Translate("RotateFeature"));
                Widgets.TextFieldNumeric(new Rect(110, 50, 215, 20), ref rotate, ref rotateBuff, 0, 360);

                Widgets.Label(new Rect(0, 75, 120, 20), Translator.Translate("FeatureLengthMax"));
                Widgets.TextFieldNumeric(new Rect(110, 75, 215, 20), ref maxLength, ref maxLengthBuff, 1f, 10000f);

                /*
                Widgets.Label(new Rect(120, 110, 230, 20), Translator.Translate("TileRange"));
                Widgets.Label(new Rect(90, 135, 60, 20), Translator.Translate("LeftRange"));
                Widgets.TextFieldNumeric(new Rect(0, 135, 165, 20), ref leftRange, ref leftRangeBuff, 0, 10000);
                Widgets.Label(new Rect(240, 135, 60, 20), Translator.Translate("RigthRange"));
                Widgets.TextFieldNumeric(new Rect(170, 135, 165, 20), ref rigthRange, ref rigthRangeBuff, 0, 10000);
                */

                if (Widgets.ButtonText(new Rect(0, 110, 345, 20), Translator.Translate("CreateNewWorldPrint")))
                {
                    CreateNew();
                }
            }

            private void CreateNew()
            {
                if (featureName == null)
                    return;

                if (maxLength <= 0)
                    return;

                int tile = Find.WorldSelector.selectedTile;

                if (tile < 0)
                    return;

                List<int> members = new List<int>();
                members.Add(tile);

                WorldFeature worldFeature = new WorldFeature
                {
                    uniqueID = Find.UniqueIDsManager.GetNextWorldFeatureID(),
                    def = DefDatabase<FeatureDef>.GetRandom(),
                    name = featureName
                };
                WorldGrid worldGrid = Find.WorldGrid;
                for (int i = 0; i < members.Count; i++)
                {
                    worldGrid[members[i]].feature = worldFeature;
                }
                worldFeature.drawCenter = worldGrid.GetTileCenter(tile);
                worldFeature.maxDrawSizeInTiles = maxLength;
                worldFeature.drawAngle = rotate;

                Find.WorldFeatures.features.Add(worldFeature);

                Find.WorldFeatures.textsCreated = false;
                Find.WorldFeatures.UpdateFeatures();
            }

            public void Show()
            {
                if (Find.WindowStack.IsOpen(typeof(CreateWorldFeature)))
                {
                    Log.Message("Currntly open...");
                }
                else
                {
                    Find.WindowStack.Add(this);
                }
            }
        }

        public override Vector2 InitialSize => new Vector2(800, 700);

        private Vector2 scrollPosition = Vector2.zero;

        private WorldFeature selectedFeature = null;
        private CreateWorldFeature featureCreator;

        public WorldObjectsEditor()
        {
            resizeable = false;
            featureCreator = new CreateWorldFeature();
        }

        public override void DoWindowContents(Rect inRect)
        {
            Text.Font = GameFont.Small;

            Widgets.Label(new Rect(0, 0, 300, 20), Translator.Translate("WorldPrintsTitle"));

            int size1 = Find.WorldFeatures.features.Count * 30;
            Rect scrollRectFact = new Rect(0, 50, 300, 290);
            Rect scrollVertRectFact = new Rect(0, 0, scrollRectFact.x, size1);
            Widgets.BeginScrollView(scrollRectFact, ref scrollPosition, scrollVertRectFact);
            int x = 0;
            foreach (var feat in Find.WorldFeatures.features)
            {
                if(Widgets.ButtonText(new Rect(0, x, 295, 20), feat.name))
                {
                    selectedFeature = feat;
                }
                x += 22;
            }
            Widgets.EndScrollView();

            if (Widgets.ButtonText(new Rect(0, 340, 300, 20), Translator.Translate("DeleteFeature")))
            {
                DeleteFeature();
            }

            if (Widgets.ButtonText(new Rect(0, 365, 300, 20), Translator.Translate("CreateNewFeature")))
            {
                featureCreator.Show();
            }
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

        public void Show()
        {
            if (Find.WindowStack.IsOpen(typeof(WorldObjectsEditor)))
            {
                Log.Message("Currntly open...");
            }
            else
            {
           
                Find.WindowStack.Add(this);
            }
        }
    }
}
