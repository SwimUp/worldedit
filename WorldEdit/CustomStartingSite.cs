using Harmony;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace WorldEdit
{
    internal sealed class CustomStartingSite : Page
    {
        public override Vector2 InitialSize => Vector2.zero;
        public override string PageTitle => "SelectStartingSite".TranslateWithBackup("SelectLandingSite");

        protected override float Margin => 0f;

        public static object EdbInstance = null;

        public CustomStartingSite()
        {
            WorldEditor.InitEditor();

            absorbInputAroundWindow = false;
            shadowAlpha = 0f;
            preventCameraMotion = false;

            var init = new GameInitData
            {
                playerFaction = Find.FactionManager.OfPlayer
            };

            Current.Game.InitData = init;
            Current.Game.Scenario.PreConfigure();

            ScenPart scenPart = Current.Game.Scenario.AllParts.Where(p => p is ScenPart_ConfigPage_ConfigureStartingPawns).FirstOrDefault();
            Current.Game.InitData.startingPawnCount = (scenPart as ScenPart_ConfigPage_ConfigureStartingPawns).pawnCount;

            Current.Game.InitData.startingAndOptionalPawns = WorldEditor.LoadedTemplate.StartPawns;
        }

        public override void DoWindowContents(Rect inRect)
        {
            
        }

        public override void ExtraOnGUI()
        {
         //   if (WorldEditor.isEdit)
         //       return;

            base.ExtraOnGUI();
            Text.Anchor = TextAnchor.UpperCenter;
            DrawPageTitle(new Rect(0f, 5f, UI.screenWidth, 300f));
            Text.Anchor = TextAnchor.UpperLeft;
            DoCustomBottomButtons();
        }

        private void DoCustomBottomButtons()
        {
            int num = (!TutorSystem.TutorialMode) ? 5 : 4;
            int num2 = (num < 4 || !((float)UI.screenWidth < 1340f)) ? 1 : 2;
            int num3 = Mathf.CeilToInt((float)num / (float)num2);
            Vector2 bottomButSize = Page.BottomButSize;
            float num4 = bottomButSize.x * (float)num3 + 10f * (float)(num3 + 1);
            float num5 = num2;
            Vector2 bottomButSize2 = Page.BottomButSize;
            float num6 = num5 * bottomButSize2.y + 10f * (float)(num2 + 1);
            Rect rect = new Rect(((float)UI.screenWidth - num4) / 2f, (float)UI.screenHeight - num6 - 4f, num4, num6);
            WorldInspectPane worldInspectPane = Find.WindowStack.WindowOfType<WorldInspectPane>();
            if (worldInspectPane != null && rect.x < InspectPaneUtility.PaneWidthFor(worldInspectPane) + 4f)
            {
                rect.x = InspectPaneUtility.PaneWidthFor(worldInspectPane) + 4f;
            }
            Widgets.DrawWindowBackground(rect);
            float num7 = rect.xMin + 10f;
            float num8 = rect.yMin + 10f;
            Text.Font = GameFont.Small;
            float x = num7;
            float y = num8;
            Vector2 bottomButSize3 = Page.BottomButSize;
            float x2 = bottomButSize3.x;
            Vector2 bottomButSize4 = Page.BottomButSize;
            float num9 = num7;
            Vector2 bottomButSize5 = Page.BottomButSize;
            num7 = num9 + (bottomButSize5.x + 10f);
            if (!TutorSystem.TutorialMode)
            {
                float x3 = num7;
                float y2 = num8;
                Vector2 bottomButSize6 = Page.BottomButSize;
                float x4 = bottomButSize6.x;
                Vector2 bottomButSize7 = Page.BottomButSize;
                if (Widgets.ButtonText(new Rect(x3, y2, x4, bottomButSize7.y), "Advanced".Translate()))
                {
                    Find.WindowStack.Add(new Dialog_AdvancedGameConfig(Find.WorldInterface.SelectedTile));
                }
                float num10 = num7;
                Vector2 bottomButSize8 = Page.BottomButSize;
                num7 = num10 + (bottomButSize8.x + 10f);
            }
            float x5 = num7;
            float y3 = num8;
            Vector2 bottomButSize9 = Page.BottomButSize;
            float x6 = bottomButSize9.x;
            Vector2 bottomButSize10 = Page.BottomButSize;
            if (Widgets.ButtonText(new Rect(x5, y3, x6, bottomButSize10.y), "SelectRandomSite".Translate()))
            {
                SoundDefOf.Click.PlayOneShotOnCamera();
                Find.WorldInterface.SelectedTile = TileFinder.RandomStartingTile();
                Find.WorldCameraDriver.JumpTo(Find.WorldGrid.GetTileCenter(Find.WorldInterface.SelectedTile));
            }
            float num11 = num7;
            Vector2 bottomButSize11 = Page.BottomButSize;
            num7 = num11 + (bottomButSize11.x + 10f);
            if (num2 == 2)
            {
                num7 = rect.xMin + 10f;
                float num12 = num8;
                Vector2 bottomButSize12 = Page.BottomButSize;
                num8 = num12 + (bottomButSize12.y + 10f);
            }
            float x7 = num7;
            float y4 = num8;
            Vector2 bottomButSize13 = Page.BottomButSize;
            float x8 = bottomButSize13.x;
            Vector2 bottomButSize14 = Page.BottomButSize;
            if (Widgets.ButtonText(new Rect(x7, y4, x8, bottomButSize14.y), "WorldFactionsTab".Translate()))
            {
                Find.WindowStack.Add(new Dialog_FactionDuringLanding());
            }
            float num13 = num7;
            Vector2 bottomButSize15 = Page.BottomButSize;
            num7 = num13 + (bottomButSize15.x + 10f);
            float x9 = num7;
            float y5 = num8;
            Vector2 bottomButSize16 = Page.BottomButSize;
            float x10 = bottomButSize16.x;
            Vector2 bottomButSize17 = Page.BottomButSize;
            if (Widgets.ButtonText(new Rect(x9, y5, x10, bottomButSize17.y), "Next".Translate()))
            {
                if (Find.WorldSelector.selectedTile < 0)
                    return;

                StartGame(Find.WorldSelector.selectedTile);
            }
            float num14 = num7;
            Vector2 bottomButSize18 = Page.BottomButSize;
            num7 = num14 + (bottomButSize18.x + 10f);
            GenUI.AbsorbClicksInRect(rect);
        }

        private void StartGame(int tile)
        {
            var init = Current.Game.InitData;

            if (WorldEditor.LoadedTemplate.PawnSelectMode == PawnSelectMode.Standart)
            {
                foreach (var scenPart in Find.Scenario.AllParts)
                {
                    ScenPart_ConfigPage_ConfigureStartingPawns part = scenPart as ScenPart_ConfigPage_ConfigureStartingPawns;
                    if (part != null)
                    {
                        init.startingAndOptionalPawns = new List<Pawn>(part.pawnChoiceCount);
                        init.startingPawnCount = part.pawnCount;
                        for (int i = 0; i < init.startingPawnCount; i++)
                        {
                            Pawn p = PawnGenerator.GeneratePawn(PawnKindDefOf.Colonist, init.playerFaction);
                            init.startingAndOptionalPawns.Add(p);
                        }

                        break;
                    }
                }
            }

            init.startingTile = tile;
            Find.World.renderer.wantedMode = WorldRenderMode.None;

            if (WorldEditor.LoadedTemplate.PawnSelectMode == PawnSelectMode.None)
            {
                Action preLoadLevelAction = delegate
                {
                    Find.GameInitData.PrepForMapGen();
                    Find.GameInitData.startedFromEntry = true;
                    Find.Scenario.PreMapGenerate();
                };
                LongEventHandler.QueueLongEvent(preLoadLevelAction, "Play", "GeneratingMap", doAsynchronously: true, null);

                return;
            }

            //if (WorldEdit.EdbLoaded)
            //    EdbConfigurator();

            var page = new Page_ConfigureStartingPawns();
            page.nextAct = nextAct = delegate
            {
                Action preLoadLevelAction = delegate
                {
                    Find.GameInitData.PrepForMapGen();
                    Find.GameInitData.startedFromEntry = true;
                    Find.Scenario.PreMapGenerate();
                };
                LongEventHandler.QueueLongEvent(preLoadLevelAction, "Play", "GeneratingMap", doAsynchronously: true, null);
            };
            Find.WindowStack.Add(page);
        }

        public static void EdbConfigurator(bool show = false)
        {
            Type type = AccessTools.TypeByName("EdB.PrepareCarefully.PrepareCarefully");
            PropertyInfo p1 = type.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static);
            object v = p1.GetValue(null, null);
            type.GetMethod("Initialize").Invoke(v, null);

            Type type2 = AccessTools.TypeByName("EdB.PrepareCarefully.Page_PrepareCarefully");
            Page page = Activator.CreateInstance(type2) as Page;
            EdbInstance = page;            

            if(show)
                Find.WindowStack.Add(page);
        }
    }
}
