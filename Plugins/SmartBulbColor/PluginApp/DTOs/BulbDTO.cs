using System;
using System.Collections.Generic;
using System.Text;

namespace SmartBulbColor.PluginApp
{
    public struct BulbDTO
    {
        public string Id;
        public string Name;
        public string Model;
        public string Ip;
        public bool IsPowered;
        public string BelongsToGroup;
        public bool IsOnline;
        public bool IsMusicModeOn;
        public int FwVer;
        public int Brightness;
        public int ColorMode;
        public int ColorTemperature;
        public bool Flowing;
        public int Delayoff;
        public int Rgb;
        public int Hue;
        public int Saturation;
    }
}
