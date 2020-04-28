using SmartBulbColor.Infrastructure;
using SmartBulbColor.Models;
using System;
using System.Windows.Input;

namespace SmartBulbColor.ViewModels
{
    class MainWindowViewModel : ViewModelBase, IDisposable
    {

        BulbController Controller = new BulbController();

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
            Controller.BulbFound += (foundBulb)=> { ColorBulbsVM.AddSafe(new ColorBulbViewModel(Controller, foundBulb)); };
            Controller.StartBulbsRefreshing();
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
            Controller.DiscoverBulbs();
        }
        public void Dispose()
        {
            Controller.Dispose();
        }
    }
}
