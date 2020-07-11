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
            Name = bulb.Name;
            IsPowered = bulb.IsPowered;

            Mediator = mediator;

            Mediator.BulbUpdated += (bulb) => Context.Post((state) => OnBulbUpdated(bulb), new object());
        }

        public void SetColor(SolidColorBrush brush)
        {
            Color color = brush.Color;
            HSBColor hsbColor = new HSBColor(color);
            Mediator.SetSceneHSV(Id, hsbColor);
            CurrentColor = brush;
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
            Mediator.TogglePower(Id);
            OnPropertyChanged("Bulb");
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
            Mediator.TurnNormalLightOn(Id);
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
            Mediator.ToggleAmbientLight(Id);
        }

        private void OnBulbUpdated(BulbDTO bulb)
        {
            if (Id == bulb.Id)
            {
                Name = bulb.Name;
                IsPowered = bulb.IsPowered;
            }
        }
    }
}
