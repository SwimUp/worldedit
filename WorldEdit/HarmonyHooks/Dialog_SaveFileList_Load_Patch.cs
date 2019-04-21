using Harmony;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace WorldEdit.HarmonyHooks
{
    [HarmonyPatch(typeof(Dialog_SaveFileList_Load))]
    [HarmonyPatch("ReloadFiles")]
    class Dialog_SaveFileList_Load_Patch
    {
        static void Postfix(Dialog_SaveFileList_Load __instance)
        {
            ChangeFileList(__instance);
        }

        static void ChangeFileList(Dialog_SaveFileList_Load instance)
        {
            object value = Utils.GetInstanceField(typeof(Dialog_SaveFileList_Load), instance, "files");

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
