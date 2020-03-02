using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using WorldEdit.Interfaces;

namespace WorldEdit.Editor
{
    internal class IconMenu : FWindow
    {
        public override Vector2 InitialSize => new Vector2(210, 300);
        private Vector2 scrollPosition = Vector2.zero;

        private List<IconDef> icons;
        private IconCategory category;
        private SettlementCreator creator;

        public IconMenu(SettlementCreator creator)
        {
            this.creator = creator;
            resizeable = false;

            icons = DefDatabase<IconDef>.AllDefs.Where(def => def.Category == category).ToList();
        }
        public override void DoWindowContents(Rect inRect)
        {
            Text.Font = GameFont.Small;

            if (Widgets.ButtonText(new Rect(0, 15, 200, 20), Translator.Translate("CategoryFilter")))
            {
                List<FloatMenuOption> list = new List<FloatMenuOption>();
                foreach (IconCategory c in Enum.GetValues(typeof(IconCategory)))
                {
                    list.Add(new FloatMenuOption(c.ToString(), delegate
                    {
                        category = c;
                        icons = DefDatabase<IconDef>.AllDefs.Where(def => def.Category == category).ToList();
                    }));
                }
                Find.WindowStack.Add(new FloatMenu(list));
            }

            int size = icons.Count * 92;
            Rect scrollRectFact = new Rect(0, 40, 190, 250);
            Rect scrollVertRectFact = new Rect(0, 0, scrollRectFact.x, size);
            Widgets.BeginScrollView(scrollRectFact, ref scrollPosition, scrollVertRectFact);
            int y = 0;
            foreach (IconDef icon in icons)
            {
                if (Widgets.ButtonImage(new Rect(0, y, 80, 80), icon.Texture))
                {
                    creator.CustomIcon = icon;
                }
                y += 90;
            }
            Widgets.EndScrollView();
        }
    }

    internal sealed class SettlementCreator : FWindow
    {

        public override Vector2 InitialSize => new Vector2(510, 450);
        private Vector2 scrollPosition = Vector2.zero;

        /// <summary>
        /// Имя поселения
        /// </summary>
        private string settlementName = string.Empty;

        /// <summary>
        /// Фракция поселения
        /// </summary>
        private Faction selectedFaction;

        /// <summary>
        /// Новое поселение
        /// </summary>
        private Settlement newSettlement = null;

        internal IconDef CustomIcon;
        private bool useCustomicon = false;

        public SettlementCreator()
        {
            resizeable = false;

            selectedFaction = Find.FactionManager.AllFactionsListForReading.RandomElement();

            if (DefDatabase<IconDef>.DefCount > 0)
                CustomIcon = DefDatabase<IconDef>.AllDefs.FirstOrDefault();
            else
                CustomIcon = null;
        }

        public override void DoWindowContents(Rect inRect)
        {
            Text.Font = GameFont.Small;

            Widgets.Label(new Rect(0, 0, 350, 30), Translator.Translate("SettlementCreatorTitle"));

            Widgets.Label(new Rect(0, 40, 100, 30), Translator.Translate("SettlementNameField"));
            settlementName = Widgets.TextField(new Rect(105, 40, 385, 30), settlementName);

            int factionDefSize = Find.FactionManager.AllFactionsListForReading.Count * 25;
            Rect scrollRectFact = new Rect(0, 80, 490, 200);
            Rect scrollVertRectFact = new Rect(0, 0, scrollRectFact.x, factionDefSize);
            Widgets.BeginScrollView(scrollRectFact, ref scrollPosition, scrollVertRectFact);
            int yButtonPos = 0;
            foreach (var spawnedFaction in Find.FactionManager.AllFactionsListForReading)
            {
                if (Widgets.ButtonText(new Rect(0, yButtonPos, 480, 20), spawnedFaction.Name))
                {
                    selectedFaction = spawnedFaction;
                    Messages.Message($"Selected faction: {selectedFaction.Name}", MessageTypeDefOf.NeutralEvent, false);
                }
                yButtonPos += 22;
            }
            Widgets.EndScrollView();

            Widgets.Label(new Rect(0, 310, 60, 30), Translator.Translate("FactionIcon"));
            if (DefDatabase<IconDef>.DefCount == 0)
            {
                Widgets.Label(new Rect(70, 310, 300, 30), Translator.Translate("NoCustomIconsInfo"));
            }
            else
            {
                if (Widgets.ButtonImage(new Rect(70, 310, 30, 30), CustomIcon.Texture))
                {
                    Find.WindowStack.Add(new IconMenu(this));
                }
                if (Widgets.RadioButtonLabeled(new Rect(120, 310, 300, 30), Translator.Translate("UseCustomIcon"), useCustomicon))
                {
                    useCustomicon = !useCustomicon;
                }
            }

            if (Widgets.ButtonText(new Rect(0, 370, 490, 20), Translator.Translate("CreateNewSettlement")))
            {
                CreateSettlement();
            }
        }

        private void CreateSettlement()
        {
            if (selectedFaction == null)
            {
                Messages.Message($"Select faction", MessageTypeDefOf.NeutralEvent, false);
                return;
            }

            if (string.IsNullOrEmpty(settlementName))
            {
                Messages.Message($"Enter valid settlement name", MessageTypeDefOf.NeutralEvent, false);
                return;
            }

            if (Find.WorldSelector.selectedTile < 0)
            {
                Messages.Message($"Select tile", MessageTypeDefOf.NeutralEvent, false);
                return;
            }

            if (Find.WorldObjects.AnySettlementAt(Find.WorldSelector.selectedTile))
            {
                Messages.Message($"Some settlement is already in this place", MessageTypeDefOf.NeutralEvent, false);
                return;
            }

            newSettlement = (Settlement)WorldObjectMaker.MakeWorldObject(WorldObjectDefOf.Settlement);
            newSettlement.SetFaction(selectedFaction);
            newSettlement.Tile = Find.WorldSelector.selectedTile;
            newSettlement.Name = settlementName;

            if(useCustomicon)
            {
                CustomFactions.CustomIcons.Add(newSettlement.Name, CustomIcon);
            }

            Find.WorldObjects.Add(newSettlement);
        }
    }
}
