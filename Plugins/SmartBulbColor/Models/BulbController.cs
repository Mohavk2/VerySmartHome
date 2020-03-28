using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VerySmartHome.MainController;

//MM - MusicMode

namespace SmartBulbColor.Models
{
    sealed class BulbController : DeviceController, IDisposable
    {
        public delegate void BulbCollectionNotifier();
        public event BulbCollectionNotifier BulbCollectionChanged;

        private Object Locker = new Object();

        public List<ColorBulb> _bulbs = new List<ColorBulb>();
        public List<ColorBulb> Bulbs 
        {
            get 
            {
                return _bulbs;
            }
            private set
            {
                _bulbs = value;
                OnBulbCollectionChanged();
            } 
        }
        public List<ColorBulb> BulbsForAmbientLight { get; private set; } = new List<ColorBulb>();
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

        public bool IsAmbientLightON { get; private set;} = false;

        public BulbController()
        {
            DeviceDiscoverer.DeviceFound += OnDeviceFound;
            DeviceDiscoverer.DeviceLost += OnDeviceLost;
            Discoverer.StartDiscover();
        }
        public override List<Device> GetDevices()
        {
            lock(Locker)
            {
                if (Bulbs.Count != 0)
                {
                    List<Device> devices = new List<Device>(Bulbs);
                    return devices;
                }
                else return new List<Device>();
            }
        }
        public List<ColorBulb> GetBulbs()
        {
            lock(Locker)
            {
                if (Bulbs.Count != 0)
                {
                    return Bulbs;
                }
                else
                {
                    //DiscoverBulbs();
                    return Bulbs;
                }
            }
        }
        public void DiscoverBulbs()
        {
            lock(Locker)
            {
                try
                {
                    Bulbs = Discoverer.FindDevices().Cast<ColorBulb>().ToList();
                }
                catch (Exception NoResponseException)
                {
                    throw NoResponseException;
                }
            }
        }
        public void StartBulbsRefreshing()
        {
            Discoverer.StartDiscover();
        }
        public void TogglePower(ColorBulb bulb)
        {
            lock(Locker)
            {
                bulb.TogglePower();
            }
        }
        public void NormalLight_ON()
        {
            lock(Locker)
            {
                AmbientLight_OFF();

                Thread.Sleep(500);
                if (Bulbs.Count != 0)
                {
                    foreach (var bulb in Bulbs)
                    {
                        bulb.SetNormalLight(5400, 100);
                        Console.WriteLine("Norm Light is On");
                    }
                }
            }
        }
        public void AmbientLight_ON()
        {
            lock(Locker)
            {
                try
                {
                    SetColorMode(2);
                    Discoverer.StopDiscover();
                    AmbientLight.SetBulbsForStreaming(Bulbs);
                    AmbientLight.StartSreaming();
                    IsAmbientLightON = true;
                }
                catch (Exception MusicModeFailedException)
                {
                    throw MusicModeFailedException;
                }
            }
        }
        public void AmbientLight_OFF()
        {
            lock(Locker)
            {
                AmbientLight.StopStreaming();
                IsAmbientLightON = false;
                Discoverer.StartDiscover();
            }
        }
        /// <summary>
        /// Sets color mode
        /// </summary>
        /// <param name="value"> 1 - CT mode, 2 - RGB mode , 3 - HSV mode</param>
        void SetColorMode(int value)
        {
            lock(Locker)
            {
                if (value > 0 & value <= 3)
                {
                    if (Bulbs.Count != 0)
                    {
                        foreach (var bulb in Bulbs)
                        {
                            bulb.SetColorMode(value);
                        }
                    }
                    else throw new Exception("Can't turn Music Mode ON becouse there is no found bulbs yet");
                }
            }
        }
        public List<String> GetDeviceReports()
        {
            lock(Locker)
            {
                List<String> reports = new List<string>();
                if (Bulbs.Count != 0)
                {
                    foreach (ColorBulb bulb in Bulbs)
                    {
                        reports.Add(bulb.GetReport());
                    }
                    return reports;
                }
                else
                {
                    reports.Add("No color bulbs found yet!!!");
                    return reports;
                }
            }
        }
        private void OnBulbCollectionChanged()
        {
            lock(Locker)
            {
                BulbCollectionChanged?.Invoke();
            }
        }
        public void SetSceneHSV(ColorBulb bulb, HSBColor color)
        {
            lock(Locker)
            {
                bulb.SetSceneHSV(color.Hue, color.Saturation, color.Brightness);
            }
        }

        private void OnDeviceFound(Device foundDevice)
        {
            Task task = new Task(() =>
            {
                lock (Locker)
                {
                    Bulbs.Add((ColorBulb)foundDevice);
                    OnBulbCollectionChanged();
                }
            });
            task.Start();
        }
        private void OnDeviceLost(Device lostDevice)
        {
            Task task = new Task(() =>
            {
                lock (Locker)
                {
                    Bulbs.Remove((ColorBulb)lostDevice);
                    OnBulbCollectionChanged();
                }
            });
            task.Start();
        }
        public void Dispose()
        {

        }
    }
}
