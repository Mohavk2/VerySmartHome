using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using DColor = System.Drawing.Color;
using MColor = System.Windows.Media.Color;

namespace VerySmartHome.Tools
{
    public sealed class ScreenColorAnalyzer
    {
        HSBColor ColorBufer = new HSBColor(0,0,0);
        object locker = new object();
        public MColor GetAvgScreenColor()
        {
            Bitmap printscreen = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);
            Graphics graphics = Graphics.FromImage(printscreen as Image);
            graphics.CopyFromScreen(0, 0, 0, 0, printscreen.Size);
            graphics.Dispose();
            Bitmap avgPixel = new Bitmap(printscreen, 1, 1);
            printscreen.Dispose();
            DColor dAvgColor = avgPixel.GetPixel(0, 0);
            avgPixel.Dispose();
            MColor avgColor = DrowingToMediaColor(dAvgColor);
            return avgColor;
        }

        public MColor GetSceneAvgColorRGB()
        {
            Bitmap printscreen = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);
            Graphics graphics = Graphics.FromImage(printscreen as Image);
            graphics.CopyFromScreen(0, 0, 0, 0, printscreen.Size);
            graphics.Dispose();
            var width = (printscreen.Width / 15);
            var hight = (printscreen.Height / 9);
            Bitmap leftUpSquare = printscreen.Clone(new Rectangle(width * 2, hight * 2, width, hight), printscreen.PixelFormat);
            Bitmap rightUpSquare = printscreen.Clone(new Rectangle(width * 13, hight * 2, width, hight), printscreen.PixelFormat);
            Bitmap centerSquare = printscreen.Clone(new Rectangle(width * 7, hight * 4, width, hight), printscreen.PixelFormat);
            Bitmap leftDownSquare = printscreen.Clone(new Rectangle(width * 2, hight * 7, width, hight), printscreen.PixelFormat);
            Bitmap rightDownSquare = printscreen.Clone(new Rectangle(width * 13, hight * 7, width, hight), printscreen.PixelFormat);
            printscreen.Dispose();
            Bitmap leftUpPixel = new Bitmap(leftUpSquare, 1, 1);
            Bitmap rightUpPixel = new Bitmap(rightUpSquare, 1, 1);
            Bitmap centerPixel = new Bitmap(centerSquare, 1, 1);
            Bitmap leftDownPixel = new Bitmap(leftDownSquare, 1, 1);
            Bitmap rightDownPixel = new Bitmap(rightDownSquare, 1, 1);

            DColor leftUp = leftUpPixel.GetPixel(0, 0);
            DColor rightUp = leftUpPixel.GetPixel(0, 0);
            DColor center = leftUpPixel.GetPixel(0, 0);
            DColor leftDown = leftUpPixel.GetPixel(0, 0);
            DColor rightDown = leftUpPixel.GetPixel(0, 0);

