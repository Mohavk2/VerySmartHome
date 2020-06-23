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

        public string Id { get; }

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

        private string _name;
        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
                OnPropertyChanged("Name");
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

        public ColorBulbViewModel(BulbDTO bulb, AppMediator mediator)
        {
            Id = bulb.Id;
            Mediator = mediator;
            UpdateBulb(bulb);
            Mediator.BulbUpdated += UpdateBulb;
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

        private void UpdateBulb(BulbDTO bulb)
        {
            if (Id == bulb.Id)
            {
                Bulb = bulb;
                Name = bulb.Name;
                IsPowered = bulb.IsPowered;
            }
        }
    }
}
