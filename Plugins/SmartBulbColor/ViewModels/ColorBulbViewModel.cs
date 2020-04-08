using SmartBulbColor.Infrastructure;
using SmartBulbColor.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using VerySmartHome.Interfaces;

namespace SmartBulbColor.ViewModels 
{
    internal delegate void GroupChangedHandler(ColorBulb bulb, ColorBulbGroup from, ColorBulbGroup to);
    public enum ColorBulbGroup { Common = 0, AmbientLight = 1 }
    internal class ColorBulbViewModel : ViewModelBase , IComparableById
    {
        public static event GroupChangedHandler GroupChanged;
        private static void OnGroupChanged(ColorBulb bulb, ColorBulbGroup from, ColorBulbGroup to)
        {
            GroupChanged?.Invoke(bulb, from, to);
        }

        public ColorBulb Bulb;
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
        public ColorBulbGroup CurrentGroup = ColorBulbGroup.Common;
        public ColorBulbGroup DefaultGroup = ColorBulbGroup.Common;
        public ColorBulbViewModel(ColorBulb bulb)
        {
            Bulb = bulb;
        }
        private SolidColorBrush _pickerBrush = Brushes.Black;
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
            Bulb.SetSceneHSV(hsbColor.Hue, hsbColor.Saturation, hsbColor.Brightness);
        }
        private bool CanExecuteSetColorByColorPickerPoint(Object parametr)
        {
            if (Bulb == null || CurrentGroup == ColorBulbGroup.AmbientLight)
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
            Bulb.TogglePower();
            OnPropertyChanged("Bulb");
            IsPowered = Bulb.IsPowered;
        }
        private bool CanExecuteTogglePower(Object parametr)
        {
            if (Bulb == null || CurrentGroup == ColorBulbGroup.AmbientLight)
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
            Bulb.SetNormalLight(5400, 100);
            IsPowered = Bulb.IsPowered;
        }
        private bool CanExecuteTurnNormalLightONCommand(Object parametr)
        {
            if(Bulb == null || CurrentGroup == ColorBulbGroup.AmbientLight)
            {
                return false;
            }
            return true;
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
            if(CurrentGroup != ColorBulbGroup.AmbientLight)
            {
                CurrentGroup = ColorBulbGroup.AmbientLight;
                OnGroupChanged(Bulb, DefaultGroup , CurrentGroup);
                IsPowered = Bulb.IsPowered;
            }
            else
            {
                var current = CurrentGroup;
                CurrentGroup = DefaultGroup;
                OnGroupChanged(Bulb, current, DefaultGroup);
            }
        }
    }
}
