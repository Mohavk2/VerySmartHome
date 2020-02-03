using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using SmartBulbColor.Infrastructure;

namespace SmartBulbColor.ViewModels
{
    class MainWindowViewModel : ViewModelBase
    {
        BulbController SmartBulbController = new BulbController();

        ObservableCollection<Bulb> bulbs;
        public ObservableCollection<Bulb> Bulbs
        {
            get
            {
                if (bulbs == null || bulbs.Count == 0)
                {
                    bulbs = SmartBulbController.GetBulbs();
                    return bulbs;
                }
                else
                {
                    return bulbs;
                }
            }
            set
            {
                if (value == null || value.GetType() != bulbs.GetType())
                {
                    throw new Exception("Can't add value to ViewModel Bulb collection. Value isn't correct");
                }
                else
                {
                    bulbs = value;
                    OnPropertyChanged("Bulbs");
                }
            }
        }
        Bulb selectedBulb;
        public Bulb SelectedBulb
        {
            get { return selectedBulb; }
            set
            {
                SelectedBulb = value;
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
                Bulbs = SmartBulbController.GetBulbs();
                OnPropertyChanged("Bulbs");
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
                        Logs = "Ambient Light is OFF";
                        OnPropertyChanged("Logs");
                    }
                    else
                    {
                        SmartBulbController.AmbientLight_ON();
                        Logs = "Ambient Light is ON";
                        OnPropertyChanged("Logs");
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
            Logs = "Ambient Light is OFF";
            OnPropertyChanged("Logs");
        }
    }
}
