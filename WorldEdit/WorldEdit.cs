using Harmony;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using Verse;
using WorldEdit.HarmonyHooks;

namespace WorldEdit
{
    [StaticConstructorOnStartup]
    public class WorldEdit : Mod
    {
        internal static HarmonyInstance harmonyInstance;

        public static Settings Settings;

        public WorldEdit(ModContentPack content) : base(content)
        {
            harmonyInstance = HarmonyInstance.Create("net.funkyshit.worldedit");
            harmonyInstance.PatchAll(Assembly.GetExecutingAssembly());

            Settings = GetSettings<Settings>();
        }

        public override string SettingsCategory()
        {
            return "WorldEdit";
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            Settings.DoSettingsWindowContents(inRect);
        }
    }
}
