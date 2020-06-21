using SmartBulbColor.PluginApp;
using System;
using System.Threading;
using System.Windows.Input;
using System.Windows.Media;

namespace SmartBulbColor.ViewModels
{
    internal class ColorBulbViewModel : ViewModelBase
    {
        SynchronizationContext Context = SynchronizationContext.Current;
        readonly AppMediator Mediator;

        private BulbDTO _bulb;
        public BulbDTO Bulb
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
        public string GetId() { return Bulb.Id; }
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
        
        public ColorBulbViewModel(AppMediator mediator, BulbDTO bulb)
        {
            Mediator = mediator;
            Bulb = bulb;
        }

        public void SetColor(SolidColorBrush brush)
        {
            Color color = brush.Color;
            HSBColor hsbColor = new HSBColor(color);
            Mediator.SetSceneHSV(Bulb, hsbColor);
            CurrentColor = brush;
            IsPowered = Bulb.IsPowered;
        }

        public ICommand TogglePower
        {
            get
            {
                return new ControllerCommand(ExecuteTogglePower);
            }
        }
        private void ExecuteTogglePower(Object parametr)
        {
            Mediator.TogglePower(Bulb);
            OnPropertyChanged("Bulb");
            IsPowered = Bulb.IsPowered;
        }
        public ICommand TurnNormalLightON
        {
            get
            {
                return new ControllerCommand(ExecuteTurnNormalLightON);
            }
        }
        private void ExecuteTurnNormalLightON(Object parametr)
        {
            Mediator.TurnNormalLightOn(Bulb);  
            IsPowered = Bulb.IsPowered;
            CurrentColor = Brushes.White;
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
            Mediator.ToggleAmbientLight(Bulb);
        }
    }
}
