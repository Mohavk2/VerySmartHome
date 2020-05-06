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
                NameToInsert = _selectedGroupVM.GroupName;
                OnPropertyChanged("SelectedGroupVM");
            }
        }
        string _nameToInsert;
        public string NameToInsert 
        { 
            get => _nameToInsert;
            set
            {
                _nameToInsert = value;
                OnPropertyChanged("NameToInsert");
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

        public ICommand CreateNewGroup
        {
            get { return new ControllerCommand(ExecuteCreateNewGroup, CanExecuteCreateNewGroup); }
        }
        void ExecuteCreateNewGroup(object parametr)
        {
            GroupVMs.AddSafe(new GroupViewModel(Controller, NameToInsert, new DispatchedCollection<ColorBulbViewModel>()));
        }
        bool CanExecuteCreateNewGroup(object parametr)
        {
            bool NotExists = true;
            foreach (var group in GroupVMs)
            {
                if (group.GroupName == NameToInsert)
                    NotExists = false;
            }
            return (GroupVMs.Count < 20 && NameToInsert != "" && NotExists);
        }

        public ICommand RenameGroup
        {
            get { return new ControllerCommand(ExecuteRenameGroup, CanExecuteRenameGroup); }
        }
        void ExecuteRenameGroup(object parametr)
        {
            if (SelectedGroupVM.RenameGroup.CanExecute(NameToInsert))
                SelectedGroupVM.RenameGroup.Execute(NameToInsert);
        }
        bool CanExecuteRenameGroup(object parametr)
        {
            bool NotExists = true;
            foreach(var group in GroupVMs)
            {
                if (group.GroupName == NameToInsert)
                    NotExists = false;
            }
            return (SelectedGroupVM?.GroupName != "AllBulbs" && NotExists && NameToInsert != "");
        }
        public void Dispose()
        {

        }
    }
}
