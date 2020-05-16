using SmartBulbColor.Infrastructure;
using SmartBulbColor.Models;
using CommonLibrary;
using System;
using System.Windows.Input;
using SmartBulbColor.RemoteBulbAPI;

namespace SmartBulbColor.ViewModels
{
    internal class MainWindowViewModel : ViewModelBase, IDisposable
    {
        BulbController Controller;

        DeviceRepository<ColorBulb> Repository;

        public AllBulbsViewModel AllBulbsVM { get; }

        public DispatchedCollection<GroupViewModel> GroupVMs { get; set; } = new DispatchedCollection<GroupViewModel>();

        public string MainGroupName { get; } = "AllBulbs";

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
            Repository = new DeviceRepository<ColorBulb>();
            Controller = new BulbController(Repository);

            AllBulbsVM = new AllBulbsViewModel(MainGroupName, Controller, Repository);

            Repository.NewDeviceAdded += (bulb)=> AllBulbsVM.AddBulbVM(new ColorBulbViewModel(Controller, bulb));
        }

        public ICommand CreateNewGroup
        {
            get { return new ControllerCommand(ExecuteCreateNewGroup, CanExecuteCreateNewGroup); }
        }
        void ExecuteCreateNewGroup(object parametr)
        {
            GroupVMs.AddSafe(new GroupViewModel(NameToInsert, Controller, Repository));
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
            return (SelectedGroupVM?.GroupName != MainGroupName && NotExists && NameToInsert != "");
        }
        public void Dispose()
        {
            Controller.Dispose();
            Repository.Dispose();
        }
    }
}
