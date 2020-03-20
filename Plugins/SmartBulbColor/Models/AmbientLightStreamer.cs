using System;
using System.Collections.Generic;
using System.Threading;
using VerySmartHome.Interfaces;
using VerySmartHome.MainController;

namespace SmartBulbColor.Models
{
    class AmbientLightStreamer
    {
        List<ColorBulb> BulbsForStreaming;

        Queue<ColorBulb> BulbsToRemove = new Queue<ColorBulb>();

        readonly Thread AmbilightThread;
        readonly ManualResetEvent AmbilightTrigger;
        public ScreenColorAnalyzer ColorAnalyzer;
        private Object Locker = new Object();

        public AmbientLightStreamer()
        {
            DeviceDiscoverer.DeviceLost += OnDeviceLost;

            ColorAnalyzer = new ScreenColorAnalyzer();

            AmbilightThread = new Thread(new ThreadStart(StreamAmbientLightHSL));
            AmbilightThread.IsBackground = true;
            AmbilightTrigger = new ManualResetEvent(true);
        }

        public void SetBulbsForStreaming(List<ColorBulb> bulbs)
        {
            BulbsForStreaming = new List<ColorBulb>(bulbs);
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
                    if(BulbsToRemove.Count != 0)
                    {
                        BulbsForStreaming.Remove(BulbsToRemove.Dequeue());
                    }
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
        private void OnDeviceLost(IDevice foundDevice)
        {
            BulbsToRemove.Enqueue((ColorBulb)foundDevice);
        }
    }
}