            List<DColor> Colors = new List<DColor>();
            Colors.Add(leftUp);
            Colors.Add(rightUp);
            Colors.Add(center);
            Colors.Add(leftDown);
            Colors.Add(rightDown);
            int r = 0, g = 0, b = 0;
            var count = Colors.Count;
            foreach (var color in Colors)
            {
                r += color.R;
                g += color.G;
                b += color.B;
            }
            MColor AvgColor = new MColor();
            AvgColor.R = (byte)(r / count);
            AvgColor.G = (byte)(g / count);
            AvgColor.B = (byte)(b / count);
            return AvgColor;

        }
        public HSBColor GetMostCommonColorHSL()
        {
            Bitmap printscreen = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);
            Graphics graphics = Graphics.FromImage(printscreen as Image);
            graphics.CopyFromScreen(0, 0, 0, 0, printscreen.Size);
            graphics.Dispose();
            Bitmap image = new Bitmap(printscreen, 128, 72);
            printscreen.Dispose();
            HSBColor pixel = GetAvgPixelHSL(image);
            return pixel;
        }
        public MColor GetMostCommonColorRGB()
        {
            Bitmap printscreen = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);
            Graphics graphics = Graphics.FromImage(printscreen as Image);
            graphics.CopyFromScreen(0, 0, 0, 0, printscreen.Size);
            graphics.Dispose();
            Bitmap image = new Bitmap(printscreen, 320, 240);
            printscreen.Dispose();
            HSBColor hslPixel = GetAvgPixelHSL(image);
            DColor rgbPixel = HsbToDColor(hslPixel);
            return DrowingToMediaColor(rgbPixel);
        }
        HSBColor GetAvgPixelHSL(Bitmap image)
        {   
            //Each number of position in array means Hue 
            
            int[] hueHistogram = new int[360];          //values means Hue repeats on a picture
            float[] saturationSums = new float[360];    //values means saturation sums for Hue repeats
            float[] brightessSums = new float[360];     //values means brightness sums for Hue repeats

            DColor pixel;
            for (int i = 0; i < image.Height; i++)
            {
                for (int j = 0; j < image.Width; j++)
                {
                    pixel = image.GetPixel(j, i);
                    int hue = (int)pixel.GetHue();
                    if(GetAccurateBrightness(pixel) > 0)
                    {
                        hueHistogram[hue]++;
                        saturationSums[hue] += pixel.GetSaturation();
                        brightessSums[hue] += GetAccurateBrightness(pixel);
                    }
                }
            }
            int[] hueHistogramSmooth = new int[360];
            float[] saturationSumsSmooth = new float[360];
            float[] brightnessSumsSmooth = new float[360];

            for (int i = 0; i < hueHistogram.Length; i++)    //Smoothing histograms
            {
                if(i == 0)
                {
                    hueHistogramSmooth[i] = (hueHistogram[i] + hueHistogram[i + 1]) / 2;
                    saturationSumsSmooth[i] = (saturationSums[i] + saturationSums[i + 1]) / 2;
                    brightnessSumsSmooth[i] = (brightessSums[i] + brightessSums[i + 1]) / 2;
                }
                else if (i == hueHistogram.Length - 1)
                {
                    hueHistogramSmooth[i] = (hueHistogram[i -1] + hueHistogram[i]) / 2;
                    saturationSumsSmooth[i] = (saturationSums[i - 1] + saturationSums[i]) / 2;
                    brightnessSumsSmooth[i] = (brightessSums[i - 1] + brightessSums[i]) / 2;
                }
                else
                {
                    hueHistogramSmooth[i] = (hueHistogram[i - 1] + hueHistogram[i] + hueHistogram[i + 1]) / 3;
                    saturationSumsSmooth[i] = (saturationSums[i - 1] + saturationSums[i] + saturationSums[i + 1]) / 3;
                    brightnessSumsSmooth[i] = (brightessSums[i - 1] + brightessSums[i] + brightessSums[i + 1]) / 3;
                }
            }
            int temp = 0;
            int mostCommonHue = 0;
            for (int i = 0; i < hueHistogramSmooth.Length; i++)
            {
                if (hueHistogramSmooth[i] > temp)
                {
                    temp = hueHistogramSmooth[i];
                    mostCommonHue = i;
                }
            }
            image.Dispose();
            if(hueHistogramSmooth[mostCommonHue] == 0) //to avoid dividing by 0
            {
                hueHistogramSmooth[mostCommonHue] = 1;
            }
            int mostCommonHueSatAvg = (100 * (int)saturationSumsSmooth[mostCommonHue]) / (int)hueHistogramSmooth[mostCommonHue];
            float mostCommonHueBrightAvg = 100 * brightnessSumsSmooth[mostCommonHue] / hueHistogramSmooth[mostCommonHue];
            HSBColor avgColor = new HSBColor(mostCommonHue, mostCommonHueSatAvg, mostCommonHueBrightAvg);
            //ColorBufer = avgColor;
            return avgColor;
        }
        HSBColor GetAvgPixelHSLOptimized(Bitmap image)
        {
            int[] hueCounts = new int[360];
            float[] saturationSums = new float[360];
            float[] brightnessSums = new float[360];
            Color pixel;
            List<int> existingHues = new List<int>();

            for (int i = 0; i < image.Height; i++)
            {
                for (int j = 0; j < image.Width; j++)
                {
                    pixel = image.GetPixel(j, i);
                    int hue = (int)pixel.GetHue();
                    existingHues.Add(hue);
                    hueCounts[hue]++;
                    saturationSums[hue] += pixel.GetSaturation();
                    brightnessSums[hue] += GetAccurateBrightness(pixel);
                }
            }
            int count = 0;
            int mostCommonHue = 0;
            foreach (var hue in existingHues)
            {
                if(hueCounts[hue] > count)
                {
                    count = hueCounts[hue];
                    mostCommonHue = hue;
                }
            }
            int hueSaturationAvg = 100 * (int)saturationSums[mostCommonHue] / (int)hueCounts[mostCommonHue];
            int hueBrightAvg = 100 * (int)brightnessSums[mostCommonHue] / (int)hueCounts[mostCommonHue];
            HSBColor avgColor = new HSBColor(mostCommonHue, hueSaturationAvg, hueBrightAvg);
            return avgColor;
        }
        float GetAccurateBrightness(Color c)
        { return (c.R * 0.299f + c.G * 0.587f + c.B * 0.114f) / 256f; }
        MColor DrowingToMediaColor (DColor dcolor)
        {
            MColor mcolor = new MColor();
            mcolor.R = dcolor.R;
            mcolor.G = dcolor.G;
            mcolor.B = dcolor.B;
            mcolor.A = dcolor.A;
            return mcolor;
        }
        [DllImport("shlwapi.dll")]
        public static extern int ColorHLSToRGB(int H, int L, int S);
        public MColor GetColorBuffer()
        {

            var color = HsbToDColor(ColorBufer);
            return DrowingToMediaColor(color);

        }
        private DColor HsbToDColor(HSBColor hsbColor)
        {
            int hue = (hsbColor.Hue / 360) * 240;
            float brt = (hsbColor.Brightness / 360) * 240;
            float sat = (hsbColor.Saturation / 360) * 240;
            int value = ColorHLSToRGB(hue, (int)sat, (int)sat);
            return ColorTranslator.FromWin32(value);
        }
    }
}
