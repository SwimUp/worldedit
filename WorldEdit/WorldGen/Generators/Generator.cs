using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WorldEdit.WorldGen.Generators
{
    public abstract class Generator
    {
        public abstract string Title {get;}

        public abstract string Description { get; }

        public abstract GeneratorMode Mode { get; }

        public abstract GeneratorType Type { get; }

        public abstract void RunGenerator();
    }
}
