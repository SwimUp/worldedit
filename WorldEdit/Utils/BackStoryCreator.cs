using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using WorldEdit;
using WorldEdit.Editor;
using WorldEdit.Interfaces;

namespace WorldEdits
{
    public class TraitData : IExposable
    {
        public TraitEntry trait;
        public TraitDegreeData degreeData;

        public TraitDef traitDef;
        public int degreeInt = 0;
        public int status = 0; //0 doesntmatter, 1 forced, 2 dissalow

        public TraitData()
        {

        }

        public TraitData(TraitEntry entry, int status)
        {
            traitDef = entry.def;
            degreeInt = entry.degree;
            this.status = status;
            trait = entry;
        }

        public TraitData(TraitEntry entry, TraitDegreeData data, int status)
        {
            traitDef = entry.def;
            degreeInt = entry.degree;
            this.status = status;
            trait = entry;
            degreeData = data;
        }

        public void ExposeData()
        {
            Scribe_Defs.Look(ref traitDef, "t_def");
            Scribe_Values.Look(ref degreeInt, "t_degree");
            Scribe_Values.Look(ref status, "t_status");
        }
    }


    internal class BackStoryCreator : FWindow
    {
        private class SkillsMenu : FWindow
        {
            public override Vector2 InitialSize => new Vector2(270, 130);

            private Dictionary<SkillDef, int> tmpCollection;

            private SkillDef skill;

            private string buff = string.Empty;
            private int count = 0;

            public SkillsMenu(Dictionary<SkillDef, int> dict)
            {
                resizeable = false;
                tmpCollection = dict;
                skill = DefDatabase<SkillDef>.AllDefsListForReading[0];
            }

            public override void DoWindowContents(Rect inRect)
            {
                Widgets.Label(new Rect(0, 15, 70, 20), Translator.Translate("BackstorySlotInfo"));
                if (Widgets.ButtonText(new Rect(80, 15, 175, 20), skill.LabelCap))
                {
                    List<FloatMenuOption> list = new List<FloatMenuOption>();
                    foreach (var skill in DefDatabase<SkillDef>.AllDefsListForReading)
                    {
                        list.Add(new FloatMenuOption(skill.LabelCap, delegate
                        {
                            this.skill = skill;
                        }));
                    }
                    Find.WindowStack.Add(new FloatMenu(list));
                }

                Widgets.Label(new Rect(0, 50, 70, 20), Translator.Translate("CountInfo"));
                Widgets.TextFieldNumeric(new Rect(80, 50, 175, 20), ref count, ref buff, -20, 20);

                if (Widgets.ButtonText(new Rect(0, 90, 260, 20), Translator.Translate("AddNewSkillGain")))
                {
                    AddNewSkill();
                }
            }

            private void AddNewSkill()
            {
                if (!tmpCollection.Keys.Contains(skill))
                {
                    Messages.Message($"Success", MessageTypeDefOf.NeutralEvent);
                    tmpCollection.Add(skill, count);
                }
                else
                {
                    Messages.Message($"Has already", MessageTypeDefOf.NeutralEvent);
                }
            }
        }

        public override Vector2 InitialSize => new Vector2(1235, 560);

        public Vector2 scrollMaintext = Vector2.zero;

        private Vector2 scroll = Vector2.zero;
        private Vector2 scroll2 = Vector2.zero;
        private Vector2 scroll3 = Vector2.zero;

        private Dictionary<WorkTags, int> workStatus = new Dictionary<WorkTags, int>();

        private Backstory story = null;

        private bool edit = false;

        private List<TraitData> traitEntrys = new List<TraitData>();

        private bool onlyCreate = false;

        public BackStoryCreator()
        {
            resizeable = false;
            onlyCreate = false;

            CreateNew();
        }

        public BackStoryCreator(bool onlyCreate)
        {
            resizeable = false;

            CreateNew();

            this.onlyCreate = onlyCreate;
        }

        private void CreateNew()
        {
            workStatus.Clear();

            foreach (WorkTags tag in Enum.GetValues(typeof(WorkTags)))
            {
                workStatus.Add(tag, 0);
            }

            traitEntrys.Clear();

            story = new Backstory();
        }

