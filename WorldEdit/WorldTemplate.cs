using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using Verse;

namespace WorldEdit
{
    [XmlRoot]
    public sealed class WorldTemplate
    {
        public string FilePath;

        public string WorldName;

        public string Author;

        public string Description;

        public string Storyteller;

        public string Scenario;

        public PawnSelectMode PawnSelectMode = PawnSelectMode.Standart;

        [XmlIgnore]
        public List<Pawn> StartPawns = new List<Pawn>();
    }

    public class StartPawnsFromTemplate : GameComponent
    {
        public static List<Pawn> StartPawns = new List<Pawn>();

        public StartPawnsFromTemplate()
        {

        }

        public StartPawnsFromTemplate(Game game)
        {

        }

        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_Collections.Look(ref StartPawns, "StartingPawnFromTemplate", LookMode.Deep);
        }

        public override void LoadedGame()
        {
            if (StartPawns != null)
            {
                WorldEditor.LoadedTemplate.StartPawns = StartPawns;
            }
        }
    }
}
