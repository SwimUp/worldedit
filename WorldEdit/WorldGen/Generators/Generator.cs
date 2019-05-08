using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Verse;

namespace WorldEdit.WorldGen.Generators
{
    public class Parameter
    {
        public FieldInfo Param;

        public string Name => Translator.Translate($"{Param.Name}_title");

        public string Description => Translator.Translate($"{Param.Name}");

        public object value;
    }

    public class Settings
    {
        public List<Parameter> Parameters = new List<Parameter>();

        public void AddParam(FieldInfo paramName, object value)
        {
            foreach(var param in Parameters)
            {
                if(param.Param == paramName)
                {
                    Log.Warning("Trying add parameter, but he already exists");
                    return;
                }
            }

            Parameter newParam = new Parameter
            {
                Param = paramName,
                value = value
            };

            Parameters.Add(newParam);
        }
    }

    public abstract class Generator
    {
        public abstract string Title {get;}

        public abstract string Description { get; }

        public abstract GeneratorMode Mode { get; }

        public abstract GeneratorType Type { get; }

        public abstract void RunGenerator();

        public Settings Settings = new Settings();

        public void Setup()
        {
            foreach (var param in Settings.Parameters)
            {
                try
                {
                    if (param.Param.FieldType.IsClass)
                        continue;

                    if (param.Param.FieldType == typeof(FloatRange))
                    {
                        param.Param.SetValue(this, param.value);
                    }
                    else
                    {
                        param.Param.SetValue(this, Utils.GetValue(param.value, param.Param.FieldType));
                    }
                }catch
                {
                    Log.Message($"Cannot convert value");
                }
            }
        }
    }
}
