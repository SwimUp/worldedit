using RimWorld;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using Verse;
using Verse.Sound;
using WorldEdit.Interfaces;

namespace WorldEdit.Editor
{
    internal class PawnMenu : Page
    {
        private class AddTraitMenu : FWindow
        {
            public override Vector2 InitialSize => new Vector2(250, 150);
            private TraitDef trait = DefDatabase<TraitDef>.GetRandom();
            private Pawn pawn;

            IntRange range = new IntRange();

            public AddTraitMenu(Pawn pawn)
            {
                resizeable = false;

                this.pawn = pawn;
            }

            public override void DoWindowContents(Rect inRect)
            {
                Widgets.Label(new Rect(0, 15, 70, 20), Translator.Translate("TraitLabel"));
                if(Widgets.ButtonText(new Rect(80, 15, 160, 20), trait.defName))
                {
                    List<FloatMenuOption> list = new List<FloatMenuOption>();
                    foreach (var trait in DefDatabase<TraitDef>.AllDefsListForReading)
                    {
                        list.Add(new FloatMenuOption(trait.defName, delegate
                        {
                            this.trait = trait;
                        }));
                    }
                    Find.WindowStack.Add(new FloatMenu(list));
                }

                Widgets.IntRange(new Rect(5, 50, 230, 40), 454325, ref range, 0, 2, Translator.Translate("DegreeLabel"));

                if (Widgets.ButtonText(new Rect(0, 110, 240, 20), Translator.Translate("AddNewTrait")))
                {
                    AddTrait();
                }

            }

            private void AddTrait()
            {
                pawn.story.traits.GainTrait(new Trait(trait, range.max));

                Close();
            }
        }

        private class HealthMenu : FWindow
        {
            private Vector2 scrollPosition = Vector2.zero;

            private bool highlight = true;

            private bool showAllHediffs = false;

            public const float TopPadding = 20f;

            private readonly Color HighlightColor = new Color(0.5f, 0.5f, 0.5f, 1f);

            private readonly Color StaticHighlightColor = new Color(0.75f, 0.75f, 0.85f, 1f);

            private readonly Texture2D BleedingIcon = ContentFinder<Texture2D>.Get("UI/Icons/Medical/Bleeding");

            public override Vector2 InitialSize => new Vector2(400, 520);
            private int heddiffCount = 0;
            private Pawn pawn;

            private BodyPartRecord bodyPart = null;
            private HediffStage hediffStage = null;
            private HediffDef hediffDef = null;
            private DamageDef damageType = null;

            private string buffAmount = string.Empty;
            private float damageAmount = 0;

            private string buffSever = string.Empty;
            private float sevAmount = 0;

            public HealthMenu(Pawn p)
            {
                pawn = p;
                resizeable = false;

                RecalculateHeight();

                bodyPart = pawn.health.hediffSet.GetNotMissingParts().RandomElement();
                hediffDef = DefDatabase<HediffDef>.GetRandom();
                hediffStage = hediffDef.stages.RandomElement();
                buffSever = $"{hediffStage.minSeverity}";
                sevAmount = hediffStage.minSeverity;

                damageType = DefDatabase<DamageDef>.GetRandom();
            }

