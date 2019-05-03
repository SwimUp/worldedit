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
    internal class WorldFeatureCreator : FWindow
    {
        public override Vector2 InitialSize => new Vector2(350, 150);

        /// <summary>
        /// Имя надписи
        /// </summary>
        private string featureName = string.Empty;

        /// <summary>
        /// Поворот
        /// </summary>
        private float rotate = 0f;
        private string rotateBuff = string.Empty;

        /// <summary>
        /// Размер
        /// </summary>
        private float maxLength = 10f;
        private string maxLengthBuff = "1";

        private WorldFeature editFeature = null;
        private bool edit = false;

        public WorldFeatureCreator()
        {
            resizeable = false;
            edit = false;
        }

        public WorldFeatureCreator(WorldFeature feat)
        {
            resizeable = false;

            editFeature = feat;
            edit = true;

            featureName = editFeature.name;
            rotate = editFeature.drawAngle;
            rotateBuff = $"{editFeature.drawAngle}";

            maxLength = editFeature.maxDrawSizeInTiles;
            maxLengthBuff = $"{editFeature.maxDrawSizeInTiles}";
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

            if (Widgets.ButtonText(new Rect(0, 110, 345, 20), Translator.Translate("CreateNewWorldPrint")))
            {
                CreateNew();
            }
        }

        private void CreateNew()
        {
            if (featureName == null)
            {
                Messages.Message($"Enter correct feature name", MessageTypeDefOf.NeutralEvent);
                return;
            }

            if (maxLength <= 0)
            {
                Messages.Message($"Enter correct size", MessageTypeDefOf.NeutralEvent);
                return;
            }

            if (edit)
            {
                editFeature.name = featureName;
                editFeature.drawAngle = rotate;
                editFeature.maxDrawSizeInTiles = maxLength;

                Find.WorldFeatures.textsCreated = false;
                Find.WorldFeatures.UpdateFeatures();

                Messages.Message($"Success", MessageTypeDefOf.NeutralEvent);

                return;
            }

            int tile = Find.WorldSelector.selectedTile;

            if (tile < 0)
            {
                Messages.Message($"Select tile", MessageTypeDefOf.NeutralEvent);
                return;
            }

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

            Messages.Message($"Feature created", MessageTypeDefOf.NeutralEvent);
        }
    }
}
