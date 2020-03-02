using RimWorld;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Xml.Serialization;
using Verse;
using WorldEdits;

namespace WorldEdit
{


    public class CustomBacktories : GameComponent
    {
        private class Story : IExposable
        {
            public string identifier;

            public BackstorySlot slot;

            public string title;

            public string titleFemale;

            public string titleShort;

            public string titleShortFemale;

            public string baseDesc;

            public Dictionary<SkillDef, int> skillGainsResolved = new Dictionary<SkillDef, int>();

            public WorkTags workDisables;

            public WorkTags requiredWorkTags;

            public List<TraitData> traitData = new List<TraitData>();

            public void ExposeData()
            {
                Scribe_Values.Look(ref identifier, "identifier");
                Scribe_Values.Look(ref slot, "slot");
                Scribe_Values.Look(ref title, "title");
                Scribe_Values.Look(ref titleFemale, "titleFemale");
                Scribe_Values.Look(ref titleShort, "titleShort");
                Scribe_Values.Look(ref titleShortFemale, "titleShortFemale");
                Scribe_Values.Look(ref baseDesc, "baseDesc");
                Scribe_Collections.Look(ref skillGainsResolved, "skillGainsResolved", LookMode.Def, LookMode.Value);
                Scribe_Values.Look(ref workDisables, "workDisables");
                Scribe_Values.Look(ref requiredWorkTags, "requiredWorkTags");
                Scribe_Collections.Look(ref traitData, "traitData", LookMode.Deep);
            }
        }

        public static List<Backstory> CustomStories = new List<Backstory>();
        private static List<Story> toSave = new List<Story>();

        public CustomBacktories()
        {

        }

        public CustomBacktories(Game game)
        {
            foreach (var story in toSave)
            {
                if (BackstoryDatabase.allBackstories.Keys.Contains(story.identifier))
                {
                    BackstoryDatabase.allBackstories.Remove(story.identifier);
                }
            }
        }

        public static void DeleteStory(Backstory story)
        {
            if(CustomStories.Contains(story))
            {
                CustomStories.Remove(story);
            }
        }

        public override void LoadedGame()
        {
            Inject();
        }

        public override void ExposeData()
        {
            base.ExposeData();
            if (Scribe.mode == LoadSaveMode.Saving)
            {
                foreach (var back in CustomStories)
                {
                    Story story = new Story
                    {
                        identifier = back.identifier,
                        title = back.title,
                        titleFemale = back.titleFemale,
                        titleShort = back.titleShort,
                        titleShortFemale = back.titleShortFemale,
                        baseDesc = back.baseDesc,
                        skillGainsResolved = back.skillGainsResolved,
                        workDisables = back.workDisables,
                        requiredWorkTags = back.requiredWorkTags,
                        traitData = new List<TraitData>()
                    };

                    if (back.forcedTraits != null)
                    {
                        foreach (var t1 in back.forcedTraits)
                        {
                            story.traitData.Add(new TraitData(t1, 1));
                        }
                    }

                    if (back.disallowedTraits != null)
                    {
                        foreach (var t2 in back.disallowedTraits)
                        {
                            story.traitData.Add(new TraitData(t2, 2));
                        }
                    }

                    toSave.Add(story);
                }
            }

            Scribe_Collections.Look(ref toSave, "CustomStoriesWorldEdit", LookMode.Deep);
        }

        public static void Inject()
        {
            Log.Message("Injecting backstories...");

            if (toSave == null || toSave.Count == 0)
            {
                Log.Message("No custom stories found...");
                return;
            }

            foreach (var story in toSave)
            {
                Backstory bStory = new Backstory()
                {
                    identifier = story.identifier,
                    title = story.title,
                    titleShort = story.titleShort,
                    titleFemale = story.titleFemale,
                    titleShortFemale = story.titleShortFemale,
                    skillGainsResolved = story.skillGainsResolved,
                    baseDesc = story.baseDesc,
                    workDisables = story.workDisables,
                    requiredWorkTags = story.requiredWorkTags,
                    forcedTraits = new List<TraitEntry>(),
                    disallowedTraits = new List<TraitEntry>()
                };
                if (story.traitData != null)
                {
                    foreach (var t in story.traitData)
                    {
                        if (t.status == 1)
                        {
                            bStory.forcedTraits.Add(new TraitEntry(t.traitDef, t.degreeInt));
                        }
                        if (t.status == 2)
                        {
                            bStory.disallowedTraits.Add(new TraitEntry(t.traitDef, t.degreeInt));
                        }
                    }
                }

                bStory.PostLoad();

                if (BackstoryDatabase.allBackstories.Keys.Contains(bStory.identifier))
                    BackstoryDatabase.allBackstories.Remove(bStory.identifier);

                BackstoryDatabase.AddBackstory(bStory);
            }

            Log.Message($"Loaded {toSave.Count} custom stories");
        }
    }
}