            public override void DoWindowContents(Rect inRect)
            {
                Widgets.Label(new Rect(0, 5, 150, 20), Translator.Translate("HeddifsCurrent"));

                if (Widgets.ButtonText(new Rect(150, 5, 210, 20), Translator.Translate("FullHeal")))
                {
                    FullHeal();
                }

                int defSize = heddiffCount;
                Rect scrollRectFact = new Rect(0, 30, 390, 200);
                Rect scrollVertRectFact = new Rect(0, 0, scrollRectFact.x, defSize);
                Widgets.BeginScrollView(scrollRectFact, ref scrollPosition, scrollVertRectFact);
                float curY = 0f;
                foreach (IGrouping<BodyPartRecord, Hediff> item in VisibleHediffGroupsInOrder(pawn, true))
                {
                    DrawHediffRow(inRect, pawn, item, ref curY);
                }
                Widgets.EndScrollView();

                Widgets.Label(new Rect(0, 250, 120, 20), Translator.Translate("BodyPartInfo"));
                if (Widgets.ButtonText(new Rect(110, 250, 270, 20), bodyPart.LabelCap))
                {
                    List<FloatMenuOption> list = new List<FloatMenuOption>();
                    foreach (var p in pawn.health.hediffSet.GetNotMissingParts())
                    {
                        list.Add(new FloatMenuOption(p.LabelCap, delegate
                        {
                            bodyPart = p;
                        }));
                    }
                    Find.WindowStack.Add(new FloatMenu(list));
                }

                Widgets.DrawLineHorizontal(0, 280, 400);

                Widgets.Label(new Rect(0, 290, 120, 20), Translator.Translate("HeddifDefInfo"));
                if (Widgets.ButtonText(new Rect(110, 290, 270, 20), hediffDef.LabelCap))
                {
                    List<FloatMenuOption> list = new List<FloatMenuOption>();
                    foreach (var h in DefDatabase<HediffDef>.AllDefsListForReading)
                    {
                        list.Add(new FloatMenuOption(h.LabelCap, delegate
                        {
                            hediffDef = h;
                        }));
                    }
                    Find.WindowStack.Add(new FloatMenu(list));
                }
                Widgets.Label(new Rect(0, 315, 120, 20), Translator.Translate("HeddifStageInfo"));
                if (Widgets.ButtonText(new Rect(110, 315, 270, 20), hediffStage.label))
                {
                    if (hediffDef != null || hediffDef.stages.Count > 1)
                    {
                        List<FloatMenuOption> list = new List<FloatMenuOption>();
                        foreach (var stage in hediffDef.stages)
                        {
                            list.Add(new FloatMenuOption(stage.label, delegate
                            {
                                buffSever = $"{stage.minSeverity}";
                                sevAmount = stage.minSeverity;
                                hediffStage = stage;
                            }));
                        }
                        Find.WindowStack.Add(new FloatMenu(list));
                    }
                }

                Widgets.Label(new Rect(0, 340, 120, 20), Translator.Translate("SeverityInfo"));
                Widgets.TextFieldNumeric(new Rect(130, 340, 240, 20), ref sevAmount, ref buffSever, 0, 1);

                if (Widgets.ButtonText(new Rect(0, 370, 390, 20), Translator.Translate("AddNewHediffLabel")))
                {
                    AddNewHediff();
                }

                Widgets.DrawLineHorizontal(0, 405, 400);

                Widgets.Label(new Rect(0, 420, 120, 20), Translator.Translate("DamageType"));
                if (Widgets.ButtonText(new Rect(110, 420, 270, 20), damageType.LabelCap))
                {
                    List<FloatMenuOption> list = new List<FloatMenuOption>();
                    foreach (var dmg in DefDatabase<DamageDef>.AllDefsListForReading)
                    {
                        list.Add(new FloatMenuOption(dmg.LabelCap, delegate
                        {
                            damageType = dmg;
                        }));
                    }
                    Find.WindowStack.Add(new FloatMenu(list));
                }

                Widgets.Label(new Rect(0, 445, 120, 20), Translator.Translate("DamageAmount"));
                Widgets.TextFieldNumeric(new Rect(130, 445, 240, 20), ref damageAmount, ref buffAmount, 0);

                if (Widgets.ButtonText(new Rect(0, 470, 390, 20), Translator.Translate("AddDamageLabel")))
                {
                    AddDamage();
                }
            }

            private void FullHeal()
            {
                foreach (BodyPartRecord notMissingPart in pawn.health.hediffSet.GetNotMissingParts())
                {
                    pawn.health.RestorePart(notMissingPart);
                }

                RecalculateHeight();
            }

            private void AddDamage()
            {
                DamageInfo info = new DamageInfo(damageType, damageAmount, hitPart: bodyPart);

                pawn.TakeDamage(info);

                RecalculateHeight();
            }

            private void AddNewHediff()
            {
                Hediff hediff = HediffMaker.MakeHediff(hediffDef, pawn, bodyPart);

                hediff.Severity = sevAmount;

                pawn.health.AddHediff(hediff);

                RecalculateHeight();
            }

