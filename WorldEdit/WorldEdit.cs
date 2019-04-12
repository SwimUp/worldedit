using Harmony;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Verse;

namespace WorldEdit
{
    [StaticConstructorOnStartup]
    internal class WorldEdit : Mod
    {
        public WorldEdit(ModContentPack content) : base(content)
        {
            HarmonyInstance harmonyInstance = HarmonyInstance.Create("net.funkyshit.rimoverhaul");
            harmonyInstance.PatchAll(Assembly.GetExecutingAssembly());
        }

        public void Save()
        {
        }
    }
}
