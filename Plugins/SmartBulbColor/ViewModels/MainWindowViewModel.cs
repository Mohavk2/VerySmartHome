using System;
using System.Collections.ObjectModel;
using System.Threading;
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

        public DispatchedCollection<ColorBulbViewModel> ColorBulbsVM { get; } = new DispatchedCollection<ColorBulbViewModel>();

        ColorBulbViewModel _selectedBulbVM;
        public ColorBulbViewModel SelectedBulbVM
        {
            get { return _selectedBulbVM; }
            set
            {
                _selectedBulbVM = value;
                OnPropertyChanged("SelectedBulbVM");
            }
        }
        public MainWindowViewModel()
        {
            SmartBulbController.BulbFound += OnBulbFound;
            SmartBulbController.BulbLost += OnBulbLost;
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
            SmartBulbController.DiscoverBulbs();
        }  
        private void OnBulbFound(ColorBulb foundBulb)
        {
            ColorBulbsVM.AddSafe(new ColorBulbViewModel(foundBulb));
        }
        private void OnBulbLost(ColorBulb lostBulb)
        {
            ColorBulbsVM.RemoveSafe(new ColorBulbViewModel(lostBulb));
        }
        private void OnBulbGroupChanged(ColorBulb bulb, ColorBulbGroup groupfrom, ColorBulbGroup groupTo)
        {

        }
        public void Dispose()
        {
            SmartBulbController.Dispose();
        }
    }
}
