using SmartBulbColor.BulbCommands;
using SmartBulbColor.Infrastructure;
using SmartBulbColor.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace SmartBulbColor.ViewModels
{
    internal class ColorBulbViewModel : ViewModelBase
    {
        readonly BulbController Controller;
        private ColorBulb _bulb;
        public ColorBulb Bulb
        {
            get { return _bulb; }
            set
            {
                _bulb = value;
                OnPropertyChanged("Bulb");
            }
        }
        private SolidColorBrush _pickerBrush = Brushes.White;
        public SolidColorBrush PickerBrush
        {
            get { return _pickerBrush; }
            set
            {
                _pickerBrush = value;
                if (SetColorByColorPickerPoint.CanExecute(value))
                {
                    SetColorByColorPickerPoint.Execute(value);
                }
            }
        }
        private bool _isControlEnabled;
        public bool IsControlEnabled
        {
            get { return IsControlEnabled; }
            set
            {
                _isControlEnabled = value;
                OnPropertyChanged("IsControlEnabled");
            }
        }
        public int GetId() { return Bulb.GetId(); }
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
        public ColorBulbViewModel(BulbController controller, ColorBulb bulb)
        {
            Controller = controller;
            Bulb = bulb;
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
            HSBColor hsbColor = new HSBColor(color);
            Bulb.ExecuteCommand(BulbCommandBuilder.CreateSetSceneHsvCommand(hsbColor.Hue, (int)hsbColor.Saturation, (int)hsbColor.Brightness));
            IsPowered = Bulb.IsPowered;
        }
        private bool CanExecuteSetColorByColorPickerPoint(Object parametr)
        {
            if (Bulb == null)
            {
                return false;
            }
            return true;
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
            Bulb.ExecuteCommand(BulbCommandBuilder.CreateToggleCommand());
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
                return new ControllerCommand(ExecuteTurnNormalLightONCommand, CanExecuteTurnNormalLightONCommand);
            }
        }
        private void ExecuteTurnNormalLightONCommand(Object parametr)
        {
            Bulb.ExecuteCommand(BulbCommandBuilder.CreateSetSceneColorTemperatureCommand(5400, 100));
            IsPowered = Bulb.IsPowered;
            _pickerBrush = Brushes.White;
            OnPropertyChanged("PickerBrush");
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
            Controller.ToggleAmbientLight(Bulb);
        }
        private bool CanExecuteTurnNormalLightONCommand(Object parametr)
        {
            if(Bulb == null)
            {
                return false;
            }
            return true;
        }
    }
}
