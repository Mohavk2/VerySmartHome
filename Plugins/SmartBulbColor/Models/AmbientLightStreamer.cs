﻿using System;
using System.Collections.Generic;
using System.Threading;
using VerySmartHome.MainController;

namespace SmartBulbColor.Models
{
    class AmbientLightStreamer
    {
        CollectionThreadSafe<ColorBulb> BulbsForStreaming { get; set; } = new CollectionThreadSafe<ColorBulb>();

        readonly Thread AmbilightThread;
        readonly ManualResetEvent AmbilightTrigger;
        public ScreenColorAnalyzer ColorAnalyzer;

        public AmbientLightStreamer()
        {
            ColorAnalyzer = new ScreenColorAnalyzer();

            AmbilightThread = new Thread(new ThreadStart(StreamAmbientLightHSL));
            AmbilightThread.IsBackground = true;
            AmbilightTrigger = new ManualResetEvent(true);
        }
        public void AddBulbForStreaming(ColorBulb bulb)
        {
            BulbsForStreaming.Add(bulb);
            if(BulbsForStreaming.Count == 1)
                StartSreaming();
        }
        public void StartSreaming()
        {
            if (AmbilightThread.IsAlive)
            {
                AmbilightTrigger.Set();
            }
            else
            {
                AmbilightThread.Start();
            }
        }
        public void StopStreaming()
        {
            AmbilightTrigger.Reset();
        }
        void StreamAmbientLightHSL()
        {
            HSBColor color;
            int previosHue = 0;
            while (true)
            {
                AmbilightTrigger.WaitOne(Timeout.Infinite);

                color = ColorAnalyzer.GetMostCommonColorHSB();

                var bright = color.Brightness;
                var hue = (bright < 1) ? previosHue : color.Hue;
                var sat = color.Saturation;

                if (BulbsForStreaming != null && BulbsForStreaming.Count != 0)
                { 
                    foreach (var bulb in BulbsForStreaming)
                    {
                        bulb.SetSceneHSV(hue, sat, bright);
                    }
                }
                else StopStreaming();

                previosHue = color.Hue;
                Thread.Sleep(60);
            }
        }
        public void RemoveBulb(ColorBulb bulb)
        {
            BulbsForStreaming.Remove(bulb);
        }
    }
}
