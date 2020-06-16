using SmartBulbColor.Domain;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CommonLibrary;
using System.Windows.Data;

namespace SmartBulbColor.PluginApp
{
    sealed class AppCore : IDisposable
    {
        static HsdpSearchingAtributes Atributes = new HsdpSearchingAtributes
        {
            SsdpMessage = "M-SEARCH * HTTP/1.1\r\n" + "HOST: 239.255.255.250:1982\r\n" + "MAN: \"ssdp:discover\"\r\n" + "ST: wifi_bulb",
            DeviceType = "MiBulbColor",
            MulticastPort = 1982
        };
        readonly HsdpDiscoverer Discoverer = new HsdpDiscoverer(Atributes);
        readonly BulbRepository Repository;
        private readonly AmbientLightStreamer AmbientLight = new AmbientLightStreamer();

        public CollectionThreadSafe<ColorBulbProxy> Bulbs { get; } = new CollectionThreadSafe<ColorBulbProxy>();
        public CollectionThreadSafe<ColorBulbProxy> BulbsForAmbientLight { get; private set; } = new CollectionThreadSafe<ColorBulbProxy>();

        public int DeviceCount
        {
            get
            {
                return Bulbs.Count;
            }
        }

        public bool IsMusicModeON { get; private set; } = false;

        public AppCore(BulbRepository repository)
        {
            Repository = repository;
            Discoverer.ResponsesReceived += OnResponsesReceived;
            Discoverer.StartDiscover();
        }
        public List<ColorBulbProxy> GetDevices()
        {
            return new List<ColorBulbProxy>();
        }
        public CollectionThreadSafe<ColorBulbProxy> GetBulbs()
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
        public void ToggleAmbientLight(ColorBulbProxy bulb)
        {
            if (!BulbsForAmbientLight.Contains(bulb))
            {
                try
                {
                    Discoverer.StopDiscover();
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
        public void SetSceneHSV(ColorBulbProxy bulb, HSBColor color)
        {
            bulb.ExecuteCommand(BulbCommandBuilder.CreateSetSceneHsvCommand(
                CommandType.Stream, color.Hue, (int)color.Saturation, (int)color.Brightness));
        }

        void OnResponsesReceived(List<string> responses)
        {
            List<int> ids = Repository.GetDeviceIds();

            if (ids.Count == 0)
            {
                foreach (var response in responses)
                {
                    Repository.AddDevice(new ColorBulbProxy(response));
                }
            }
            else
            {
                foreach (var response in responses)
                {
                    foreach (var id in ids)
                    {
                        var tempId = id.ToString("X").ToLower();
                        if (!response.Contains(tempId))
                            Repository.AddDevice(new ColorBulbProxy(response));
                    }
                }
            }
        }
        public void Dispose()
        {

        }
    }
}
