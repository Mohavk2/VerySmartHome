using SmartBulbColor.RemoteBulbAPI;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CommonLibrary;

namespace SmartBulbColor.Models
{
    sealed class BulbController : DeviceController, IDisposable
    {
        static DeviceSearchingAtributes Atributes = new DeviceSearchingAtributes
        {
            SsdpMessage = "M-SEARCH * HTTP/1.1\r\n" + "HOST: 239.255.255.250:1982\r\n" + "MAN: \"ssdp:discover\"\r\n" + "ST: wifi_bulb",
            DeviceType = "MiBulbColor",
            MulticastPort = 1982
        };
        readonly DeviceDiscoverer Discoverer = new DeviceDiscoverer(Atributes, new BulbFactory());
        readonly DeviceRepository<ColorBulb> Repository;
        private readonly AmbientLightStreamer AmbientLight = new AmbientLightStreamer();

        public CollectionThreadSafe<ColorBulb> Bulbs { get; } = new CollectionThreadSafe<ColorBulb>();
        public CollectionThreadSafe<ColorBulb> BulbsForAmbientLight { get; private set; } = new CollectionThreadSafe<ColorBulb>();

        public override int DeviceCount
        {
            get
            {
                return Bulbs.Count;
            }
        }

        public bool IsMusicModeON { get; private set; } = false;

        public BulbController(DeviceRepository<ColorBulb> repository)
        {
            Repository = repository;
            Discoverer.DeviceFound += (device) => Repository.AddDevice((ColorBulb)device);
            Discoverer.SetIdsToIgnore(new List<int>(Repository.GetDeviceIds()));
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
        public void ToggleAmbientLight(ColorBulb bulb)
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
        public void SetSceneHSV(ColorBulb bulb, HSBColor color)
        {
            bulb.ExecuteCommand(BulbCommandBuilder.CreateSetSceneHsvCommand(
                CommandType.Stream, color.Hue, (int)color.Saturation, (int)color.Brightness));
        }
        public void Dispose()
        {

        }
    }
}
