using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using Verse;
using WorldEdit.Interfaces;
using WorldEdit.WorldGen;
using WorldEdit.WorldGen.Generators;

namespace WorldEdit.Editor
{
    internal class WorldGenStepsMenu : FWindow
    {
        public override Vector2 InitialSize => new Vector2(600, 500);

        public Vector2 scroll = Vector2.zero;

        private static Dictionary<GeneratorMode, List<Generator>> Generators = new Dictionary<GeneratorMode, List<Generator>>();
        private GeneratorMode mode;
        private Generator generator = null;

        static WorldGenStepsMenu()
        {
            Type basicType = typeof(Generator);
            IEnumerable<Type> list = Assembly.GetAssembly(basicType).GetTypes().Where(type => type.IsSubclassOf(basicType));
            List<Generator> gens = new List<Generator>();
            foreach (Type itm in list)
            {
                Generator instance = Activator.CreateInstance(itm) as Generator;
                gens.Add(instance);
            }

            foreach (GeneratorMode mode in Enum.GetValues(typeof(GeneratorMode)))
            {
                List<Generator> generatorsForType = gens.Where(gen => gen.Mode == mode).ToList();
                Generators.Add(mode, generatorsForType);
            }
        }

        public WorldGenStepsMenu()
        {
            resizeable = false;

            mode = Generators.Keys.RandomElement();
            generator = Generators[mode].RandomElement();
        }

        public override void DoWindowContents(Rect inRect)
        {
            Text.Font = GameFont.Small;

            Widgets.Label(new Rect(0, 0, 280, 20), Translator.Translate("WorldGenStepsTitle"));

            if(Widgets.ButtonText(new Rect(0, 30, 200, 20), Translator.Translate("UseGenerator")))
            {
                if (generator != null)
                    generator.RunGenerator();
            }

            Widgets.Label(new Rect(0, 60, 80, 20), Translator.Translate("GenMode"));
            if(Widgets.ButtonText(new Rect(90, 60, 150, 20), Translator.Translate($"{mode}_title")))
            {
                List<FloatMenuOption> list = new List<FloatMenuOption>();
                foreach (GeneratorMode mode in Enum.GetValues(typeof(GeneratorMode)))
                    list.Add(new FloatMenuOption(Translator.Translate($"{mode}_title"), delegate
                    {
                        this.mode = mode;
                        generator = Generators[mode].FirstOrDefault();
                    }));
                Find.WindowStack.Add(new FloatMenu(list));
            }
            Widgets.Label(new Rect(265, 60, 80, 20), Translator.Translate("GenType"));
            if (Widgets.ButtonText(new Rect(310, 60, 260, 20), generator.Title))
            {
                List<FloatMenuOption> list = new List<FloatMenuOption>();
                foreach (var generator in Generators[mode])
                    list.Add(new FloatMenuOption(generator.Title, delegate
                    {
                        this.generator = generator;
                    }));
                Find.WindowStack.Add(new FloatMenu(list));
            }
            Widgets.LabelScrollable(new Rect(0, 90, 300, 300), generator.Description, ref scroll);
        }
    }
}
