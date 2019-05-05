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

        public static bool EdbLoaded = false;
        public static bool RealisticPlanetsLoaded = false;

        public WorldEdit(ModContentPack content) : base(content)
        {
            harmonyInstance = HarmonyInstance.Create("net.funkyshit.worldedit");
            harmonyInstance.PatchAll(Assembly.GetExecutingAssembly());

            Settings = GetSettings<Settings>();

            try
            {
                EdbPatch(harmonyInstance);
                RealisticPlanetsPatch(harmonyInstance);
            }catch(Exception ex)
            {
                Log.Error($"Error while loading worldedit - {ex}");
            }
        }

        private static void EdbPatch(HarmonyInstance harmonyInstance)
        {
            Type type = AccessTools.TypeByName("EdB.PrepareCarefully.Page_PrepareCarefully");
            if (type != null)
            {
                if (harmonyInstance.Patch(type.GetMethod("ShowStartConfirmation"), prefix: new HarmonyMethod(typeof(EdbPatch).GetMethod("Prefix"))) == null)
                {
                    Log.Warning("Error while patching EDB.", false);
                }
                else
                {
                    EdbLoaded = true;
                    Log.Message("Edb successfully patched", false);
                }
            }
            else
            {
                Log.Warning("Prepare Carefully not found...skip", false);
            }
        }

        private static void RealisticPlanetsPatch(HarmonyInstance harmonyInstance)
        {
            Type type = AccessTools.TypeByName("Planets_Code.Planets_CreateWorldParams");
            if (type != null)
            {
                if (harmonyInstance.Patch(type.GetMethod("DoWindowContents"), new HarmonyMethod(null), new HarmonyMethod(typeof(RP_PatchWindow).GetMethod("PostfixRP"))) == null)
                {
                    Log.Warning("Error while patching Relastic planets", false);
                }
                else
                {
                    Log.Message("Realistic Planets successfully patched", false);
                    RealisticPlanetsLoaded = true;
                }
            }
            else
            {
                Log.Warning("Realistic planets not found...skip", false);
            }
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
