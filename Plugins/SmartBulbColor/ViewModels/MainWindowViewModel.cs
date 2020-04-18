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
            SmartBulbController.BulbFound += (foundBulb)=> { ColorBulbsVM.AddSafe(new ColorBulbViewModel(foundBulb)); };
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
        public ICommand ToggleAmbientLight
        {
            get
            {
                return new ControllerCommand(ExecuteToggleAmbientLightCommand);
            }
        }
        private void ExecuteToggleAmbientLightCommand(Object parametr)
        {
            SmartBulbController.ToggleAmbientLight(SelectedBulbVM.Bulb);
        }
        public void Dispose()
        {
            SmartBulbController.Dispose();
        }
    }
}
