using System;
using System.Collections.ObjectModel;
using System.Threading;
//using System.Drawing;
using System.Windows.Input;
using System.Windows.Media;
using SmartBulbColor.Infrastructure;
using SmartBulbColor.Models;
using VerySmartHome.MainController;

namespace SmartBulbColor.ViewModels
{
    class MainWindowViewModel : ViewModelBase, IDisposable
    {

        BulbController SmartBulbController = new BulbController();
        
        Object RefresherLocker = new Object();

        BulbCollectionUIThreadSafe _bulbs = new BulbCollectionUIThreadSafe();
        public ObservableCollection<ColorBulb> Bulbs
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
        ColorBulb _selectedBulb;
        public ColorBulb SelectedBulb
        {
            get { return _selectedBulb; }
            set
            {
                _selectedBulb = value;
                OnPropertyChanged("SelectedBulb");
            }
        }

        BulbCollectionUIThreadSafe _currentBulbs = new BulbCollectionUIThreadSafe();
        public ObservableCollection<ColorBulb> CurrentBulbs
        {
            get
            {
                return _currentBulbs;
            }
            set
            {
                if (value == null)
                {
                    throw new Exception("Can't add value to ViewModel Bulb collection. Value isn't correct");
                }
                else
                {
                    _currentBulbs.RefreshSafe(value);
                    OnPropertyChanged("CurrentBulbs");
                }

            }
        }
        private SolidColorBrush _pickerBrush = Brushes.Black;
        public SolidColorBrush PickerBrush
        {
            get { return _pickerBrush; }
            set
            {
                _pickerBrush = value;
                if(SetColorByColorPickerPoint.CanExecute(value))
                {
                    SetColorByColorPickerPoint.Execute(value);
                }
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
            SmartBulbController.BulbCollectionChanged += RefreshBulbCollection;
            SmartBulbController.StartBulbsRefreshing();
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
        public ICommand MoveToCurrentBulbs
        {
            get
            {
                return new ControllerCommand(ExecuteMoveToCurrentBulbs, CanExecuteMoveToCurrentBulbs);
            }
        }
        private void ExecuteMoveToCurrentBulbs(Object parametr)
        {
            CurrentBulbs.Add(SelectedBulb);
            Bulbs.Remove(SelectedBulb);
        }
        private bool CanExecuteMoveToCurrentBulbs(Object parametr)
        {
            if(Bulbs == null || SelectedBulb == null || SmartBulbController.IsAmbientLightON)
            {
                return false;
            }
            return true;
        }


        //////////////////// 
        /// Bulb Commands///
        ////////////////////
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
                SmartBulbController.DiscoverBulbs();
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
                        if (SmartBulbController.IsAmbientLightON == false)
                        {
                            Logs = "Ambient Light is OFF";
                            OnPropertyChanged("Logs");
                        }
                    }
                    else
                    {
                        SmartBulbController.AmbientLight_ON();
                        if (SmartBulbController.IsAmbientLightON == true)
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
        public ICommand SetColorByColorPickerPoint
        {
            get
            {
                return new ControllerCommand(ExecuteSetColorByColorPickerPoint, CanExecuteSetColorByColorPickerPoint);
            }
        }
        private void ExecuteSetColorByColorPickerPoint(Object parametr)
        {
            var brush = (SolidColorBrush)parametr;
            Color color = brush.Color;
            SmartBulbController.SetSceneHSV(SelectedBulb, new HSBColor(color));
        }
        private bool CanExecuteSetColorByColorPickerPoint(Object parametr)
        {
            if (Bulbs == null || SelectedBulb == null || SmartBulbController.IsAmbientLightON)
            {
                return false;
            }
            return true;
        }

        private void RefreshBulbCollection()
        {            
            lock(RefresherLocker)
            {
                var bulbsThatAreOnline = new ObservableCollection<ColorBulb>(SmartBulbController.GetBulbs());
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
