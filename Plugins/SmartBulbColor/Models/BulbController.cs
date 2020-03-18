﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using VerySmartHome.MainController;
using VerySmartHome.Interfaces;

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
        public override List<IDevice> GetDevices()
        {
            if (Bulbs.Count != 0)
            {
                List<IDevice> devices = new List<IDevice>(Bulbs);
                return devices;
            }
            else return new List<IDevice>();
        }
        public List<ColorBulb> GetBulbs()
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
        public void DiscoverBulbs()
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
        public void StartBulbsRefreshing()
        {
            Discoverer.StartDiscover();
        }
        public void TogglePower(ColorBulb bulb)
        {
            bulb.TogglePower();
        }
        public void NormalLight_ON()
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
        public void AmbientLight_ON()
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
        public void AmbientLight_OFF()
        {
            AmbientLight.StopStreaming();
            IsAmbientLightON = false;
            Discoverer.StartDiscover();
        }
        /// <summary>
        /// Sets color mode
        /// </summary>
        /// <param name="value"> 1 - CT mode, 2 - RGB mode , 3 - HSV mode</param>
        void SetColorMode(int value)
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
        public List<String> GetDeviceReports()
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
        void RefreshBulbCollection(List<IDevice> foundBulbs)
        {
            Bulbs = foundBulbs.Cast<ColorBulb>().ToList();
        }
        private void OnBulbCollectionChanged()
        {
            BulbCollectionChanged?.Invoke();
        }
        public void SetSceneHSV(ColorBulb bulb, HSBColor color)
        {
            bulb.SetSceneHSV(color.Hue, color.Saturation, color.Brightness);
        }

        private void OnDeviceFound(IDevice foundDevice)
        {
            lock(Locker)
            {
                Bulbs.Add((ColorBulb)foundDevice);
                OnBulbCollectionChanged();
            }
        }
        private void OnDeviceLost(IDevice foundDevice)
        {
            lock (Locker)
            {
                Bulbs.Remove((ColorBulb)foundDevice);
                OnBulbCollectionChanged();
            }
        }
        public void Dispose()
        {

        }
    }
}
