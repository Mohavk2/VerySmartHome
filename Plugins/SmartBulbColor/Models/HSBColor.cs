using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DColor = System.Drawing.Color;
using MColor = System.Windows.Media.Color;

namespace SmartBulbColor.Tools
{
    public class HSBColor
    {
        public static int GetHue(DColor color)
        {
            return CalculateHue(color.R, color.G, color.B);
        }
        public static int GetHue(MColor color)
        {
            return CalculateHue(color.R, color.G, color.B);
        }
        public static int GetSaturation(DColor color)
        {
            return CalculateSaturation(color.R, color.G, color.B);
        }
        public static int GetSaturation(MColor color)
        {
            return CalculateSaturation(color.R, color.G, color.B);
        }
        public static int GetBrightness(DColor color)
        {
            return CalculateBrightness(color.R, color.G, color.B);
        }
        public static int GetBrightness(MColor color)
        {
            return CalculateBrightness(color.R, color.G, color.B);
        }
        private static int CalculateHue(int red, int green, int blue)
        {
            float r = (red / 255f);
            float g = (green / 255f);
            float b = (blue / 255f);
            float min = (r < g & r < b) ? r : g < b ? g : b;
            float max = (r > g & r > b) ? r : g > b ? g : b;
            if (min == max) return 0;
            else if (max == r & g >= b) return (int)(60 * ((g - b) / (max - min)));
            else if (max == r & g < b) return (int)(60 * ((g - b) / (max - min)) + 360);
            else if (max == g) return (int)(60 * ((b - r) / (max - min)) + 120);
            else return (int)(60 * ((r - g) / (max - min)) + 240);
        }
        private static int CalculateSaturation(int red, int green, int blue)
        {
            float r = red / 255f;
            float g = green / 255f;
            float b = blue / 255f;
            float min = (r < g & r < b) ? r : g < b ? g : b;
            float max = (r > g & r > b) ? r : g > b ? g : b;
            if (max == 0) return 0;
            else return (int)((1 - min / max) * 100);
        }
        private static int CalculateBrightness(int red, int green, int blue)
        {
            return (int)(((red * 0.299f + green * 0.587f + blue * 0.114f) / 256f) * 100);
        }
        public static DColor MediaToDrowingColor(MColor mcolor)
        {
            return DColor.FromArgb(mcolor.A, mcolor.R, mcolor.G, mcolor.B);
        }
        public static MColor DrowingToMediaColor(DColor dcolor)
        {
            MColor mcolor = new MColor();
            mcolor.R = dcolor.R;
            mcolor.G = dcolor.G;
            mcolor.B = dcolor.B;
            mcolor.A = dcolor.A;
            return mcolor;
        }

        public int Hue { get; }
        public float Saturation { get;}
        public float Brightness { get;}

        public HSBColor(int Hue, float Sat, float Light)
        {
            this.Hue = Hue;
            this.Saturation = Sat;
            this.Brightness = Light;
        }
        public HSBColor(DColor color)
        {
            Hue = GetHue(color);
            Saturation = GetSaturation(color);
            Brightness = GetBrightness(color);
        }
        public HSBColor(MColor color)
        {
            Hue = GetHue(color);
            Saturation = GetSaturation(color);
            Brightness = GetBrightness(color);
        }
    }
}