        public BackStoryCreator(Backstory b)
        {
            resizeable = false;
            onlyCreate = false;

            story = b;

            workStatus.Clear();
            traitEntrys.Clear();

            foreach (WorkTags tag in Enum.GetValues(typeof(WorkTags)))
            {
                if ((b.workDisables & tag) == tag)
                    workStatus.Add(tag, 1);
                else
                if ((b.requiredWorkTags & tag) == tag)
                    workStatus.Add(tag, 2);
                else
                    workStatus.Add(tag, 0);
            }

            if (story.forcedTraits != null)
            {
                foreach (var t in story.forcedTraits)
                {
                    TraitDegreeData data = t.def.degreeDatas[t.degree];
                    traitEntrys.Add(new TraitData(t, data, 1));
                }
            }

            if (story.disallowedTraits != null)
            {
                foreach (var t in story.disallowedTraits)
                {
                    TraitDegreeData data = t.def.degreeDatas[t.degree];
                    traitEntrys.Add(new TraitData(t, data, 2));
                }
            }

            edit = true;
        }
        public override void DoWindowContents(Rect inRect)
        {
            Text.Font = GameFont.Small;

            Widgets.Label(new Rect(0, 15, 70, 20), Translator.Translate("BackstorySlotInfo"));
            if (Widgets.ButtonText(new Rect(80, 15, 250, 20), (story.slot == BackstorySlot.Adulthood) ? "Adulthood".Translate() : "Childhood".Translate()))
            {
                List<FloatMenuOption> list = new List<FloatMenuOption>()
                {
                    new FloatMenuOption("Childhood".Translate(), delegate
                    {
                        story.slot = BackstorySlot.Childhood;
                    }),
                    new FloatMenuOption("Adulthood".Translate(), delegate
                    {
                        story.slot = BackstorySlot.Adulthood;
                    })
                };
                Find.WindowStack.Add(new FloatMenu(list));
            }

            Widgets.Label(new Rect(0, 50, 170, 20), Translator.Translate("BackstoryTitle"));
            story.title = Widgets.TextField(new Rect(180, 50, 300, 20), story.title);

            Widgets.Label(new Rect(0, 80, 170, 20), Translator.Translate("BackstoryTitleFemale"));
            story.titleFemale = Widgets.TextField(new Rect(180, 80, 300, 20), story.titleFemale);

            Widgets.Label(new Rect(0, 110, 170, 20), Translator.Translate("BackstoryTitleShort"));
            story.titleShort = Widgets.TextField(new Rect(180, 110, 300, 20), story.titleShort);

            Widgets.Label(new Rect(0, 140, 170, 20), Translator.Translate("BackstoryTitleShortFemale"));
            story.titleShortFemale = Widgets.TextField(new Rect(180, 140, 300, 20), story.titleShortFemale);

            Widgets.Label(new Rect(0, 170, 170, 20), Translator.Translate("DescriptionBasic"));
            story.baseDesc = Widgets.TextAreaScrollable(new Rect(0, 195, 490, 290), story.baseDesc, ref scrollMaintext);

            Widgets.Label(new Rect(510, 15, 280, 20), Translator.Translate("GainsPerksInfo"));
            int size = story.skillGainsResolved.Count * 25;
            Rect scrollRectFact = new Rect(510, 45, 300, 200);
            Rect scrollVertRectFact = new Rect(0, 0, scrollRectFact.x, size);
            Widgets.BeginScrollView(scrollRectFact, ref scroll, scrollVertRectFact);
            int x = 0;
            for (int i = 0; i < story.skillGainsResolved.Count; i++)
            {
                KeyValuePair<SkillDef, int> gainedSkill = story.skillGainsResolved.ElementAt(i);
                Widgets.Label(new Rect(0, x, 170, 20), $"{gainedSkill.Key.LabelCap}: {gainedSkill.Value}");
                if (Widgets.ButtonText(new Rect(180, x, 110, 20), Translator.Translate("DeleteTargetRelation")))
                {
                    story.skillGainsResolved.Remove(gainedSkill.Key);
                }
                x += 22;
            }
            Widgets.EndScrollView();

            if (Widgets.ButtonText(new Rect(510, 255, 300, 20), Translator.Translate("AddNewSkillGained")))
            {
                Find.WindowStack.Add(new SkillsMenu(story.skillGainsResolved));
            }

            int size2 = Enum.GetValues(typeof(WorkTags)).Length * 25;
            Rect scrollRectFact2 = new Rect(510, 290, 300, 200);
            Rect scrollVertRectFact2 = new Rect(0, 0, scrollRectFact2.x, size2);
            Widgets.BeginScrollView(scrollRectFact2, ref scroll2, scrollVertRectFact2);
            x = 0;
            for (int i = 0; i < workStatus.Count; i++)
            {
                var tag = workStatus.Keys.ElementAt(i);
                var value = workStatus.Values.ElementAt(i);

                Widgets.Label(new Rect(0, x, 150, 20), tag.ToString());
                if (Widgets.ButtonText(new Rect(160, x, 120, 20), TranslateSkillStatus(value)))
                {
                    switch (value)
                    {
                        case 0:
                            {
                                workStatus[tag] = 1;
                                break;
                            }
                        case 1:
                            {
                                workStatus[tag] = 2;
                                break;
                            }
                        case 2:
                            {
                                workStatus[tag] = 0;
                                break;
                            }
                    }
                }

                x += 22;
            }
            Widgets.EndScrollView();

            Widgets.Label(new Rect(820, 15, 170, 20), Translator.Translate("TraitsData"));
            int size3 = traitEntrys.Count * 25;
            Rect scrollRectFact3 = new Rect(820, 40, 380, 470);
            Rect scrollVertRectFact3 = new Rect(0, 0, scrollRectFact3.x, size3);
            Widgets.BeginScrollView(scrollRectFact3, ref scroll3, scrollVertRectFact3);
            x = 0;
            for (int i = 0; i < traitEntrys.Count; i++)
            {
                TraitData trt = traitEntrys[i];
                Widgets.Label(new Rect(0, x, 150, 20), $"{trt.degreeData.label}");
                if (Widgets.ButtonText(new Rect(160, x, 160, 20), TranslateTraitStatus(trt.status)))
                {
                    switch (trt.status)
                    {
                        case 1:
                            {
                                traitEntrys[i].status = 2;
                                break;
                            }
                        case 2:
                            {
                                traitEntrys[i].status = 1;
                                break;
                            }
                    }
                }
                if (Widgets.ButtonText(new Rect(340, x, 20, 20), "X"))
                {
                    traitEntrys.Remove(trt);
                }
                x += 22;
            }
            Widgets.EndScrollView();

            if (Widgets.ButtonText(new Rect(820, 520, 380, 20), Translator.Translate("AddNewTrait")))
            {
                List<FloatMenuOption> list = new List<FloatMenuOption>();
                foreach (var tr in DefDatabase<TraitDef>.AllDefsListForReading)
                {
                    for (int i = 0; i < tr.degreeDatas.Count; i++)
                    {
                        TraitDegreeData deg = tr.degreeDatas[i];
                        list.Add(new FloatMenuOption(deg.label, delegate
                        {
                            traitEntrys.Add(new TraitData(new TraitEntry(tr, deg.degree), deg, 1));
                        }));
                    }
                }
                Find.WindowStack.Add(new FloatMenu(list));
            }

            if (Widgets.ButtonText(new Rect(0, 520, 810, 20), Translator.Translate("SaveBacktoryToWorld")))
            {
                SaveBacktoryToWorld();
            }
        }

