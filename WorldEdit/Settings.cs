using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace WorldEdit
{
    public class Settings : ModSettings
    {
        public static bool FullyActiveEditor = false;

        public static KeyCode EditorHotKey = KeyCode.F5;
        public static KeyCode FactionHotKey = KeyCode.F6;
        public static KeyCode RiversAndRoadsHotKey = KeyCode.F7;
        public static KeyCode WorldObjectHotKey = KeyCode.F8;

        public static void DoSettingsWindowContents(Rect inRect)
        {
            Listing_Standard listing_Standard = new Listing_Standard();
            listing_Standard.Begin(inRect);
            listing_Standard.GapLine();
            listing_Standard.Label(Translator.Translate("WE_Settings_General"));
            Rect rect2 = new Rect(0, listing_Standard.CurHeight, 600, 20);
            TooltipHandler.TipRegion(rect2, Translator.Translate("PernamentEditorInfo"));
            if (listing_Standard.RadioButton(Translator.Translate("PernamentEditor"), FullyActiveEditor))
            {
                FullyActiveEditor = !FullyActiveEditor;
            }
            listing_Standard.GapLine();
            if (listing_Standard.ButtonText($"{Translator.Translate("EditHotKeyInfo")}: {EditorHotKey}"))
            {
                List<FloatMenuOption> list = new List<FloatMenuOption>();
                foreach (KeyCode code in Enum.GetValues(typeof(KeyCode)))
                {
                    list.Add(new FloatMenuOption(code.ToString(), delegate
                            {
                                EditorHotKey = code;
                            }));
                }
                Find.WindowStack.Add(new FloatMenu(list));
            }
            if (listing_Standard.ButtonText($"{Translator.Translate("FactionHotKeyInfo")}: {FactionHotKey}"))
            {
                List<FloatMenuOption> list = new List<FloatMenuOption>();
                foreach (KeyCode code in Enum.GetValues(typeof(KeyCode)))
                {
                    list.Add(new FloatMenuOption(code.ToString(), delegate
                    {
                        FactionHotKey = code;
                    }));
                }
                Find.WindowStack.Add(new FloatMenu(list));
            }
            if (listing_Standard.ButtonText($"{Translator.Translate("RiversAndRoadsHotKeyInfo")}: {RiversAndRoadsHotKey}"))
            {
                List<FloatMenuOption> list = new List<FloatMenuOption>();
                foreach (KeyCode code in Enum.GetValues(typeof(KeyCode)))
                {
                    list.Add(new FloatMenuOption(code.ToString(), delegate
                    {
                        RiversAndRoadsHotKey = code;
                    }));
                }
                Find.WindowStack.Add(new FloatMenu(list));
            }
            if (listing_Standard.ButtonText($"{Translator.Translate("WorldObjectHotKeyInfo")}: {WorldObjectHotKey}"))
            {
                List<FloatMenuOption> list = new List<FloatMenuOption>();
                foreach (KeyCode code in Enum.GetValues(typeof(KeyCode)))
                {
                    list.Add(new FloatMenuOption(code.ToString(), delegate
                    {
                        WorldObjectHotKey = code;
                    }));
                }
                Find.WindowStack.Add(new FloatMenu(list));
            }

            listing_Standard.End();
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref FullyActiveEditor, "FullyActiveEditor", false);
            Scribe_Values.Look(ref EditorHotKey, "EditorHotKey", KeyCode.F5);
            Scribe_Values.Look(ref FactionHotKey, "FactionHotKey", KeyCode.F6);
            Scribe_Values.Look(ref RiversAndRoadsHotKey, "RiversAndRoadsHotKey", KeyCode.F7);
            Scribe_Values.Look(ref WorldObjectHotKey, "WorldObjectHotKey", KeyCode.F8);
        }
    }
}