            private void DrawHediffRow(Rect rect, Pawn pawn, IEnumerable<Hediff> diffs, ref float curY)
            {
                float num = rect.width * 0.375f;
                float width = rect.width - num - 20f;
                BodyPartRecord part = diffs.First().Part;
                float a = (part != null) ? Text.CalcHeight(part.LabelCap, num) : Text.CalcHeight("WholeBody".Translate(), num);
                float num2 = 0f;
                float num3 = curY;
                float num4 = 0f;
                foreach (IGrouping<int, Hediff> item in from x in diffs
                                                        group x by x.UIGroupKey)
                {
                    int num5 = item.Count();
                    string text = item.First().LabelCap;
                    if (num5 != 1)
                    {
                        text = text + " x" + num5.ToString();
                    }
                    num4 += Text.CalcHeight(text, width);
                }
                num2 = num4;
                Rect rect2 = new Rect(0f, curY, rect.width, Mathf.Max(a, num2));
                DoRightRowHighlight(rect2);
                if (part != null)
                {
                    GUI.color = HealthUtility.GetPartConditionLabel(pawn, part).Second;
                    Widgets.Label(new Rect(0f, curY, num, 100f), part.LabelCap);
                }
                else
                {
                    GUI.color = HealthUtility.DarkRedColor;
                    Widgets.Label(new Rect(0f, curY, num, 100f), "WholeBody".Translate());
                }
                GUI.color = Color.white;
                foreach (IGrouping<int, Hediff> item2 in from x in diffs
                                                         group x by x.UIGroupKey)
                {
                    int num6 = 0;
                    Hediff hediff = null;
                    Texture2D texture2D = null;
                    TextureAndColor textureAndColor = null;
                    float num7 = 0f;
                    foreach (Hediff item3 in item2)
                    {
                        if (num6 == 0)
                        {
                            hediff = item3;
                        }
                        textureAndColor = item3.StateIcon;
                        if (item3.Bleeding)
                        {
                            texture2D = BleedingIcon;
                        }
                        num7 += item3.BleedRate;
                        num6++;
                    }
                    string text2 = hediff.LabelCap;
                    if (num6 != 1)
                    {
                        text2 = text2 + " x" + num6.ToStringCached();
                    }
                    GUI.color = hediff.LabelColor;
                    float num8 = Text.CalcHeight(text2, width);
                    Rect rect3 = new Rect(num, curY, width, num8);
                    Widgets.Label(rect3, text2);
                    GUI.color = Color.white;
                    Rect rect4 = new Rect(rect2.xMax - 20f, curY, 20f, 20f);
                    if ((bool)texture2D)
                    {
                        Rect position = rect4.ContractedBy(GenMath.LerpDouble(0f, 0.6f, 5f, 0f, Mathf.Min(num7, 1f)));
                        GUI.DrawTexture(position, texture2D);
                        rect4.x -= rect4.width;
                    }
                    if (textureAndColor.HasValue)
                    {
                        GUI.color = textureAndColor.Color;
                        GUI.DrawTexture(rect4, textureAndColor.Texture);
                        GUI.color = Color.white;
                        rect4.x -= rect4.width;
                    }
                    curY += num8;

                    if (Widgets.ButtonText(new Rect(rect3.x + 205f, rect3.y, 20, 20), "X"))
                    {
                        pawn.health.RemoveHediff(hediff);
                        RecalculateHeight();
                    }
                }
                GUI.color = Color.white;
                curY = num3 + Mathf.Max(a, num2);
                /*
                if (Widgets.ButtonInvisible(rect2))
                {
                    EntryClicked(diffs, pawn);
                }
                TooltipHandler.TipRegion(rect2, new TipSignal(() => GetTooltip(diffs, pawn, part), (int)curY + 7857));
                */
            }

            private void RecalculateHeight() => heddiffCount = VisibleHediffGroupsInOrder(pawn, true).Count() * 40;

            private IEnumerable<IGrouping<BodyPartRecord, Hediff>> VisibleHediffGroupsInOrder(Pawn pawn, bool showBloodLoss)
            {
                foreach (IGrouping<BodyPartRecord, Hediff> item in from x in VisibleHediffs(pawn, showBloodLoss)
                                                                   group x by x.Part into x
                                                                   orderby GetListPriority(x.First().Part) descending
                                                                   select x)
                {
                    yield return item;
                }
            }

            private float GetListPriority(BodyPartRecord rec)
            {
                if (rec == null)
                {
                    return 9999999f;
                }
                return (float)((int)rec.height * 10000) + rec.coverageAbsWithChildren;
            }

            private IEnumerable<Hediff> VisibleHediffs(Pawn pawn, bool showBloodLoss)
            {
                if (!showAllHediffs)
                {
                    List<Hediff_MissingPart> mpca = pawn.health.hediffSet.GetMissingPartsCommonAncestors();
                    for (int i = 0; i < mpca.Count; i++)
                    {
                        yield return mpca[i];
                    }
                    IEnumerable<Hediff> visibleDiffs = pawn.health.hediffSet.hediffs.Where(delegate (Hediff d)
                    {
                        if (d is Hediff_MissingPart)
                        {
                            return false;
                        }
                        if (!d.Visible)
                        {
                            return false;
                        }
                        return (!showBloodLoss && d.def == HediffDefOf.BloodLoss) ? false : true;
                    });
                    foreach (Hediff item in visibleDiffs)
                    {
                        yield return item;
                    }
                }
                else
                {
                    foreach (Hediff hediff in pawn.health.hediffSet.hediffs)
                    {
                        yield return hediff;
                    }
                }
            }

