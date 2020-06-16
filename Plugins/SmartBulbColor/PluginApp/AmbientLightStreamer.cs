using SmartBulbColor.Models;
using System.Threading;
using CommonLibrary;

namespace SmartBulbColor.PluginApp
{
    class AmbientLightStreamer
    {
        CollectionThreadSafe<ColorBulbProxy> BulbsForStreaming { get; set; } = new CollectionThreadSafe<ColorBulbProxy>();

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
        public void AddBulbForStreaming(ColorBulbProxy bulb)
        {
            //bulb.ExecuteCommand(BulbCommandBuilder.CreateSetPowerCommand(Power.On, Effect.Smooth, 5, ColorMode.HSV));
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
                var command = BulbCommandBuilder.CreateSetSceneHsvCommand(CommandType.Stream, hue, (int)sat, (int)bright);
                if (BulbsForStreaming != null && BulbsForStreaming.Count != 0)
                { 
                    foreach (var bulb in BulbsForStreaming)
                    {
                        bulb.ExecuteCommand(command);
                    }
                }
                else StopStreaming();

                previosHue = color.Hue;
                Thread.Sleep(60);
            }
        }
        public void RemoveBulb(ColorBulbProxy bulb)
        {
            BulbsForStreaming.Remove(bulb);
        }
    }
}
