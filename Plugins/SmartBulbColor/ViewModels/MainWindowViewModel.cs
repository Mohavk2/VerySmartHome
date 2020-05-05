using SmartBulbColor.Infrastructure;
using SmartBulbColor.Models;
using CommonLibrary;
using System;
using System.Windows.Input;
using SmartBulbColor.RemoteBulbAPI;

namespace SmartBulbColor.ViewModels
{
    class MainWindowViewModel : ViewModelBase, IDisposable
    {
        BulbController Controller;

        DeviceRepository<ColorBulb> DevicePerository;

        public DispatchedCollection<GroupViewModel> GroupVMs { get; set; } = new DispatchedCollection<GroupViewModel>();

        GroupViewModel _selectedGroupVM;
        public GroupViewModel SelectedGroupVM
        {
            get { return _selectedGroupVM; }
            set
            {
                _selectedGroupVM = value;
                OnPropertyChanged("SelectedGroupVM");
            }
        }
        public MainWindowViewModel()
        {
            DevicePerository = new DeviceRepository<ColorBulb>();
            Controller = new BulbController(DevicePerository);

            var allBulbs = DevicePerository.GetDevices();
            var bulbViewModels = new DispatchedCollection<ColorBulbViewModel>();
            foreach (var bulb in allBulbs)
            {
                bulbViewModels.Add(new ColorBulbViewModel(Controller, bulb));
            }
            var mainGroupViewModel = new GroupViewModel(Controller, "AllBulbs", bulbViewModels);

            DevicePerository.NewDeviceAdded += (bulb)=> mainGroupViewModel.AddBulbVM(new ColorBulbViewModel(Controller, bulb));
            GroupVMs.AddSafe(mainGroupViewModel);
        }
        public void Dispose()
        {

        }
    }
}
