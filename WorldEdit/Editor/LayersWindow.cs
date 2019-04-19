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
    internal class LayersWindow : EditWindow, IFWindow
    {
        private Vector2 scrollPosition = Vector2.zero;
        public override Vector2 InitialSize => new Vector2(250, 300);

        public LayersWindow()
        {
            resizeable = false;
        }

        public void Show()
        {
            if (Find.WindowStack.IsOpen(typeof(LayersWindow)))
            {
                Log.Message("Currntly open...");
            }
            else
            {
                Find.WindowStack.Add(this);
            }
        }
        public override void DoWindowContents(Rect inRect)
        {
            Text.Font = GameFont.Small;

            Rect mainScrollRect = new Rect(0, 0, inRect.width, inRect.height);
            int lenght = MainMenu.Editor.Layers.Count * 30;
            Rect mainScrollVertRect = new Rect(0, 0, mainScrollRect.x, lenght);
            Widgets.BeginScrollView(mainScrollRect, ref scrollPosition, mainScrollVertRect);
            int yButton = 0;
            foreach(string layer in MainMenu.Editor.Layers.Keys)
            {
                if(Widgets.ButtonText(new Rect(0, yButton, 230, 20), layer))
                {
                    WorldLayer curLayer = MainMenu.Editor.Layers[layer];
                    MainMenu.Editor.WorldUpdater.UpdateLayer(curLayer);
                }
                yButton += 25;
            }
            Widgets.EndScrollView();
        }
    }
}
