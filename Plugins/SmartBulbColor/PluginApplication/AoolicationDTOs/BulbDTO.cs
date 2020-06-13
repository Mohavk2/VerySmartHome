using System;
using System.Collections.Generic;
using System.Text;

namespace SmartBulbColor.PluginApplication.AoolicationDTOs
{
    public struct BulbDTO
    {
        public int Id;
        public string Name;
        public string Model;
        public string Ip;
        public bool IsPowered;
        public string BelongToGroup;
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
