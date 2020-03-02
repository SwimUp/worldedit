using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml.Serialization;
using UnityEngine;
using Verse;
using Verse.Profile;
using WorldEdit.Editor;
using WorldEdit.Interfaces;

namespace WorldEdit
{
    public enum PawnSelectMode : byte
    {
        None = 0,
        Standart = 1
    }

    public sealed class WorldTemplateManager : FWindow
    {
        class ScenarioInfo : Page_SelectScenario
        {
            protected override bool CanDoNext()
            {
                FieldInfo scen = typeof(Page_SelectScenario).GetField("curScen", BindingFlags.NonPublic | BindingFlags.Instance);
                if(scen == null)
                {
                    return false;
                }
                Scenario scenario = scen.GetValue(this) as Scenario;
                if(scenario == null)
                {
                    return false;
                }
                Current.Game.Scenario = scenario;

                Close();

                return false;
            }
        }

        public static string TemplateFolderName = "WorldTemplates";
        public static string TemplateFolder => Path.Combine(GenFilePaths.SaveDataFolderPath, $"Saves\\{TemplateFolderName}\\");
        public static List<WorldTemplate> Templates = new List<WorldTemplate>();
        public override Vector2 InitialSize => new Vector2(720, 600);
        private Vector2 scrollDesc = Vector2.zero;

        private string saveName = string.Empty;
        private string author = string.Empty;
        private string description = string.Empty;

        private bool editor = true;

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

            StartPawnsFromTemplate.StartPawns = template.StartPawns;

            GameDataSaveLoader.SaveGame(savePath);

            Messages.Message("Tempalte has been created", MessageTypeDefOf.PositiveEvent, false);
        }

        public WorldTemplateManager()
        {
            resizeable = false;
        }

        public override void PreOpen()
        {
            saveName = WorldEditor.LoadedTemplate?.WorldName;
            author = WorldEditor.LoadedTemplate?.Author;
            description = WorldEditor.LoadedTemplate?.Description;
            Find.GameInitData.startingAndOptionalPawns = WorldEditor.LoadedTemplate?.StartPawns;

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

            Widgets.Label(new Rect(310, 15, 400, 20), Translator.Translate("SelectStorytellerTitle"));
            if (Widgets.ButtonText(new Rect(310, 40, 400, 20), Current.Game.storyteller.def.LabelCap))
            {
                Find.WindowStack.Add(new Page_SelectStorytellerInGame());
            }
            Widgets.Label(new Rect(310, 80, 400, 20), Translator.Translate("SelectScenario"));
            if (Widgets.ButtonText(new Rect(310, 105, 400, 20), Current.Game.Scenario.name))
            {
                Find.WindowStack.Add(new ScenarioInfo());
            }

            Widgets.Label(new Rect(310, 140, 400, 20), Translator.Translate("SelectPawnMode"));
            Rect rect1 = new Rect(310, 165, 400, 20);
            TooltipHandler.TipRegion(rect1, Translator.Translate("SelectPawnModeInfo"));
            if (Widgets.ButtonText(new Rect(310, 165, 400, 20), GetPawnModeLabel(WorldEditor.LoadedTemplate.PawnSelectMode)))
            {
                List<FloatMenuOption> list = new List<FloatMenuOption>();
                foreach (PawnSelectMode m in Enum.GetValues(typeof(PawnSelectMode)))
                {
                    list.Add(new FloatMenuOption(GetPawnModeLabel(m), delegate
                    {
                        WorldEditor.LoadedTemplate.PawnSelectMode = m;
                    }));
                }
                Find.WindowStack.Add(new FloatMenu(list));
            }
            if(WorldEditor.LoadedTemplate.PawnSelectMode == PawnSelectMode.None)
            {
                Widgets.Label(new Rect(310, 190, 200, 20), Translator.Translate("EditStartPawns"));
                if (Widgets.ButtonText(new Rect(515, 190, 195, 20), editor ? "Classic" : "EdB Prepare Carefull"))
                {
                    if (WorldEdit.EdbLoaded)
                        editor = !editor;
                }
                if (Widgets.ButtonText(new Rect(310, 215, 400, 20), Translator.Translate("")))
                {
                    if(editor)
                    {
                        StandartEditor();
                    }
                    else
                    {
                        EdbEditor();
                    }
                }
            }

            if (Widgets.ButtonText(new Rect(0, 555, 710, 20), Translator.Translate("CreateTemplate")))
            {
                CreateWorldTemplate();
            }
        }

        private void StandartEditor()
        {
            Find.WindowStack.Add(new PawnMenu(Find.GameInitData.startingAndOptionalPawns, 99));
        }

        private void EdbEditor()
        {
            CustomStartingSite.EdbConfigurator(true);
        }

        private string GetPawnModeLabel(PawnSelectMode mode)
        {
            switch(mode)
            {
                case PawnSelectMode.None:
                    {
                        return "Без выбора";
                    }
                case PawnSelectMode.Standart:
                    {
                        return "Стандартный выбор";
                    }
                default:
                    {
                        return "";
                    }
            }
        }

        private void CreateWorldTemplate()
        {
            if (string.IsNullOrEmpty(saveName))
            {
                Messages.Message("Enter correct template name", MessageTypeDefOf.NeutralEvent, false);
                return;
            }

            if (string.IsNullOrEmpty(author))
            {
                Messages.Message("Enter correct author", MessageTypeDefOf.NeutralEvent, false);
                return;
            }

            if (string.IsNullOrEmpty(description))
            {
                Messages.Message("Enter correct description", MessageTypeDefOf.NeutralEvent, false);
                return;
            }

            WorldTemplate template = WorldEditor.LoadedTemplate;

            template.WorldName = saveName;
            template.Storyteller = Current.Game.storyteller.def.LabelCap;
            template.Scenario = Current.Game.Scenario.name;
            template.Author = author;
            template.Description = description;
            template.StartPawns = Find.GameInitData.startingAndOptionalPawns;

            SaveTemplate(template);

            WorldEditor.LoadedTemplate = template;

            LoadTemplates();
        }
    }
}
