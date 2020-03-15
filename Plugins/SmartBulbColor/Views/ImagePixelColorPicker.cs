using SmartBulbColor.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DColor = System.Drawing.Color;
using MColor = System.Windows.Media.Color;

namespace SmartBulbColor.Models
{
    class ImagePixelColorPicker
    {
        Bitmap Image;
        public ImagePixelColorPicker(string resourceAddress)
        {
            var tempImage = new Bitmap(resourceAddress);
            Image = new Bitmap(tempImage);
            tempImage.Dispose();
        }
        public HSBColor GetHSBColor(int x, int y)
        {
            var dColor = Image.GetPixel(x, y);
            return new HSBColor(HSBColor.GetHue(dColor), HSBColor.GetSaturation(dColor), HSBColor.GetBrightness(dColor));
        }
        public DColor GetDrawingColor(int x, int y)
        {
            return Image.GetPixel(x, y);
        }
        public MColor GetMediaColor(int x, int y)
        {
            return HSBColor.DrowingToMediaColor(Image.GetPixel(x, y));
        }
        public int GetImageWidth()
        {
            return Image.Width;
        }
        public int GetImageHeight()
        {
            return Image.Height;
        }
    }
}
