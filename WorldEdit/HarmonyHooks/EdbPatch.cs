using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Verse;

namespace WorldEdit.HarmonyHooks
{
    class EdbPatch
    {
        public static bool Prefix()
        {
            if (WorldEditor.isEdit && WorldEditor.isInit)
            {
                if (WorldEditor.LoadedTemplate.PawnSelectMode == PawnSelectMode.None)
                {
                    Type type = AccessTools.TypeByName("EdB.PrepareCarefully.Page_PrepareCarefully");
                    FieldInfo f1 = type.GetField("controller", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
                        | BindingFlags.Static);
                    object v2 = f1.GetValue(CustomStartingSite.EdbInstance);
                    Type type2 = v2.GetType();
                    var method = type2.GetMethod("PrepareGame");
                    method.Invoke(v2, null);

                    Messages.Message("Pawns successfully saved", MessageTypeDefOf.PositiveEvent);
                    return false;
                }
            }
            return true;
        }
    }
}
