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
    internal sealed class LayersWindow : FWindow
    {
        private Vector2 scrollPosition = Vector2.zero;
        public override Vector2 InitialSize => new Vector2(250, 300);

        public LayersWindow()
        {
            resizeable = false;
        }

        public override void DoWindowContents(Rect inRect)
        {
            Text.Font = GameFont.Small;

            Rect mainScrollRect = new Rect(0, 0, inRect.width, inRect.height);
            int lenght = WorldEditor.Editor.Layers.Count * 30;
            Rect mainScrollVertRect = new Rect(0, 0, mainScrollRect.x, lenght);
            Widgets.BeginScrollView(mainScrollRect, ref scrollPosition, mainScrollVertRect);
            int yButton = 0;
            foreach(string layer in WorldEditor.Editor.Layers.Keys)
            {
                if(Widgets.ButtonText(new Rect(0, yButton, 230, 20), layer))
                {
                    LongEventHandler.QueueLongEvent(delegate
                    {
                        WorldLayer curLayer = WorldEditor.Editor.Layers[layer];
                        WorldEditor.Editor.WorldUpdater.UpdateLayer(curLayer);
                    }, "Updating layer...", doAsynchronously: false, null);
                }
                yButton += 25;
            }
            Widgets.EndScrollView();
        }
    }
}
