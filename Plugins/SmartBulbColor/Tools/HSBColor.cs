using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartBulbColor.Tools
{
    public class HSBColor
    {
        public int Hue { get; }
        public float Saturation { get;}
        public float Brightness { get;}

        public HSBColor(int Hue, float Sat, float Light)
        {
            this.Hue = Hue;
            this.Saturation = Sat;
            this.Brightness = Light;
        }
    }
}
