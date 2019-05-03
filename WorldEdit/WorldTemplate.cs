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
    }
}