        private string TranslateSkillStatus(int state)
        {
            if(state == 0)
            {
                return Translator.Translate("DoesntMatter");
            }

            return state == 1 ? Translator.Translate("BlockTag") : Translator.Translate("Required");
        }

        private string TranslateTraitStatus(int state)
        {
            if (state == 0)
            {
                return Translator.Translate("DoesntMatter");
            }

            return state == 1 ? Translator.Translate("ForcedTag") : Translator.Translate("DissalowTrait");
        }

        private void SaveBacktoryToWorld()
        {
            WorkTags blockTags = WorkTags.None;
            WorkTags reqTags = WorkTags.None;
            foreach (var tag in workStatus)
            {
                if (tag.Value == 1)
                {
                    blockTags |= tag.Key;
                }
                else if (tag.Value == 2)
                {
                    reqTags |= tag.Key;
                }
            }
            blockTags ^= WorkTags.None;
            reqTags ^= WorkTags.None;

            story.workDisables = blockTags;
            story.requiredWorkTags = reqTags;

            story.forcedTraits = new List<TraitEntry>();
            story.disallowedTraits = new List<TraitEntry>();

            foreach (var trait in traitEntrys)
            {
                if(trait.status == 1)
                {
                    story.forcedTraits.Add(trait.trait);
                }else if(trait.status == 2)
                {
                    story.disallowedTraits.Add(trait.trait);
                }
            }

            if (!edit)
            {
                story.PostLoad();
                int num = Mathf.Abs(GenText.StableStringHash(story.baseDesc) % 100);
                string s = story.title.Replace('-', ' ');
                s = GenText.CapitalizedNoSpaces(s);
                story.identifier = GenText.RemoveNonAlphanumeric(s) + num.ToString();

                BackstoryDatabase.AddBackstory(story);

                CustomBacktories.CustomStories.Add(story);

                CreateNew();
            }
            else
            {
                if(!CustomBacktories.CustomStories.Contains(story))
                    CustomBacktories.CustomStories.Add(story);
            }

            if(!onlyCreate)
                PawnMenu.RecacheSkillsData();

            Messages.Message($"Success", MessageTypeDefOf.NeutralEvent);
        }
    }
}
