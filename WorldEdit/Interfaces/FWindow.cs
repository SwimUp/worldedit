using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace WorldEdit.Interfaces
{
    public abstract class FWindow : EditWindow
    {
        public virtual void Show()
        {
            Find.WindowStack.Add(this);
        }
    }
}
