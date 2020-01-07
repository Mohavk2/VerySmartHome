using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Dcolor = System.Drawing.Color;
using Mcolor = System.Windows.Media.Color;

namespace VerySmartHome.Tools
{
    public sealed class ScreenColorAnalyzer
    {
        Dcolor ColorBufer = Dcolor.Empty;
        public Mcolor GetAvgScreenColor()
        {
            Bitmap printscreen = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);
            Graphics graphics = Graphics.FromImage(printscreen as Image);
            graphics.CopyFromScreen(0, 0, 0, 0, printscreen.Size);
            graphics.Dispose();
            Bitmap avgPixel = new Bitmap(printscreen, 1, 1);
            printscreen.Dispose();
            Dcolor dAvgColor = avgPixel.GetPixel(0, 0);
            avgPixel.Dispose();
            ColorBufer = dAvgColor;
            Mcolor avgColor = DrowingToMediaColor(dAvgColor);
            return avgColor;
        }
        public float GetBrightness()
        {
            if (ColorBufer != Dcolor.Empty)
                return ColorBufer.GetBrightness();
            else return 0.5F;
        }
        Mcolor DrowingToMediaColor (Dcolor dcolor)
        {
            Mcolor mcolor = new Mcolor();
            mcolor.R = dcolor.R;
            mcolor.G = dcolor.G;
            mcolor.B = dcolor.B;
            mcolor.A = dcolor.A;
            return mcolor;
        }
    }
}
