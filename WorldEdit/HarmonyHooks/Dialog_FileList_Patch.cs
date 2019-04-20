using Harmony;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace WorldEdit.HarmonyHooks
{
    [HarmonyPatch(typeof(Dialog_FileList))]
    [HarmonyPatch("PostOpen")]
    class GetSavesGamesConstructorHook
    {
        static void Postfix()
        {
            WorldEditor.isWorldTemplate = false;
        }
    }

    [HarmonyPatch(typeof(Dialog_SaveFileList))]
    [HarmonyPatch("ReloadFiles")]
    class GetSavesListHook
    {
        static void Postfix(Dialog_SaveFileList __instance)
        {
            ChangeFileList(__instance);
        }

        static void ChangeFileList(Dialog_SaveFileList instance)
        {
            object value = Utils.GetInstanceField(typeof(Dialog_SaveFileList), instance, "files");

            if (value != null)
            {
                List<SaveFileInfo> infos = value as List<SaveFileInfo>;

                if (infos != null)
                {
                    for (int i = 0; i < infos.Count; i++)
                    {
                        SaveFileInfo info = infos[i];

                        if (info.FileInfo.Name.Contains("wtemplate_"))
                            infos.Remove(info);
                    }
                }
            }
        }
    }
}
