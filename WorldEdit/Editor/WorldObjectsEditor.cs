using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using Verse;
using WorldEdit.Editor.WorldObjectsMenu;
using WorldEdit.Interfaces;

namespace WorldEdit.Editor
{
    public sealed class WorldObjectsEditor : FWindow
    {
        public override Vector2 InitialSize => new Vector2(630, 460);
        private Vector2 scrollPosition = Vector2.zero;
        private Vector2 scrollPositionWb = Vector2.zero;

        /// <summary>
        /// Выбарнная надпись
        /// </summary>
        private WorldFeature selectedFeature = null;

        private WorldObject selectedObject = null;
        private List<WorldObject> allObjects => Find.WorldObjects.AllWorldObjects.Where(o => o.def != WorldObjectDefOf.Settlement).ToList();

        public WorldObjectsEditor()
        {
            resizeable = false;
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
                    Messages.Message($"Selected feature: {selectedFeature.name}", MessageTypeDefOf.NeutralEvent, false); 
                }
                x += 22;
            }
            Widgets.EndScrollView();

            if (Widgets.ButtonText(new Rect(0, 340, 300, 20), Translator.Translate("CreateNewFeature")))
            {
                Find.WindowStack.Add(new WorldFeatureCreator());
            }

            if (Widgets.ButtonText(new Rect(0, 365, 300, 20), Translator.Translate("DeleteFeature")))
            {
                DeleteFeature();
            }

            if (Widgets.ButtonText(new Rect(0, 390, 300, 20), Translator.Translate("RemoveAllFeatures")))
            {
                RemoveAllFeatures();
            }

            if (Widgets.ButtonText(new Rect(0, 415, 300, 20), Translator.Translate("EditFeature")))
            {
                EditFeature();
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
                if (Widgets.ButtonText(new Rect(0, x, 290, 20), $"[{obj.Tile}] {obj.LabelCap}"))
                {
                    selectedObject = obj;
                    Find.WorldCameraDriver.JumpTo(obj.Tile);
                    Messages.Message($"Selected object: {selectedObject.LabelCap}", MessageTypeDefOf.NeutralEvent, false);
                }
                x += 22;
            }
            Widgets.EndScrollView();

            if (Widgets.ButtonText(new Rect(310, 340, 300, 20), Translator.Translate("CreateNewObject")))
            {
                Find.WindowStack.Add(new WorldObjectsCreator());
            }

            if (Widgets.ButtonText(new Rect(310, 365, 300, 20), Translator.Translate("DeleteObject")))
            {
                DeleteWorldObject();
            }

            if (Widgets.ButtonText(new Rect(310, 390, 300, 20), Translator.Translate("RemoveAllObjects")))
            {
                RemoveAllObjects();
            }

            if (Widgets.ButtonText(new Rect(310, 415, 300, 20), Translator.Translate("EditObject")))
            {
                EditObject();
            }
        }

        private void DeleteWorldObject()
        {
            if (selectedObject == null)
            {
                Messages.Message($"Select object", MessageTypeDefOf.NeutralEvent, false);
                return;
            }

            Find.WorldObjects.Remove(selectedObject);
        }

        private void RemoveAllObjects()
        {
            for(int i = 0; i < allObjects.Count; i++)
            {
                Find.WorldObjects.Remove(allObjects[i]);
            }

            Messages.Message($"All objects has been removed", MessageTypeDefOf.NeutralEvent, false);
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

            Messages.Message($"All features has been removed", MessageTypeDefOf.NeutralEvent, false);
        }

        private void DeleteFeature()
        {
            if (selectedFeature == null)
            {
                Messages.Message($"Select feature to delete", MessageTypeDefOf.NeutralEvent, false);
                return;
            }

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

        private void EditFeature()
        {
            if (selectedFeature == null)
            {
                Messages.Message($"Select feature to edit", MessageTypeDefOf.NeutralEvent, false);
                return;
            }

            Find.WindowStack.Add(new WorldFeatureCreator(selectedFeature));
        }

        private void EditObject()
        {
            if (selectedObject == null)
            {
                Messages.Message($"Select feature to edit", MessageTypeDefOf.NeutralEvent, false);
                return;
            }

            if(selectedObject.def == WorldObjectDefOf.AbandonedSettlement ||
                selectedObject.def == WorldObjectDefOf.DestroyedSettlement)
            {
                Find.WindowStack.Add(new AbandonedSettlementMenu(selectedObject));

                return;
            }

            if(selectedObject.def == WorldObjectDefOf.EscapeShip)
            {
                Find.WindowStack.Add(new EscapeShipMenu(selectedObject));

                return;
            }

            var core = ((Site)selectedObject).parts[0];

            if (core.def == SitePartDefOf.PreciousLump)
            {
                Find.WindowStack.Add(new PreciousLumpMenu((Site)selectedObject));
            //}else if(core.def == SitePartDefOf.ItemStash)
            //{
            //    Find.WindowStack.Add(new StashMenu((Site)selectedObject));
            //}else if(core.def == SitePartDefOf.DownedRefugee)
            //{
            //    Find.WindowStack.Add(new DownedRefugeeMenu((Site)selectedObject));
            //}else if (core.def == SitePartDefOf.PrisonerWillingToJoin)
            //{
            //    Find.WindowStack.Add(new PrisonerWillingToJoinMenu((Site)selectedObject));
            }
            else
            {
                Find.WindowStack.Add(new SingleObjectEditor((Site)selectedObject));
            }
        }
    }
}
