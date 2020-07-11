using SmartBulbColor.PluginApp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Windows.Input;
using System.Windows.Media;

namespace SmartBulbColor.ViewModels
{
    class GroupViewModel : ViewModelBase
    {
        SynchronizationContext Context = SynchronizationContext.Current;
        private AppMediator Mediator;

        public delegate void GroupRenamedHandler(string currentGroupName, string newGroupName);
        public event GroupRenamedHandler GroupRenamed;

        public GroupDTO Group { get; set; }

        public string Id { get; }

        string _groupName;
        public string Name
        {
            get => _groupName;
            set
            {
                var temp = _groupName;
                _groupName = value;
                OnPropertyChanged("GroupName");
                OnGroupRenamed(temp, value);
            }
        }

        public ObservableCollection<ColorBulbViewModel> ColorBulbVMs { get; set; } = new ObservableCollection<ColorBulbViewModel>();

        List<ColorBulbViewModel> _selectedBulbVMs = new List<ColorBulbViewModel>();
        public List<ColorBulbViewModel> SelectedBulbVMs
        {
            get { return _selectedBulbVMs; }
            set
            {
                _selectedBulbVMs = value;
                OnPropertyChanged("SelectedBulbVMs");
            }
        }
        private SolidColorBrush _pickerBrush = Brushes.White;
        public SolidColorBrush PickerBrush
        {
            get { return _pickerBrush; }
            set
            {
                _pickerBrush = value;
                SetColorWithBrush(value);
            }
        }
        public GroupViewModel(GroupDTO group, AppMediator mediator)
        {
            Id = group.Id;
            Name = group.Name;
            Mediator = mediator;

            Mediator.GroupUpdated += (group) => Context.Post((state) => OnGroupUpdated(group), new object());
        }

        public ICommand RenameGroup
        {
            get { return new ControllerCommand(ExecuteRenameGroup, CanExecuteRenameGroup); }
        }
        void ExecuteRenameGroup(object parametr)
        {
            Name = parametr as string;
        }
        bool CanExecuteRenameGroup(object parametr)
        {
            var name = parametr as string;
            return (name != null && name != "");
        }
        public ICommand TogglePower
        {
            get { return new ControllerCommand(ExecuteTogglePower, CanExecuteTogglePower); }
        }
        void ExecuteTogglePower(object parametr)
        {
            foreach (var bulbVM in SelectedBulbVMs)
            {
                if (bulbVM.TogglePower.CanExecute(parametr))
                    bulbVM.TogglePower.Execute(parametr);
            }
        }
        bool CanExecuteTogglePower(object parametr)
        {
            if (SelectedBulbVMs == null || SelectedBulbVMs.Count == 0)
                return false;
            else
                return true;
        }
        public ICommand SetNormalLight
        {
            get { return new ControllerCommand(ExecuteSetNormalLight, CanExecuteSetNormalLight); }
        }
        void ExecuteSetNormalLight(object parametr)
        {
            foreach (var bulbVM in SelectedBulbVMs)
            {
                if (bulbVM.TurnNormalLightON.CanExecute(parametr))
                    bulbVM.TurnNormalLightON.Execute(parametr);
            }
        }
        bool CanExecuteSetNormalLight(object parametr)
        {
            if (SelectedBulbVMs == null || SelectedBulbVMs.Count == 0)
                return false;
            else
                return true;
        }
        public ICommand ToggleAmbientLight
        {
            get { return new ControllerCommand(ExecuteToggleAmbientLight, CanExecuteToggleAmbientLight); }
        }
        void ExecuteToggleAmbientLight(object parametr)
        {
            foreach (var bulbVM in SelectedBulbVMs)
            {
                if (bulbVM.ToggleAmbientLight.CanExecute(parametr))
                    bulbVM.ToggleAmbientLight.Execute(parametr);
            }
        }
        bool CanExecuteToggleAmbientLight(object parametr)
        {
            if (SelectedBulbVMs == null || SelectedBulbVMs.Count == 0)
                return false;
            else
                return true;
        }
        public void AddBulbVM(ColorBulbViewModel bulbVM)
        {
            Context.Post((object state) => { ColorBulbVMs.Add(bulbVM); }, new object());
        }
        private void SetColorWithBrush(Object parametr)
        {
            if (SelectedBulbVMs != null && SelectedBulbVMs.Count != 0)
            {
                var brush = (SolidColorBrush)parametr;

                foreach (var bulbVM in SelectedBulbVMs)
                {
                    bulbVM.SetColor(brush);
                }
            }
        }
        private void OnGroupUpdated(GroupDTO group)
        {
            if(Id == group.Id)
            {
                Name = group.Name;

                var bulbList = group.Bulbs;

                foreach (var bulb in bulbList)
                {
                    var isContains = false;
                    foreach (var bulbVM in ColorBulbVMs)
                    {
                        if (bulbVM.Id == bulb.Id)
                            isContains = true;
                    }
                    if (isContains == false)
                        ColorBulbVMs.Add(new ColorBulbViewModel(bulb, Mediator));
                }

                ColorBulbViewModel bulbVmToRemove = null;

                foreach(var bulbVM in ColorBulbVMs)
                {
                    var isContains = false;
                    foreach(var bulb in bulbList)
                    {
                        if(bulb.Id == bulbVM.Id)
                            isContains = true;
                    }
                    if (isContains == false)
                        bulbVmToRemove = bulbVM;
                }
                ColorBulbVMs.Remove(bulbVmToRemove);
            }
        }

        void OnGroupRenamed(string currentGroupName, string newGroupName)
        {
            GroupRenamed?.Invoke(currentGroupName, newGroupName);
        }
    }
}
