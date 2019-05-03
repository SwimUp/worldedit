using RimWorld;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using UnityEngine;
using Verse;
using Verse.Profile;
using WorldEdit.Interfaces;

namespace WorldEdit
{
    public sealed class WorldTemplateManager : FWindow
    {
        public static string TemplateFolderName = "WorldTemplates";
        public static string TemplateFolder => Path.Combine(GenFilePaths.SaveDataFolderPath, $"Saves\\{TemplateFolderName}\\");
        public static List<WorldTemplate> Templates = new List<WorldTemplate>();
        public override Vector2 InitialSize => new Vector2(620, 600);
        private Vector2 scrollPositionStoryteller = Vector2.zero;
        private Vector2 scrollPositionScenario = Vector2.zero;
        private Vector2 scrollDesc = Vector2.zero;

        private string saveName = string.Empty;
        private string author = string.Empty;
        private string description = string.Empty;

        static WorldTemplateManager()
        {
            LoadTemplates();
        }

        public static void LoadTemplates()
        {
            if (!Directory.Exists(TemplateFolder))
                Directory.CreateDirectory(TemplateFolder);

            Templates.Clear();

            XmlSerializer serializer = new XmlSerializer(typeof(WorldTemplate));

            foreach (var folder in Directory.GetDirectories(TemplateFolder))
            {
                string templateInfo = Path.Combine(folder, "info.xml");
                if (File.Exists(templateInfo))
                {
                    WorldTemplate template = Utils.Deserialize<WorldTemplate>(serializer, templateInfo);
                    Templates.Add(template);
                }
            }
        }

        public static void SaveTemplate(WorldTemplate template)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(WorldTemplate));

            string folderName = Path.Combine(TemplateFolder, template.WorldName);
            string infoFile = Path.Combine(folderName, "info.xml");

            if (File.Exists(infoFile))
                File.Delete(infoFile);

            string saveName = $"{template.WorldName}";
            string savePath = Path.Combine(folderName, saveName);

            template.FilePath = $"{TemplateFolderName}\\{template.WorldName}\\{template.WorldName}";

            Directory.CreateDirectory(folderName);

            using (FileStream fs = new FileStream(infoFile, FileMode.OpenOrCreate))
            {
                serializer.Serialize(fs, template);
            }

            GameDataSaveLoader.SaveGame(savePath);

            Messages.Message("Tempalte has been created", MessageTypeDefOf.PositiveEvent);
        }

        public WorldTemplateManager()
        {
            resizeable = false;
        }

        public override void PreOpen()
        {
            if (WorldEditor.LoadedTemplate != null)
            {
                saveName = WorldEditor.LoadedTemplate.WorldName;
                author = WorldEditor.LoadedTemplate.Author;
                description = WorldEditor.LoadedTemplate.Description;
            }

            base.PreOpen();
        }

        public override void DoWindowContents(Rect inRect)
        {
            Text.Font = GameFont.Small;

            Widgets.Label(new Rect(0, 0, 150, 20), Translator.Translate("TemplateNameEdit"));
            saveName = Widgets.TextField(new Rect(0, 20, 300, 20), saveName);

            Widgets.Label(new Rect(0, 50, 150, 20), Translator.Translate("TemplateAuthorEdit"));
            author = Widgets.TextField(new Rect(0, 70, 300, 20), author);

            Widgets.Label(new Rect(0, 100, 150, 20), Translator.Translate("TemplateDescription"));
            description = Widgets.TextAreaScrollable(new Rect(0, 120, 300, 430), description, ref scrollDesc);

            Widgets.Label(new Rect(310, 15, 300, 20), Translator.Translate("SelectStorytellerTitle"));
            int tellersSize = DefDatabase<StorytellerDef>.DefCount * 25;
            Rect scrollRectFact = new Rect(310, 45, 300, 230);
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

            Widgets.Label(new Rect(310, 275, 300, 20), Translator.Translate("SelectScenario"));
            int scenarioSize = ScenarioLister.AllScenarios().ToList().Count * 25;
            Rect scrollRectScen = new Rect(310, 305, 300, 250);
            Rect scrollVertRectScen = new Rect(0, 0, scrollRectScen.x, scenarioSize);
            Widgets.BeginScrollView(scrollRectScen, ref scrollPositionScenario, scrollVertRectScen);
            x = 0;
            foreach (var scenario in ScenarioLister.AllScenarios())
            {
                if (Widgets.ButtonText(new Rect(0, x, 290, 20), scenario.name))
                {
                    Current.Game.Scenario = scenario;
                }
                x += 22;
            }
            Widgets.EndScrollView();

            if (Widgets.ButtonText(new Rect(0, 555, 610, 20), Translator.Translate("CreateTemplate")))
            {
                CreateWorldTemplate();
            }
        }

        private void CreateWorldTemplate()
        {
            if (string.IsNullOrEmpty(saveName))
            {
                Messages.Message("Enter correct template name", MessageTypeDefOf.NeutralEvent);
                return;
            }

            if (string.IsNullOrEmpty(author))
            {
                Messages.Message("Enter correct author", MessageTypeDefOf.NeutralEvent);
                return;
            }

            if (string.IsNullOrEmpty(description))
            {
                Messages.Message("Enter correct description", MessageTypeDefOf.NeutralEvent);
                return;
            }

            WorldTemplate template = new WorldTemplate();

            template.WorldName = saveName;
            template.Storyteller = Current.Game.storyteller.def.LabelCap;
            template.Scenario = Current.Game.Scenario.name;
            template.Author = author;
            template.Description = description;

            SaveTemplate(template);

            WorldEditor.LoadedTemplate = template;

            LoadTemplates();
        }
    }
}
