using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace WorldEdit
{
    public enum IconCategory : byte
    {
        Country,
        Other
    }

    public class IconDef : Def
    {
        public IconCategory Category = IconCategory.Other;

        public string TexturePath;

        [Unsaved]
        private Texture2D textureInt;

        public Texture2D Texture
        {
            get
            {
                if (textureInt == null)
                {
                    if (!TexturePath.NullOrEmpty())
                    {
                        textureInt = ContentFinder<Texture2D>.Get(TexturePath);
                    }
                    else
                    {
                        textureInt = BaseContent.BadTex;
                    }
                }
                return textureInt;
            }
        }

        public static IconDef Named(string defName)
        {
            return DefDatabase<IconDef>.GetNamed(defName, true);
        }
    }

    [DefOf]
    public static class IconDefOf
    {

    }

}