            private void DoRightRowHighlight(Rect rowRect)
            {
                if (highlight)
                {
                    GUI.color = StaticHighlightColor;
                    GUI.DrawTexture(rowRect, TexUI.HighlightTex);
                }
                highlight = !highlight;
                if (Mouse.IsOver(rowRect))
                {
                    GUI.color = HighlightColor;
                    GUI.DrawTexture(rowRect, TexUI.HighlightTex);
                }
            }
        }

        private class RelationsMenu : FWindow
        {
            public override Vector2 InitialSize => new Vector2(400, 140);

            private PawnRelationDef relationType = null;

            private Faction faction = null;

            private Pawn pawnToRelation = null;
            private Pawn parentPawn = null;

            private FactionManager rimfactionManager = Find.FactionManager;

            private List<Faction> getFactionList => Find.FactionManager.AllFactionsListForReading.Where(f =>
            rimfactionManager.OfMechanoids.def != f.def && rimfactionManager.OfInsects.def != f.def &&
            rimfactionManager.OfAncientsHostile.def != f.def && rimfactionManager.OfAncients.def != f.def).ToList();

            public RelationsMenu(Pawn p)
            {
                resizeable = false;
                
                relationType = DefDatabase<PawnRelationDef>.GetRandom();
                faction = getFactionList.RandomElement();

                pawnToRelation = (from x in PawnsFinder.AllMapsWorldAndTemporary_AliveOrDead
                                  where x.Faction == faction
                                  select x).RandomElement();

                parentPawn = p;
            }

            public override void DoWindowContents(Rect inRect)
            {
                Widgets.Label(new Rect(0, 10, 120, 20), Translator.Translate("RelationType"));
                if (Widgets.ButtonText(new Rect(110, 10, 270, 20), relationType.LabelCap))
                {
                    List<FloatMenuOption> list = new List<FloatMenuOption>();
                    foreach (var rDef in DefDatabase<PawnRelationDef>.AllDefsListForReading)
                    {
                        list.Add(new FloatMenuOption(rDef.LabelCap, delegate
                        {
                            relationType = rDef;
                        }));
                    }
                    Find.WindowStack.Add(new FloatMenu(list));
                }

                Widgets.Label(new Rect(0, 40, 120, 20), Translator.Translate("FactionForSort"));
                if (Widgets.ButtonText(new Rect(110, 40, 270, 20), faction.Name))
                {
                    List<FloatMenuOption> list = new List<FloatMenuOption>();
                    foreach (var f in getFactionList)
                    {
                        list.Add(new FloatMenuOption(f.Name, delegate
                        {
                            faction = f;
                        }));
                    }
                    Find.WindowStack.Add(new FloatMenu(list));
                }

                Widgets.Label(new Rect(0, 70, 120, 20), Translator.Translate("PawnToSelect"));
                if (Widgets.ButtonText(new Rect(110, 70, 270, 20), pawnToRelation.Name.ToStringFull))
                {
                    List<FloatMenuOption> list = new List<FloatMenuOption>();
                    foreach (var p in from x in PawnsFinder.AllMapsWorldAndTemporary_AliveOrDead
                                      where x.Faction == faction select x)
                    {
                        list.Add(new FloatMenuOption(p.Name.ToStringShort, delegate
                        {
                            pawnToRelation = p;
                        }));
                    }
                    Find.WindowStack.Add(new FloatMenu(list));
                }

                if (Widgets.ButtonText(new Rect(0, 100, 390, 20), Translator.Translate("AddNewRelationToPawn")))
                {
                    AddNewRelationToPawn();
                }

                if (Widgets.ButtonText(new Rect(0, 130, 390, 20), Translator.Translate("AddNewRelationToPawn")))
                {
                    AddNewRelationToPawn2();
                }
            }

            private void AddNewRelationToPawn()
            {
                
                var req = new PawnGenerationRequest(pawnToRelation.kindDef, pawnToRelation.Faction);
                relationType.Worker.CreateRelation(parentPawn, pawnToRelation, ref req);
            }

            private void AddNewRelationToPawn2()
            {

                var req = new PawnGenerationRequest(parentPawn.kindDef, parentPawn.Faction);
                relationType.Worker.CreateRelation(parentPawn, pawnToRelation, ref req);
            }
        }

