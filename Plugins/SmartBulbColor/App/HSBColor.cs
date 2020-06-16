using System;
using DColor = System.Drawing.Color;
using MColor = System.Windows.Media.Color;

namespace SmartBulbColor.PluginApplication
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

        public int Hue { get; } // 0-360
        public float Saturation { get;}// 0-100
        public float Brightness { get;}// 0-100

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

        public MColor ToRgb()
        {
            int r;
            int g;
            int b;
            HsvToRgb(Hue, Saturation/100, Brightness/100, out r, out g, out b);
            MColor color = MColor.FromRgb((byte)r, (byte)g, (byte)b);
            return color;
        }

        void HsvToRgb(double h, double S, double V, out int r, out int g, out int b)
        {
            double H = h;
            while (H < 0) { H += 360; };
            while (H >= 360) { H -= 360; };
            double R, G, B;
            if (V <= 0)
            { R = G = B = 0; }
            else if (S <= 0)
            {
                R = G = B = V;
            }
            else
            {
                double hf = H / 60.0;
                int i = (int)Math.Floor(hf);
                double f = hf - i;
                double pv = V * (1 - S);
                double qv = V * (1 - S * f);
                double tv = V * (1 - S * (1 - f));
                switch (i)
                {

                    // Red is the dominant color

                    case 0:
                        R = V;
                        G = tv;
                        B = pv;
                        break;

                    // Green is the dominant color

                    case 1:
                        R = qv;
                        G = V;
                        B = pv;
                        break;
                    case 2:
                        R = pv;
                        G = V;
                        B = tv;
                        break;

                    // Blue is the dominant color

                    case 3:
                        R = pv;
                        G = qv;
                        B = V;
                        break;
                    case 4:
                        R = tv;
                        G = pv;
                        B = V;
                        break;

                    // Red is the dominant color

                    case 5:
                        R = V;
                        G = pv;
                        B = qv;
                        break;

                    // Just in case we overshoot on our math by a little, we put these here. Since its a switch it won't slow us down at all to put these here.

                    case 6:
                        R = V;
                        G = tv;
                        B = pv;
                        break;
                    case -1:
                        R = V;
                        G = pv;
                        B = qv;
                        break;

                    // The color is not defined, we should throw an error.

                    default:
                        //LFATAL("i Value error in Pixel conversion, Value is %d", i);
                        R = G = B = V; // Just pretend its black/white
                        break;
                }
            }
            r = Clamp((int)(R * 255.0));
            g = Clamp((int)(G * 255.0));
            b = Clamp((int)(B * 255.0));
        }

        /// <summary>
        /// Clamp a value to 0-255
        /// </summary>
        int Clamp(int i)
        {
            if (i < 0) return 0;
            if (i > 255) return 255;
            return i;
        }
    }
}
