using Harmony;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace WorldEdit.HarmonyHooks
{
    static class RPP
    {
        static void Postfix()
        {
            Log.Message("OPEN!!!");
        }
    }
}
