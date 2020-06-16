using SmartBulbColor.Domain;
using SmartBulbColor.PluginApp;
using System;
using System.Windows.Input;
using System.Windows.Media;

namespace SmartBulbColor.ViewModels
{
    internal class ColorBulbViewModel : ViewModelBase
    {
        readonly Mediator Controller;
        private ColorBulbProxy _bulb;
        public ColorBulbProxy Bulb
        {
            get { return _bulb; }
            set
            {
                _bulb = value;
                OnPropertyChanged("Bulb");
            }
        }
        private SolidColorBrush _currentColor = Brushes.White;
        public SolidColorBrush CurrentColor
        {
            get { return _currentColor; }
            set
            {
                _currentColor = value;
                OnPropertyChanged("CurrentColor");
            }
        }
        private bool _isControlEnabled;
        public bool IsControlEnabled
        {
            get { return _isControlEnabled; }
            set
            {
                _isControlEnabled = value;
                OnPropertyChanged("IsControlEnabled");
            }
        }
        public int GetId() { return Bulb.Id; }
        private bool _isPowered;
        public bool IsPowered
        {
            get
            {
                _isPowered = Bulb.IsPowered;
                return _isPowered;
            }
            set
            {
                _isPowered = value;
                OnPropertyChanged("IsPowered");
            }
        }
        
        public ColorBulbViewModel(Mediator controller, ColorBulbProxy bulb)
        {
            Controller = controller;
            Bulb = bulb;
        }
        public void SetColor(SolidColorBrush brush)
        {
            Color color = brush.Color;
            HSBColor hsbColor = new HSBColor(color);
            int hue = hsbColor.Hue;
            int sat = (int)hsbColor.Saturation;
            int bright = (int)hsbColor.Brightness;
            var command = BulbCommandBuilder.CreateSetSceneHsvCommand(CommandType.Stream, hue, sat, bright);
            Bulb.PushCommand(command);
            CurrentColor = brush;
            IsPowered = Bulb.IsPowered;
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
            Bulb.PushCommand(BulbCommandBuilder.CreateToggleCommand());
            OnPropertyChanged("Bulb");
            IsPowered = Bulb.IsPowered;
        }
        private bool CanExecuteTogglePower(Object parametr)
        {
            if (Bulb == null)
            {
                return false;
            }
            return true;
        }
        public ICommand TurnNormalLightON
        {
            get
            {
                return new ControllerCommand(ExecuteTurnNormalLightON, CanExecuteTurnNormalLightON);
            }
        }
        private void ExecuteTurnNormalLightON(Object parametr)
        {
            Bulb.PushCommand(BulbCommandBuilder.CreateSetSceneColorTemperatureCommand(CommandType.RefreshState, 5400, 100));
            IsPowered = Bulb.IsPowered;
            CurrentColor = Brushes.White;
        }
        private bool CanExecuteTurnNormalLightON(Object parametr)
        {
            if (Bulb == null)
            {
                return false;
            }
            return true;
        }
        public ICommand ToggleAmbientLight
        {
            get
            {
                return new ControllerCommand(ExecuteToggleAmbientLight);
            }
        }
        private void ExecuteToggleAmbientLight(Object parametr)
        {
            Controller.ToggleAmbientLight(Bulb);
        }
    }
}