        private Pawn curPawn;

        private readonly Vector2 PawnPortraitSize = new Vector2(100f, 140f);

        private readonly Vector2 PawnSelectorPortraitSize = new Vector2(70f, 110f);

        private Vector2 scroll = Vector2.zero;

        public const int MainRectsY = 100;

        public Vector2 PawnCardSize = new Vector2(570f, 470f);

        public const int MaxNickLength = 16;

        public const int MaxTitleLength = 25;

        public Regex ValidNameRegex = new Regex("^[\\p{L}0-9 '\\-]*$");

        private static float levelLabelWidth = -1f;

        private static List<SkillDef> skillDefsInListOrderCached;

        public const float SkillHeight = 24f;

        public const float SkillYSpacing = 3f;

        private readonly Color DisabledSkillColor = new Color(1f, 1f, 1f, 0.5f);

        private Texture2D PassionMinorIcon = ContentFinder<Texture2D>.Get("UI/Icons/PassionMinor");

        private Texture2D PassionMajorIcon = ContentFinder<Texture2D>.Get("UI/Icons/PassionMajor");

        private Texture2D SkillBarFillTex = SolidColorMaterials.NewSolidColorTexture(new Color(1f, 1f, 1f, 0.1f));

        public override string PageTitle => "CreateCharacters".Translate();

        private List<Pawn> allPawns = new List<Pawn>();
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
            allPawns.Add(GeneratePawn());
            // allPawns.Add(DownedRefugeeQuestUtility.GenerateRefugee(-1));
        }

        private Pawn GeneratePawn()
        {
            PawnKindDef spaceRefugee = PawnKindDefOf.SpaceRefugee;
            Faction randomFactionForRefugee = GetRandomFactionForRefugee();
            PawnGenerationRequest request = new PawnGenerationRequest(spaceRefugee, randomFactionForRefugee, PawnGenerationContext.NonPlayer, -1, forceGenerateNewPawn: false, newborn: false, allowDead: false, allowDowned: false, canGeneratePawnRelations: true, mustBeCapableOfViolence: false, 20f, forceAddFreeWarmLayerIfNeeded: true, allowGay: true, allowFood: true, inhabitant: false, certainlyBeenInCryptosleep: false, forceRedressWorldPawnIfFormerColonist: false, worldPawnFactionDoesntMatter: false, null, null, 0.2f);
            Pawn pawn = PawnGenerator.GeneratePawn(request);

            return pawn;
        }

        private Faction GetRandomFactionForRefugee()
        {
            if (Rand.Chance(0.6f) && Find.FactionManager.TryGetRandomNonColonyHumanlikeFaction(out Faction faction, tryMedievalOrBetter: true))
            {
                return faction;
            }
            return null;
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
            if (Widgets.ButtonText(new Rect(rect3.x + 130f, rect3.y - 10f, 160f, 40f), Translator.Translate("ModifyHealth")))
            {
                Find.WindowStack.Add(new HealthMenu(curPawn));
            }
            Text.Font = GameFont.Small;
            rect3.yMin += 35f;
            HealthCardUtility.DrawHediffListing(rect3, curPawn, showBloodLoss: true);
            Rect rect4 = new Rect(rect3.x, rect3.yMax, rect3.width, 200f);
            Text.Font = GameFont.Medium;
            Widgets.Label(rect4, "Relations".Translate());
            if (Widgets.ButtonText(new Rect(rect4.x + 130f, rect4.y - 10f, 160f, 40f), Translator.Translate("ModifyRelations")))
            {
                Find.WindowStack.Add(new RelationsMenu(curPawn));
            }
            Text.Font = GameFont.Small;
            rect4.yMin += 35f;
            SocialCardUtility.DrawRelationsAndOpinions(rect4, curPawn);
        }

        private void RandomizeCurPawn()
        {
            curPawn = GeneratePawn();
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

                                    MethodInfo cache = typeof(Pawn_StoryTracker).GetMethod("Notify_TraitChanged", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                                    if(cache != null)
                                    {
                                        cache.Invoke(pawn.story, null);
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
            if(Widgets.ButtonText(new Rect(0f, num2, 200f, 30f), "Traits".Translate()))
            {
                Find.WindowStack.Add(new AddTraitMenu(pawn));
            }
            //Widgets.Label(new Rect(0f, num2, 200f, 30f), "Traits".Translate());
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
                if (Widgets.ButtonText(new Rect(position.width - 30f, num2, 20f, 24f), "X"))
                {
                    pawn.story.traits.allTraits.Remove(trait);
                }
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
