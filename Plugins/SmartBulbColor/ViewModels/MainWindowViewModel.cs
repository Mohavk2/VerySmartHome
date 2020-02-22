using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;
using SmartBulbColor.Infrastructure;
using SmartBulbColor.Models;

namespace SmartBulbColor.ViewModels
{
    class MainWindowViewModel : ViewModelBase, IDisposable
    {
        BulbController SmartBulbController = new BulbController();
        Object RefresherLocker = new Object();
        BulbCollectionUIThreadSafe _bulbs = new BulbCollectionUIThreadSafe();
        public ObservableCollection<BulbColor> Bulbs
        {
            get
            {
                return _bulbs;
            }
            set
            {
                if (value == null)
                {
                    throw new Exception("Can't add value to ViewModel Bulb collection. Value isn't correct");
                }
                else
                {
                    _bulbs.RefreshSafe(value);
                    OnPropertyChanged("Bulbs");
                }
                
            }
        }
        BulbColor _selectedBulb;
        public BulbColor SelectedBulb
        {
            get { return _selectedBulb; }
            set
            {
                _selectedBulb = value;
                OnPropertyChanged("SelectedBulb");
            }
        }

        string LogsToSave;
        string currentLogs = "..\r\n";
        public string Logs
        {
            get
            {
                return currentLogs;
            }
            set
            {
                if(value == "" || value == string.Empty)
                {
                    currentLogs = string.Empty;
                }
                else
                {
                    currentLogs += value + "\r\n";
                    LogsToSave += value + "\r\n";
                }
                OnPropertyChanged("Logs");
            }
        }

        public MainWindowViewModel()
        {
            SmartBulbController.BulbCollectionChanged += RefreshBulbs;
            SmartBulbController.StartBulbsRefreshing();
        }
        public ICommand FindBulbs
        {
            get
            {
                return new ControllerCommand(ExecuteFindBulbsCommand);
            }
        }
        public void ExecuteFindBulbsCommand(Object parametr)
        {
            try
            {
                SmartBulbController.ConnectBulbs_MusicMode();
                Bulbs = new BulbCollectionUIThreadSafe(SmartBulbController.GetBulbs());
                var reports = SmartBulbController.GetDeviceReports();
                foreach (var report in reports)
                {
                    Logs = report;
                }
                Logs = SmartBulbController.DeviceCount + " bulbs found";
            }
            catch (Exception NoDeviceException)
            {
                Logs = NoDeviceException.Message;
            }
        }

        public ICommand ClearConsole
        {
            get
            {
                return new ControllerCommand(ExecuteClearConsoleCommand);
            }
        }
        private void ExecuteClearConsoleCommand(Object parametr)
        {
            Logs = string.Empty;
            OnPropertyChanged("Logs");
        }

        public ICommand ToggleAmbientLight
        {
            get
            {
                return new ControllerCommand(ExecuteToggleAmbientLightCommand);
            }
        }
        private void ExecuteToggleAmbientLightCommand(Object parametr)
        {
            if (SmartBulbController.DeviceCount != 0)
            {
                try
                {
                    if (SmartBulbController.IsAmbientLightON)
                    {
                        SmartBulbController.AmbientLight_OFF();
                        if(SmartBulbController.IsAmbientLightON == false)
                        {
                            Logs = "Ambient Light is OFF";
                            OnPropertyChanged("Logs");
                        }
                    }
                    else
                    {
                        SmartBulbController.AmbientLight_ON();
                        if(SmartBulbController.IsAmbientLightON == true)
                        {
                            Logs = "Ambient Light is ON";
                            OnPropertyChanged("Logs");
                        }
                    }
                }
                catch (Exception MusicModeFailedException)
                {
                    Logs = MusicModeFailedException.Message.ToString();
                    OnPropertyChanged("Logs");
                }
            }
            else
            {
                Logs = "There is no found bulbs yet, please use \"Find Devices\" first";
            }
        }

        public ICommand TurnNormalLightON
        {
            get
            {
                return new ControllerCommand(ExecuteTurnNormalLightONCommand);
            }
        }
        private void ExecuteTurnNormalLightONCommand(Object parametr)
        {
            SmartBulbController.NormalLight_ON();
            if(SmartBulbController.IsAmbientLightON == false)
            {
                Logs = "Ambient Light is OFF";
                OnPropertyChanged("Logs");
            }
        }
        public ICommand TogglePower
        {
            get
            {
                return new ControllerCommand(ExecuteTogglePower, CanExecuteTogglePower);
            }
        }
        private void ExecuteTogglePower(Object parametr)
        {
            SmartBulbController.TogglePower(SelectedBulb);
            OnPropertyChanged("SelectedBulb");
        }
        private bool CanExecuteTogglePower(Object parametr)
        {
            if(Bulbs == null || Bulbs.Count == 0 || SelectedBulb == null || SmartBulbController.IsAmbientLightON)
            {
                return false;
            }
            return true;
        }
        private void RefreshBulbs()
        {            
            lock(RefresherLocker)
            {
                var bulbsThatAreOnline = new ObservableCollection<BulbColor>(SmartBulbController.GetBulbs());
                if (bulbsThatAreOnline != null && bulbsThatAreOnline.Count != 0)
                {
                    Bulbs = bulbsThatAreOnline;
                }
            }
        }

        public void Dispose()
        {
            SmartBulbController.Dispose();
        }
    }
}
