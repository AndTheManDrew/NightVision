// Nightvision NVExperiments ColorShort.cs
// 
// 23 03 2019
// 
// 23 03 2019

using System;
using UnityEngine;

namespace NVExperiments.ThrownLights
{
    public struct ColorShort
    {
        public short r;
        public short g;
        public short b;
        public short a;
        public Color32 colour;
        

        public const byte maxAlbedo = 100;

        public ColorShort(short r, short g, short b, short a)
        {
            this.r = r;
            this.g = g;
            this.b = b;
            this.a = a;
            colour = new Color32(r> byte.MaxValue ? byte.MaxValue : (byte) r, g > byte.MaxValue ? byte.MaxValue : (byte) g, b > byte.MaxValue ? byte.MaxValue : (byte) b, a > 100? maxAlbedo : (byte)a);
        }

        public static ColorShort operator +(ColorShort a, ColorShort b)
        {
            return new ColorShort((short) (a.r + b.r), (short) (a.g + b.g), (short) (a.b + b.b), (short) (a.a + b.a));
        }
    }
}