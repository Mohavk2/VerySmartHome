using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VerySmartHome.MainController;

//MM - MusicMode

namespace SmartBulbColor.Models
{
    sealed class BulbController : DeviceController, IDisposable
    {
        public delegate void BulbCollectionNotifier();
        public delegate void BulbStatusChangedHandler(ColorBulb bulb);
        public event BulbStatusChangedHandler BulbFound;

        public CollectionThreadSafe<ColorBulb> Bulbs { get; } = new CollectionThreadSafe<ColorBulb>();
        public CollectionThreadSafe<ColorBulb> BulbsForAmbientLight { get; private set; } = new CollectionThreadSafe<ColorBulb>();
        public override int DeviceCount
        {
            get
            {
                return Bulbs.Count;
            }
        }
        readonly static BulbDiscoverer Discoverer = new BulbDiscoverer(ColorBulb.SSDPMessage);
        private readonly AmbientLightStreamer AmbientLight = new AmbientLightStreamer();

        public bool IsMusicModeON { get; private set; } = false;

        public BulbController()
        {
            DeviceDiscoverer.DeviceFound += OnDeviceFound;
            Discoverer.StartDiscover();
        }
        public override List<Device> GetDevices()
        {
            return new List<Device>();
        }
        public CollectionThreadSafe<ColorBulb> GetBulbs()
        {
            if (Bulbs.Count != 0)
            {
                return Bulbs;
            }
            else
            {
                return Bulbs;
            }
        }
        public void DiscoverBulbs()
        {
            Discoverer.FindDevices();
        }
        public void StartBulbsRefreshing()
        {
            Discoverer.StartDiscover();
        }
        public void ToggleAmbientLight(ColorBulb bulb)
        {
            if (!BulbsForAmbientLight.Contains(bulb))
            {
                try
                {
                    Discoverer.StopDiscover();
                    bulb.SetColorMode(2);
                    BulbsForAmbientLight.Add(bulb);
                    AmbientLight.AddBulbForStreaming(bulb);
                }
                catch (Exception MusicModeFailedException)
                {
                    throw MusicModeFailedException;
                }
            }
            else
            {
                AmbientLight.RemoveBulb(bulb);
                BulbsForAmbientLight.Remove(bulb);
                if (BulbsForAmbientLight.Count == 0)
                    Discoverer.StartDiscover();
            }
        }
        /// <summary>
        /// Sets color mode
        /// </summary>
        /// <param name="value"> 1 - CT mode, 2 - RGB mode , 3 - HSV mode</param>
        public void SetSceneHSV(ColorBulb bulb, HSBColor color)
        {
            bulb.SetSceneHSV(color.Hue, color.Saturation, color.Brightness);
        }
        private void OnDeviceFound(Device foundDevice)
        {
            Task task = new Task(() =>
            {
                var bulb = (ColorBulb)foundDevice;
                if( ! BulbsForAmbientLight.Contains(bulb))
                {
                    Bulbs.Add(bulb);
                    BulbFound?.Invoke(bulb);
                }
            });
            task.Start();
        }
        public void Dispose()
        {
            foreach(var bulb in Bulbs)
            {
                bulb.Dispose();
            }
        }
    }
}
