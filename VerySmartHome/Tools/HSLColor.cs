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
        public float Saturation { get;}
        public float Lightness { get;}

        public HSLColor(int Hue, float Sat, float Light)
        {
            this.Hue = Hue;
            this.Saturation = Sat;
            this.Lightness = Light;
        }
    }
}
