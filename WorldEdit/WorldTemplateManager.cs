using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using WorldEdit.Interfaces;

namespace WorldEdit
{
    public sealed class WorldTemplateManager : Window, IFWindow
    {
        public override Vector2 InitialSize => new Vector2(800, 600);
        private Vector2 scrollPositionStoryteller = Vector2.zero;
        private Vector2 scrollPositionScenario = Vector2.zero;

        private string saveName = string.Empty;

        //Current.Game.storyteller
        public override void DoWindowContents(Rect inRect)
        {
            Text.Font = GameFont.Small;

            saveName = Widgets.TextField(new Rect(0, 0, 780, 20), saveName);
            Widgets.Label(new Rect(0, 30, 300, 20), Translator.Translate("SelectStorytellerTitle"));
            int tellersSize = DefDatabase<StorytellerDef>.DefCount * 25;
            Rect scrollRectFact = new Rect(0, 60, 300, 200);
            Rect scrollVertRectFact = new Rect(0, 0, scrollRectFact.x, tellersSize);
            Widgets.BeginScrollView(scrollRectFact, ref scrollPositionStoryteller, scrollVertRectFact);
            int x = 0;
            foreach (var storyteller in DefDatabase<StorytellerDef>.AllDefs)
            {
                if (Widgets.ButtonText(new Rect(0, x, 290, 20), storyteller.label))
                {
                    Current.Game.storyteller = new Storyteller(storyteller, DifficultyDefOf.Easy);
                }
                x += 22;
            }
            Widgets.EndScrollView();


            Widgets.Label(new Rect(0, 270, 300, 20), Translator.Translate("SelectScenario"));
            int scenarioSize = ScenarioLister.AllScenarios().ToList().Count * 25;
            Rect scrollRectScen = new Rect(0, 300, 300, 200);
            Rect scrollVertRectScen = new Rect(0, 0, scrollRectScen.x, scenarioSize);
            Widgets.BeginScrollView(scrollRectScen, ref scrollPositionScenario, scrollVertRectScen);
            x = 300;
            foreach (var scenario in ScenarioLister.AllScenarios())
            {
                if (Widgets.ButtonText(new Rect(0, x, 290, 20), scenario.name))
                {
                    Current.Game.Scenario = scenario;
                }
                x += 22;
            }
            Widgets.EndScrollView();

            if (Widgets.ButtonText(new Rect(0, 540, 780, 20), Translator.Translate("CreateTemplate")))
            {
                CreateWorldTemplate();
            }
        }

        private void CreateWorldTemplate()
        {
            string fileName = $"wtemplate_{saveName}";

            GameDataSaveLoader.SaveGame(fileName);
        }

        public void Show()
        {
            {
                if (Find.WindowStack.IsOpen(this))
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
}
