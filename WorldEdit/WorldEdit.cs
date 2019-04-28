using Harmony;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Verse;
using WorldEdit.HarmonyHooks;

namespace WorldEdit
{
    [StaticConstructorOnStartup]
    public class WorldEdit : Mod
    {
        internal static HarmonyInstance harmonyInstance;

        public WorldEdit(ModContentPack content) : base(content)
        {
            harmonyInstance = HarmonyInstance.Create("net.funkyshit.worldedit");
            harmonyInstance.PatchAll(Assembly.GetExecutingAssembly());
        }
    }
}
