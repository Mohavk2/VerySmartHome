using SmartBulbColor.PluginApp;
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
                NameToInsert = _selectedGroupVM.GroupName;
                OnPropertyChanged("SelectedGroupVM");
            }
        }
        public ObservableCollection<string> GroupNames { get; set; } = new ObservableCollection<string>();
        string _selectedGroupName;
        public string SelectedGroupName
        {
            get => _selectedGroupName;
            set
            {
                _selectedGroupName = value;
                OnPropertyChanged("SelectedGroupName");
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
            Mediator.GroupsUpdated += (groups) => Context.Post((state) => UpdateGroups(groups), new object());
            UpdateGroups(Mediator.GetGroups());
        }

        public ICommand CreateNewGroup
        {
            get { return new ControllerCommand(ExecuteCreateNewGroup, CanExecuteCreateNewGroup); }
        }

        void ExecuteCreateNewGroup(object parametr)
        {
            Mediator.AddGroup(NameToInsert);
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
            {
                SelectedGroupVM.RenameGroup.Execute(NameToInsert);
            }
        }

        bool CanExecuteRenameGroup(object parametr)
        {
            bool NotExists = true;
            foreach (var group in GroupVMs)
            {
                if (group.GroupName == NameToInsert)
                    NotExists = false;
            }
            return (NotExists && NameToInsert != "");
        }

        private void UpdateGroups(List<GroupDTO> groups)
        {
            GroupVMs.Clear();
            GroupNames.Clear();

            foreach (var group in groups)
            {
                var newGroup = new GroupViewModel(group, Mediator);
                newGroup.GroupRenamed += OnGroupRenamed;
                GroupVMs.Add(newGroup);
                GroupNames.Add(group.Name);
            }
        }

        private void OnGroupRenamed(string currentName, string newName)
        {
            GroupNames.Remove(currentName);
            GroupNames.Add(newName);
        }
    }
}
