using SmartBulbColor.PluginApp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Windows.Input;
using System.Windows.Media;

namespace SmartBulbColor.ViewModels
{
    internal class AllBulbsViewModel : ViewModelBase
    {
        SynchronizationContext Context = SynchronizationContext.Current;

        private AppMediator Mediator;

        public ObservableCollection<string> GroupNames { get; set; } = new ObservableCollection<string>();

        string _selectedGroupName;
        public string SelectedGroupName
        {
            get => _selectedGroupName;
            set
            {
                _selectedGroupName = value;
                OnPropertyChanged("SelectedGroupName");
                AddToGroup.Execute(new object());
            }
        }

        public ObservableCollection<ColorBulbViewModel> ColorBulbVMs { get; set; }

        List<ColorBulbViewModel> _selectedBulbVMs;
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

        public AllBulbsViewModel(AppMediator mediator)
        {
            ColorBulbVMs = new ObservableCollection<ColorBulbViewModel>();
            SelectedBulbVMs = new List<ColorBulbViewModel>();
            Mediator = mediator;
            UpdateBulbs(Mediator.GetBulbs());
            UpdateGroupNames(Mediator.GetGroups());
            Mediator.BulbsCollectionUpdated += (allBulbs) => Context.Post((state) => UpdateBulbs(allBulbs), new object());
            Mediator.GroupsUpdated += (groups) => Context.Post((state) => UpdateGroupNames(groups), new object());
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
        public ICommand AddToGroup
        {
            get { return new ControllerCommand(ExecuteAddToGroup, CanExecuteAddToGroup); }
        }
        void ExecuteAddToGroup(object parametr)
        {
            foreach(var bulbVM in SelectedBulbVMs)
            {
                Mediator.AddBulbToGroup(SelectedGroupName, bulbVM.Bulb);
            }
        }
        bool CanExecuteAddToGroup(object parametr)
        {
            return GroupNames.Count != 0;
        }
        public void UpdateBulbs(List<BulbDTO> bulbs)
        {
            foreach (var bulb in bulbs)
            {
                bool alreadyExists = false;
                foreach (var bulbVM in ColorBulbVMs)
                {
                    if (bulbVM.Id == bulb.Id)
                        alreadyExists = true;
                }
                if (alreadyExists == false)
                    ColorBulbVMs.Add(new ColorBulbViewModel(bulb, Mediator));
            }
            foreach(var bulbVM in ColorBulbVMs)
            {
                bool isAbsolite = true;
                foreach (var bulb in bulbs)
                {
                    if (bulb.Id == bulbVM.Id)
                        isAbsolite = false;
                }
                if (isAbsolite)
                    ColorBulbVMs.Remove(bulbVM);
            }
        }
        public void UpdateGroupNames(List<GroupDTO> groups)
        {
            GroupNames.Clear();
            foreach(var group in groups)
            {
                GroupNames.Add(group.Name);
            }
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
    }
}
