using RimWorld;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace WorldEdit.Editor
{
    internal class PawnMenu : Page
    {
        private Pawn curPawn;

        private static readonly Vector2 PawnPortraitSize = new Vector2(100f, 140f);

        private static readonly Vector2 PawnSelectorPortraitSize = new Vector2(70f, 110f);

        private Vector2 scroll = Vector2.zero;

        public const int MainRectsY = 100;

        public static Vector2 PawnCardSize = new Vector2(570f, 470f);

        public const int MaxNickLength = 16;

        public const int MaxTitleLength = 25;

        public static Regex ValidNameRegex = new Regex("^[\\p{L}0-9 '\\-]*$");

        private static float levelLabelWidth = -1f;

        private static List<SkillDef> skillDefsInListOrderCached;

        private const float SkillWidth = 240f;

        public const float SkillHeight = 24f;

        public const float SkillYSpacing = 3f;

        private const float LeftEdgeMargin = 6f;

        private const float IncButX = 205f;

        private const float IncButSpacing = 10f;

        private static readonly Color DisabledSkillColor = new Color(1f, 1f, 1f, 0.5f);

        private static Texture2D PassionMinorIcon = ContentFinder<Texture2D>.Get("UI/Icons/PassionMinor");

        private static Texture2D PassionMajorIcon = ContentFinder<Texture2D>.Get("UI/Icons/PassionMajor");

        private static Texture2D SkillBarFillTex = SolidColorMaterials.NewSolidColorTexture(new Color(1f, 1f, 1f, 0.1f));

        private int SkillsPerColumn = -1;

        public override string PageTitle => "CreateCharacters".Translate();

        private List<Pawn> allPawns = new List<Pawn>();

        private string test;

        public PawnMenu(List<Pawn> selectedPawns)
        {
            allPawns = selectedPawns;

            skillDefsInListOrderCached = (from sd in DefDatabase<SkillDef>.AllDefs
                                          orderby sd.listOrder descending
                                          select sd).ToList();
        }

        public override void DoWindowContents(Rect rect)
        {
            DrawPageTitle(rect);
            rect.yMin += 45f;
            DoBottomButtons(rect, Translator.Translate("SaveRefugeeList"), Translator.Translate("AddNewRefugee"), AddNewPawn, showNext: true, doNextOnKeypress: false);
            rect.yMax -= 38f;
            Rect rect2 = rect;
            rect2.width = 140f;
            DrawPawnList(rect2);
            UIHighlighter.HighlightOpportunity(rect2, "ReorderPawn");
            Rect rect3 = rect;
            rect3.xMin += 140f;
            Rect rect4 = rect3.BottomPartPixels(141f);
            rect3.yMax = rect4.yMin;
            rect3 = rect3.ContractedBy(4f);
            rect4 = rect4.ContractedBy(4f);
            DrawPortraitArea(rect3);
        }

        private void AddNewPawn()
        {
            allPawns.Add(DownedRefugeeQuestUtility.GenerateRefugee(-1));
        }

        private void DrawPawnList(Rect rect)
        {
            Rect rect2 = rect;
            rect2.height = 60f;
            rect2 = rect2.ContractedBy(4f);
            rect2.y += 15f;
            DrawPawnListLabelAbove(rect2, "StartingPawnsSelected".Translate());
            int size = allPawns.Count * 65;
            Rect scrollRectFact = new Rect(0, 15, rect2.width, rect.height + 5f);
            Rect scrollVertRectFact = new Rect(0, 0, scrollRectFact.x, size);
            Widgets.BeginScrollView(scrollRectFact, ref scroll, scrollVertRectFact, false);
            for (int i = 0; i < allPawns.Count; i++)
            {
                Pawn pawn = allPawns[i];
                GUI.BeginGroup(rect2);
                Rect rect3 = new Rect(Vector2.zero, rect2.size);
                Widgets.DrawOptionBackground(rect3, curPawn == pawn);
                MouseoverSounds.DoRegion(rect3);
                GUI.color = new Color(1f, 1f, 1f, 0.2f);
                Vector2 pawnSelectorPortraitSize = PawnSelectorPortraitSize;
                float x = 110f - pawnSelectorPortraitSize.x / 2f;
                Vector2 pawnSelectorPortraitSize2 = PawnSelectorPortraitSize;
                float y = 40f - pawnSelectorPortraitSize2.y / 2f;
                Vector2 pawnSelectorPortraitSize3 = PawnSelectorPortraitSize;
                float x2 = pawnSelectorPortraitSize3.x;
                Vector2 pawnSelectorPortraitSize4 = PawnSelectorPortraitSize;
                GUI.DrawTexture(new Rect(x, y, x2, pawnSelectorPortraitSize4.y), PortraitsCache.Get(pawn, PawnSelectorPortraitSize));
                GUI.color = Color.white;
                Rect rect4 = rect3.ContractedBy(4f).Rounded();
                NameTriple nameTriple = pawn.Name as NameTriple;
                Widgets.Label(label: (nameTriple == null) ? pawn.LabelShort : ((!string.IsNullOrEmpty(nameTriple.Nick)) ? nameTriple.Nick : nameTriple.First), rect: rect4.TopPart(0.5f).Rounded());
                Vector2 vector = Text.CalcSize(pawn.story.TitleCap);
                if (vector.x > rect4.width)
                {
                    Widgets.Label(rect4.BottomPart(0.5f).Rounded(), pawn.story.TitleShortCap);
                }
                else
                {
                    Widgets.Label(rect4.BottomPart(0.5f).Rounded(), pawn.story.TitleCap);
                }
                if (Event.current.type == EventType.MouseDown && Mouse.IsOver(rect3))
                {
                    curPawn = pawn;
                    SoundDefOf.Tick_Tiny.PlayOneShotOnCamera();
                }
                if(Widgets.ButtonText(new Rect(rect2.width - 40f, 3, 30, 20), "X"))
                {
                    allPawns.Remove(curPawn);
                }
                GUI.EndGroup();
                rect2.y += 60f;
            }
            Widgets.EndScrollView();
        }

        private void DrawPawnListLabelAbove(Rect rect, string label)
        {
            rect.yMax = rect.yMin;
            rect.yMin -= 30f;
            rect.xMin -= 4f;
            Text.Font = GameFont.Tiny;
            Text.Anchor = TextAnchor.LowerLeft;
            Widgets.Label(rect, label);
            Text.Anchor = TextAnchor.UpperLeft;
            Text.Font = GameFont.Small;
        }

        private void DrawPortraitArea(Rect rect)
        {
            if (curPawn == null)
                return;

            Widgets.DrawMenuSection(rect);
            rect = rect.ContractedBy(17f);
            Vector2 center = rect.center;
            float x = center.x;
            Vector2 pawnPortraitSize = PawnPortraitSize;
            float x2 = x - pawnPortraitSize.x / 2f;
            float y = rect.yMin - 20f;
            Vector2 pawnPortraitSize2 = PawnPortraitSize;
            float x3 = pawnPortraitSize2.x;
            Vector2 pawnPortraitSize3 = PawnPortraitSize;
            GUI.DrawTexture(new Rect(x2, y, x3, pawnPortraitSize3.y), PortraitsCache.Get(curPawn, PawnPortraitSize));
            Rect rect2 = rect;
            rect2.width = 500f;
            DrawCharacterCard(rect2, curPawn, rect);
            Rect rect3 = rect;
            rect3.yMin += 100f;
            rect3.xMin = rect2.xMax + 5f;
            rect3.height = 200f;
            Text.Font = GameFont.Medium;
            Widgets.Label(rect3, "Health".Translate());
            Text.Font = GameFont.Small;
            rect3.yMin += 35f;
            HealthCardUtility.DrawHediffListing(rect3, curPawn, showBloodLoss: true);
            Rect rect4 = new Rect(rect3.x, rect3.yMax, rect3.width, 200f);
            Text.Font = GameFont.Medium;
            Widgets.Label(rect4, "Relations".Translate());
            Text.Font = GameFont.Small;
            rect4.yMin += 35f;
            SocialCardUtility.DrawRelationsAndOpinions(rect4, curPawn);
        }

        private void DrawSkillSummaries(Rect rect)
        {
            rect.xMin += 10f;
            rect.xMax -= 10f;
            Widgets.DrawMenuSection(rect);
            rect = rect.ContractedBy(17f);
            Text.Font = GameFont.Medium;
            Widgets.Label(new Rect(rect.min, new Vector2(rect.width, 45f)), "TeamSkills".Translate());
            Text.Font = GameFont.Small;
            rect.yMin += 45f;
            rect = rect.LeftPart(0.25f);
            rect.height = 27f;
            List<SkillDef> allDefsListForReading = DefDatabase<SkillDef>.AllDefsListForReading;
            if (SkillsPerColumn < 0)
            {
                SkillsPerColumn = Mathf.CeilToInt((float)(from sd in allDefsListForReading
                                                          where sd.pawnCreatorSummaryVisible
                                                          select sd).Count() / 4f);
            }
            int num = 0;
            for (int i = 0; i < allDefsListForReading.Count; i++)
            {
                SkillDef skillDef = allDefsListForReading[i];
                if (skillDef.pawnCreatorSummaryVisible)
                {
                    Rect r = rect;
                    r.x = rect.x + rect.width * (float)(num / SkillsPerColumn);
                    r.y = rect.y + rect.height * (float)(num % SkillsPerColumn);
                    r.height = 24f;
                    r.width -= 4f;
                    Pawn pawn = FindBestSkillOwner(skillDef);
                    SkillUI.DrawSkill(pawn.skills.GetSkill(skillDef), r.Rounded(), SkillUI.SkillDrawMode.Menu, pawn.Name.ToString());
                    num++;
                }
            }
        }

        private Pawn FindBestSkillOwner(SkillDef skill)
        {
            Pawn pawn = allPawns[0];
            SkillRecord skillRecord = pawn.skills.GetSkill(skill);
            for (int i = 1; i < allPawns.Count; i++)
            {
                SkillRecord skill2 = allPawns[i].skills.GetSkill(skill);
                if (skillRecord.TotallyDisabled || skill2.Level > skillRecord.Level || (skill2.Level == skillRecord.Level && (int)skill2.passion > (int)skillRecord.passion))
                {
                    pawn = allPawns[i];
                    skillRecord = skill2;
                }
            }
            return pawn;
        }

        private void RandomizeCurPawn()
        {
            curPawn = DownedRefugeeQuestUtility.GenerateRefugee(-1);
        }


        protected override void DoNext()
        {
            Close();
        }

        public void SelectPawn(Pawn c)
        {
            if (c != curPawn)
            {
                curPawn = c;
            }
        }

        public void DrawCharacterCard(Rect rect, Pawn pawn, Rect creationRect = default(Rect))
        {
            GUI.BeginGroup(creationRect);
            Rect rect2 = new Rect(0f, 0f, 300f, 30f);
            NameTriple nameTriple = pawn.Name as NameTriple;
            if (nameTriple != null)
            {
                Rect rect3 = new Rect(rect2);
                rect3.width *= 0.333f;
                Rect rect4 = new Rect(rect2);
                rect4.width *= 0.333f;
                rect4.x += rect4.width;
                Rect rect5 = new Rect(rect2);
                rect5.width *= 0.333f;
                rect5.x += rect4.width * 2f;
                string name = nameTriple.First;
                string name2 = nameTriple.Nick;
                string name3 = nameTriple.Last;
                DoNameInputRect(rect3, ref name, 12);
                if (nameTriple.Nick == nameTriple.First || nameTriple.Nick == nameTriple.Last)
                {
                    GUI.color = new Color(1f, 1f, 1f, 0.5f);
                }
                DoNameInputRect(rect4, ref name2, 16);
                GUI.color = Color.white;
                DoNameInputRect(rect5, ref name3, 12);
                if (nameTriple.First != name || nameTriple.Nick != name2 || nameTriple.Last != name3)
                {
                    pawn.Name = new NameTriple(name, name2, name3);
                }
                TooltipHandler.TipRegion(rect3, "FirstNameDesc".Translate());
                TooltipHandler.TipRegion(rect4, "ShortIdentifierDesc".Translate());
                TooltipHandler.TipRegion(rect5, "LastNameDesc".Translate());
            }
            else
            {
                rect2.width = 999f;
                Text.Font = GameFont.Medium;
                Widgets.Label(rect2, pawn.Name.ToStringFull);
                Text.Font = GameFont.Small;
            }

            Rect rect6 = new Rect(creationRect.width - 24f - 100f, 0f, 100f, rect2.height);
            if (Widgets.ButtonText(rect6, "Randomize".Translate()))
            {
                SoundDefOf.Tick_Tiny.PlayOneShotOnCamera();
                RandomizeCurPawn();
            }
            UIHighlighter.HighlightOpportunity(rect6, "RandomizePawn");

            Rect rect6_1 = new Rect(creationRect.width - 24f - 200f, 0f, 100f, rect2.height);
            if (Widgets.ButtonText(rect6_1, Translator.Translate("Faction")))
            {
                SoundDefOf.Tick_Tiny.PlayOneShotOnCamera();
                List<FloatMenuOption> list = new List<FloatMenuOption>();
                foreach (var f in Find.FactionManager.AllFactionsListForReading)
                    list.Add(new FloatMenuOption(f.Name, delegate
                    {
                        curPawn.SetFaction(f);
                    }));
                Find.WindowStack.Add(new FloatMenu(list));
            }
            UIHighlighter.HighlightOpportunity(rect6_1, "SelectFaction");

            Widgets.InfoCardButton(creationRect.width - 24f, 0f, pawn);

            if (!pawn.health.Dead)
            {
                float num = PawnCardSize.x - 85f;
                if ((pawn.Faction == Faction.OfPlayer || pawn.IsPrisonerOfColony) && pawn.Spawned)
                {
                    Rect rect7 = new Rect(num, 0f, 30f, 30f);
                    TooltipHandler.TipRegion(rect7, PawnBanishUtility.GetBanishButtonTip(pawn));
                    if (Widgets.ButtonImage(rect7, null))
                    {
                        if (pawn.Downed)
                        {
                            Messages.Message("MessageCantBanishDownedPawn".Translate(pawn.LabelShort, pawn).AdjustedFor(pawn), pawn, MessageTypeDefOf.RejectInput, historical: false);
                        }
                        else
                        {
                            PawnBanishUtility.ShowBanishPawnConfirmationDialog(pawn);
                        }
                    }
                    num -= 40f;
                }
                if (pawn.IsColonist)
                {
                    Rect rect8 = new Rect(num, 0f, 30f, 30f);
                    TooltipHandler.TipRegion(rect8, "RenameColonist".Translate());
                    if (Widgets.ButtonImage(rect8, null))
                    {
                        Find.WindowStack.Add(new Dialog_NamePawn(pawn));
                    }
                    num -= 40f;
                }
            }
            string label = pawn.MainDesc(writeAge: true);
            Rect rect9 = new Rect(0f, 45f, rect.width, 60f);
            Widgets.Label(rect9, label);
            TooltipHandler.TipRegion(rect9, () => pawn.ageTracker.AgeTooltipString, 6873641);
            Rect position = new Rect(0f, 100f, 250f, 450f);
            Rect position2 = new Rect(position.xMax, 100f, 258f, 450f);
            GUI.BeginGroup(position);
            float num2 = 0f;
            Text.Font = GameFont.Medium;
            Widgets.Label(new Rect(0f, 0f, 200f, 30f), "Backstory".Translate());
            num2 += 30f;
            Text.Font = GameFont.Small;
            IEnumerator enumerator = Enum.GetValues(typeof(BackstorySlot)).GetEnumerator();
            try
            {
                while (enumerator.MoveNext())
                {
                    BackstorySlot backstorySlot = (BackstorySlot)enumerator.Current;
                    Backstory backstory = pawn.story.GetBackstory(backstorySlot);
                    if (backstory != null)
                    {
                        Rect rect10 = new Rect(0f, num2, position.width, 24f);
                        if (Mouse.IsOver(rect10))
                        {
                            Widgets.DrawHighlight(rect10);
                        }
                        TooltipHandler.TipRegion(rect10, backstory.FullDescriptionFor(pawn));
                        Text.Anchor = TextAnchor.MiddleLeft;
                        string str = (backstorySlot != BackstorySlot.Adulthood) ? "Childhood".Translate() : "Adulthood".Translate();
                        Widgets.Label(rect10, str + ":");
                        Text.Anchor = TextAnchor.UpperLeft;
                        Rect rect11 = new Rect(rect10);
                        rect11.x += 90f;
                        rect11.width -= 90f;
                        string label2 = backstory.TitleCapFor(pawn.gender);
                        if (Widgets.ButtonText(rect11, label2))
                        {
                            List<FloatMenuOption> list = new List<FloatMenuOption>();
                            foreach (var story in BackstoryDatabase.allBackstories.Where(story => story.Value.slot == backstorySlot).ToList())
                            {
                                list.Add(new FloatMenuOption(story.Value.title, delegate
                                {
                                    if (backstorySlot == BackstorySlot.Adulthood)
                                        pawn.story.adulthood = story.Value;
                                    else
                                        pawn.story.childhood = story.Value;

                                    for (int j = 0; j < skillDefsInListOrderCached.Count; j++)
                                    {
                                        SkillDef skillDef = skillDefsInListOrderCached[j];
                                        pawn.skills.GetSkill(skillDef).Notify_SkillDisablesChanged();
                                    }
                                }));
                            }
                            Find.WindowStack.Add(new FloatMenu(list));
                        }
                        //Widgets.Label(rect11, label2);
                        num2 += rect10.height + 2f;
                    }
                }
            }
            finally
            {
                IDisposable disposable;
                if ((disposable = (enumerator as IDisposable)) != null)
                {
                    disposable.Dispose();
                }
            }
            if (pawn.story != null && pawn.story.title != null)
            {
                Rect rect12 = new Rect(0f, num2, position.width, 24f);
                Text.Anchor = TextAnchor.MiddleLeft;
                Widgets.Label(rect12, "Current".Translate() + ":");
                Text.Anchor = TextAnchor.UpperLeft;
                Rect rect13 = new Rect(rect12);
                rect13.x += 90f;
                rect13.width -= 90f;
                Widgets.Label(rect13, pawn.story.title);
                num2 += rect12.height + 2f;
            }
            num2 += 25f;
            Text.Font = GameFont.Medium;
            Widgets.Label(new Rect(0f, num2, 200f, 30f), "IncapableOf".Translate());
            num2 += 30f;
            Text.Font = GameFont.Small;
            StringBuilder stringBuilder = new StringBuilder();
            WorkTags combinedDisabledWorkTags = pawn.story.CombinedDisabledWorkTags;
            if (combinedDisabledWorkTags == WorkTags.None)
            {
                stringBuilder.Append("(" + "NoneLower".Translate() + "), ");
            }
            else
            {
                List<WorkTags> list = WorkTagsFrom(combinedDisabledWorkTags).ToList();
                bool flag2 = true;
                foreach (WorkTags item in list)
                {
                    if (flag2)
                    {
                        stringBuilder.Append(item.LabelTranslated().CapitalizeFirst());
                    }
                    else
                    {
                        stringBuilder.Append(item.LabelTranslated());
                    }
                    stringBuilder.Append(", ");
                    flag2 = false;
                }
            }
            string text = stringBuilder.ToString();
            text = text.Substring(0, text.Length - 2);
            Rect rect14 = new Rect(0f, num2, position.width, 999f);
            Widgets.Label(rect14, text);
            num2 += 100f;
            Text.Font = GameFont.Medium;
            Widgets.Label(new Rect(0f, num2, 200f, 30f), "Traits".Translate());
            num2 += 30f;
            Text.Font = GameFont.Small;
            for (int i = 0; i < pawn.story.traits.allTraits.Count; i++)
            {
                Trait trait = pawn.story.traits.allTraits[i];
                Rect rect15 = new Rect(0f, num2, position.width, 24f);
                if (Mouse.IsOver(rect15))
                {
                    Widgets.DrawHighlight(rect15);
                }
                Widgets.Label(rect15, trait.LabelCap);
                num2 += rect15.height + 2f;
                Trait trLocal = trait;
                TooltipHandler.TipRegion(tip: new TipSignal(() => trLocal.TipString(pawn), (int)num2 * 37), rect: rect15);
            }
            GUI.EndGroup();
            GUI.BeginGroup(position2);
            Text.Font = GameFont.Medium;
            Widgets.Label(new Rect(0f, 0f, 200f, 30f), "Skills".Translate());
            DrawSkillsOf(p: pawn, offset: new Vector2(0f, 35f));
            GUI.EndGroup();
            GUI.EndGroup();
        }

        public void DrawSkillsOf(Pawn p, Vector2 offset)
        {
            Text.Font = GameFont.Small;
            List<SkillDef> allDefsListForReading = DefDatabase<SkillDef>.AllDefsListForReading;
            for (int i = 0; i < allDefsListForReading.Count; i++)
            {
                Vector2 vector = Text.CalcSize(allDefsListForReading[i].skillLabel.CapitalizeFirst());
                float x = vector.x;
                if (x > levelLabelWidth)
                {
                    levelLabelWidth = x;
                }
            }
            for (int j = 0; j < skillDefsInListOrderCached.Count; j++)
            {
                SkillDef skillDef = skillDefsInListOrderCached[j];
                float y = (float)j * 27f + offset.y;
                DrawSkill(p.skills.GetSkill(skillDef), new Vector2(offset.x, y), string.Empty);
            }
        }

        public void DrawSkill(SkillRecord skill, Vector2 topLeft, string tooltipPrefix = "")
        {
            DrawSkill(skill, new Rect(topLeft.x, topLeft.y, 240f, 24f), string.Empty);
        }

        public void DrawSkill(SkillRecord skill, Rect holdingRect, string tooltipPrefix = "")
        {
            if (Mouse.IsOver(holdingRect))
            {
                GUI.DrawTexture(holdingRect, TexUI.HighlightTex);
            }
            GUI.BeginGroup(holdingRect);
            Text.Anchor = TextAnchor.MiddleLeft;
            Rect rect = new Rect(6f, 0f, levelLabelWidth + 6f, holdingRect.height);
            Widgets.Label(rect, skill.def.skillLabel.CapitalizeFirst());
            Rect position = new Rect(rect.xMax, 0f, 24f, 24f);

            if (!skill.TotallyDisabled)
            {
                Texture2D image = null;
                if (skill.passion != Passion.None)
                    image = (skill.passion != Passion.Major) ? PassionMinorIcon : PassionMajorIcon;

                if (Widgets.ButtonImage(position, image))
                {
                    switch (skill.passion)
                    {
                        case Passion.None:
                            skill.passion = Passion.Minor;
                            break;
                        case Passion.Minor:
                            skill.passion = Passion.Major;
                            break;
                        case Passion.Major:
                            skill.passion = Passion.None;
                            break;
                    }
                }
            }
                //GUI.DrawTexture(position, image);
            if (!skill.TotallyDisabled)
            {
                Rect rect2 = new Rect(position.xMax, 0f, holdingRect.width - position.xMax, holdingRect.height);
                float fillPercent = Mathf.Max(0.01f, (float)skill.Level / 20f);
                Widgets.FillableBar(rect2, fillPercent, SkillBarFillTex, null, doBorder: false);
            }
            Rect rect3 = new Rect(position.xMax + 4f, 0f, 80f, holdingRect.height);
            rect3.yMin += 3f;
            
            string label;
            if (skill.TotallyDisabled)
            {
                GUI.color = DisabledSkillColor;
                label = "-";
                Widgets.Label(rect3, label);
            }
            else
            {
                GUI.color = DisabledSkillColor;
                DoIntInputRect(rect3, ref skill.levelInt);
                //label = skill.Level.ToStringCached();
            }
            
            GenUI.SetLabelAlign(TextAnchor.MiddleLeft);
            GenUI.ResetLabelAlign();
            GUI.color = Color.white;
            GUI.EndGroup();
            string text = GetSkillDescription(skill);
            if (tooltipPrefix != string.Empty)
            {
                text = tooltipPrefix + "\n\n" + text;
            }
            TooltipHandler.TipRegion(holdingRect, new TipSignal(text, skill.def.GetHashCode() * 397945));
        }

        private string GetSkillDescription(SkillRecord sk)
        {
            StringBuilder stringBuilder = new StringBuilder();
            if (sk.TotallyDisabled)
            {
                stringBuilder.Append("DisabledLower".Translate().CapitalizeFirst());
            }
            else
            {
                stringBuilder.AppendLine("Level".Translate() + " " + sk.Level + ": " + sk.LevelDescriptor);
                string text = (sk.Level != 20) ? "ProgressToNextLevel".Translate() : "Experience".Translate();
                stringBuilder.AppendLine(text + ": " + sk.xpSinceLastLevel.ToString("F0") + " / " + sk.XpRequiredForLevelUp);
                stringBuilder.Append("Passion".Translate() + ": ");
                switch (sk.passion)
                {
                    case Passion.None:
                        stringBuilder.Append("PassionNone".Translate(0.35f.ToStringPercent("F0")));
                        break;
                    case Passion.Minor:
                        stringBuilder.Append("PassionMinor".Translate(1f.ToStringPercent("F0")));
                        break;
                    case Passion.Major:
                        stringBuilder.Append("PassionMajor".Translate(1.5f.ToStringPercent("F0")));
                        break;
                }
                if (sk.LearningSaturatedToday)
                {
                    stringBuilder.AppendLine();
                    stringBuilder.Append("LearnedMaxToday".Translate(sk.xpSinceMidnight.ToString("F0"), 4000, 0.2f.ToStringPercent("F0")));
                }
            }
            stringBuilder.AppendLine();
            stringBuilder.AppendLine();
            stringBuilder.Append(sk.def.description);
            return stringBuilder.ToString();
        }

        public void DoNameInputRect(Rect rect, ref string name, int maxLength)
        {
            string text = Widgets.TextField(rect, name);
            if (text.Length <= maxLength && ValidNameRegex.IsMatch(text))
            {
                name = text;
            }
        }

        public void DoIntInputRect(Rect rect, ref int value)
        {
            string buff = value.ToString();
            Widgets.TextFieldNumeric(rect, ref value, ref buff, max: 20);
            /*string text = Widgets.TextField(rect, value.ToString());
            
            if (text.Length <= maxLength && ValidIntRegex.IsMatch(text))
            {
                int.TryParse(text, out value);
            }
            */
        }

        private IEnumerable<WorkTags> WorkTagsFrom(WorkTags tags)
        {
            foreach (WorkTags workTag in tags.GetAllSelectedItems<WorkTags>())
            {
                if (workTag != 0)
                {
                    yield return workTag;
                }
            }
        }
    }
}
