// Nightvision NVTesting GlowCell.cs
// 
//   
// 
// 22 03 2019

using UnityEngine;

namespace NVTesting.ThrownLights {
    public class GlowCell
    {
        public int     index;
        public int     dist;
        public Color32 oldGlow;
        public Color32 addedColour;
        public Color32 displayColour;

        public GlowCell() { }

        public void SetNewColour(Color32 newColor)
        {
            addedColour = newColor;
        }

        public void StoreOldColour(Color32 color)
        {
            oldGlow = color;
        }

        public void Update(int newCellIndex)
        {
            index = newCellIndex;
        }

        public bool NewCellNeedsUpdate(GlowCell oldCell)
        {
            return NewCellNeedsUpdate(oldCell.dist, oldCell.oldGlow);
        }

        public bool NewCellNeedsUpdate(int oldDist, Color32 oldGlow)
        {
            this.oldGlow = oldGlow;
            if (oldDist != dist)
            {

                return true;
            }

            return false;
        }

        public void SetDist(int dist)
        {
            this.dist = dist;
        }

        public GlowCell(GlowCell toBeCopied)
        {
            index      = toBeCopied.index;
            dist       = toBeCopied.dist;
            oldGlow    = toBeCopied.oldGlow;
            addedColour = toBeCopied.addedColour;
        }
    }
}