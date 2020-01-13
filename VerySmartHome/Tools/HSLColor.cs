using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VerySmartHome.Tools
{
    public class HSLColor
    {
        public int Hue { get; }
        public int Saturation { get;}
        public int Lightness { get;}

        public HSLColor(int Hue, int Sat, int Light)
        {
            this.Hue = Hue;
            this.Saturation = Sat;
            this.Lightness = Light;
        }
    }
}
