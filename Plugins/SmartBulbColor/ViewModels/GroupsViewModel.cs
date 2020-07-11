using SmartBulbColor.PluginApp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Windows.Input;

namespace SmartBulbColor.ViewModels
{
    internal class GroupsViewModel : ViewModelBase
    {
        SynchronizationContext Context = SynchronizationContext.Current;
        AppMediator Mediator;

        public ObservableCollection<GroupViewModel> GroupVMs { get; set; } = new ObservableCollection<GroupViewModel>();
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
        public GroupsViewModel(AppMediator mediator)
        {
            Mediator = mediator;
            var groups = Mediator.GetGroups();

            Mediator.GroupCreated += (group) => Context.Post((state) => OnGroupCreated(group), new object());
            Mediator.GroupDeleted += (group) => Context.Post((state) => OnGroupDeleted(group), new object());
        }

        public ICommand CreateNewGroup
        {
            get { return new ControllerCommand(ExecuteCreateNewGroup, CanExecuteCreateNewGroup); }
        }

        void ExecuteCreateNewGroup(object parametr)
        {
            Mediator.CreateGroup(NameToInsert);
            NameToInsert = "";
        }

        bool CanExecuteCreateNewGroup(object parametr)
        {
            bool NotExists = true;

            foreach (var group in GroupVMs)
            {
                if (group.Name == NameToInsert)
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
            Mediator.RenameGroup(SelectedGroupVM.Id, NameToInsert);
            NameToInsert = "";
        }

        bool CanExecuteRenameGroup(object parametr)
        {
            bool NotExists = true;
            foreach (var group in GroupVMs)
            {
                if (group.Name == NameToInsert)
                    NotExists = false;
            }
            return (NotExists && NameToInsert != "");
        }

        private void OnGroupDeleted(GroupDTO group)
        {
            GroupViewModel groupVmToRemove = null;
            foreach (var groupVM in GroupVMs)
            {
                if (groupVM.Id == group.Id)
                    groupVmToRemove = groupVM;
            }
            if (groupVmToRemove != null)
                GroupVMs.Remove(groupVmToRemove);
        }

        private void OnGroupCreated(GroupDTO group)
        {
            var isContains = false;
            foreach (var groupVM in GroupVMs)
            {
                if (groupVM.Id == group.Id)
                    isContains = true;
            }
            if (isContains == false)
                GroupVMs.Add(new GroupViewModel(group, Mediator));
        }
    }
}
