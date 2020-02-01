using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using DColor = System.Drawing.Color;
using MColor = System.Windows.Media.Color;

namespace SmartBulbColor.Tools
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
        public HSBColor GetMostCommonColorHSL()
        {
            Bitmap printscreen = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);
            Graphics graphics = Graphics.FromImage(printscreen as Image);
            graphics.CopyFromScreen(0, 0, 0, 0, printscreen.Size);
            graphics.Dispose();
            Bitmap image = new Bitmap(printscreen, 128, 72);
            printscreen.Dispose();
            HSBColor color = GetMostCommonColorHSV(image);
            return color;
        }
        public MColor GetMostCommonColorRGB()
        {
            Bitmap printscreen = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);
            Graphics graphics = Graphics.FromImage(printscreen as Image);
            graphics.CopyFromScreen(0, 0, 0, 0, printscreen.Size);
            graphics.Dispose();
            Bitmap image = new Bitmap(printscreen, 320, 240);
            printscreen.Dispose();
            HSBColor hslPixel = GetMostCommonColorHSV(image);
            DColor rgbPixel = HsbToDColor(hslPixel);
            return DrowingToMediaColor(rgbPixel);
        }
        HSBColor GetMostCommonColorHSV(Bitmap image)
        {   
            //Each number of position in array means Hue            
            List<DColor>[] hues = new List<DColor>[360];     //values means Hue pixel repeats on a picture
            int[] hueHistogram = new int[360];
            DColor pixel;
            bool isEmpty = true;
            for (int i = 0; i < image.Height; i++)
            {
                for (int j = 0; j < image.Width; j++)
                {
                    pixel = image.GetPixel(j, i);
                    int hue = GetHue(pixel);
                    if(GetBrightness(pixel) > 0)
                    {
                        if (hues[hue] == null)
                        {
                            hues[hue] = new List<DColor>();
                            isEmpty = false;
                        }
                        hues[hue].Add(pixel);
                        hueHistogram[hue]++;
                    }
                }
            }
            image.Dispose();
            HSBColor black = new HSBColor(0,0,0);
            if (isEmpty)
            {
                return black;
            }
            int[] hueHistogramSmooth = new int[360];
            for (int i = 0; i < hueHistogram.Length; i++)    //Smoothing histograms
            {
                if (hueHistogram[i] == 0)
                { continue; }

                else if(i == 0)
                {
                    hueHistogramSmooth[i] = (hueHistogram[i] + hueHistogram[i + 1]) / 2;
                }
                else if (i == hues.Length - 1)
                {
                    hueHistogramSmooth[i] = (hueHistogram[i -1] + hueHistogram[i]) / 2;
                }
                else
                {
                    hueHistogramSmooth[i] = (hueHistogram[i - 1] + hueHistogram[i] + hueHistogram[i + 1]) / 3;
                }
            }
            int temp = 0;
            int mostCommonHue = 0;
            isEmpty = true;
            for (int i = 0; i < hueHistogramSmooth.Length; i++)
            {
                if (hueHistogramSmooth[i] > temp)
                {
                    temp = hueHistogramSmooth[i];
                    mostCommonHue = i;
                    isEmpty = false;
                }
            }
            if(isEmpty)
            {
                return black;
            }
            int[] saturations = new int[101];
            int[] brightesses = new int[101];
            foreach (var color in hues[mostCommonHue])
            {
                saturations[GetSaturation(color)]++;
                brightesses[GetBrightness(color)]++;
            }
            int tempSat = 0;
            int mostCommonSat = 0;
            for (int i = 0; i < saturations.Length; i++)
            {
                if (saturations[i] > tempSat)
                {
                    tempSat = saturations[i];
                    mostCommonSat = i;
                }
            }
            int tempBright = 0;
            int mostCommonBright = 0;
            for (int i = 0; i < brightesses.Length; i++)
            {
                if (brightesses[i] > tempBright)
                {
                    tempBright = brightesses[i];
                    mostCommonBright = i;
                }
            }
            HSBColor avgColor = new HSBColor(mostCommonHue, mostCommonSat, mostCommonBright);
            return avgColor;
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
                    if (GetBrightness(pixel) > 0)
                    {
                        hueHistogram[hue]++;
                        saturationSums[hue] += pixel.GetSaturation();
                        brightessSums[hue] += GetBrightness(pixel);
                    }
                }
            }
            int[] hueHistogramSmooth = new int[360];
            float[] saturationSumsSmooth = new float[360];
            float[] brightnessSumsSmooth = new float[360];
            for (int i = 0; i < hueHistogram.Length; i++)    //Smoothing histograms
            {
                if (i == 0)
                {
                    hueHistogramSmooth[i] = (hueHistogram[i] + hueHistogram[i + 1]) / 2;
                    saturationSumsSmooth[i] = (saturationSums[i] + saturationSums[i + 1]) / 2;
                    brightnessSumsSmooth[i] = (brightessSums[i] + brightessSums[i + 1]) / 2;
                }
                else if (i == hueHistogram.Length - 1)
                {
                    hueHistogramSmooth[i] = (hueHistogram[i - 1] + hueHistogram[i]) / 2;
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
            if (hueHistogramSmooth[mostCommonHue] == 0) //to avoid dividing by 0
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
                    brightnessSums[hue] += GetBrightness(pixel);
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
        int GetHue(DColor color)
        {
            float r = (color.R/255f);
            float g = (color.G/255f);
            float b = (color.B/255f);
            float min = (r < g & r < b) ? r : g < b ? g : b;
            float max = (r > g & r > b) ? r : g > b ? g : b;
            if (min == max) return 0;
            else if (max == r & g >= b) return (int)( 60 * ((g - b) / (max - min)));
            else if (max == r & g < b) return (int)(60 * ((g - b) / (max - min)) + 360) ;
            else if (max == g) return (int)(60 * ((b - r) / (max - min)) + 120);
            else return (int)(60 * ((r - g) / (max - min)) + 240);
        }
        int GetSaturation(DColor color)
        {
            float r = color.R / 255f;
            float g = color.G / 255f;
            float b = color.B / 255f;
            float min = (r < g & r < b) ? r : g < b ? g : b;
            float max = (r > g & r > b) ? r : g > b ? g : b;
            if (max == 0) return 0;
            else return (int)((1 - min / max) * 100);
        }
        int GetBrightness(Color c)
        { return (int)(((c.R * 0.299f + c.G * 0.587f + c.B * 0.114f) / 256f) * 100); }
    }
}
