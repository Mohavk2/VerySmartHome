using SmartBulbColor.PluginApp;
using SmartBulbColor.ViewModels.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Input;
using System.Windows.Media;

namespace SmartBulbColor.ViewModels
{
    internal class AllBulbsViewModel : ViewModelBase
    {
        SynchronizationContext Context = SynchronizationContext.Current;

        private AppMediator Mediator;

        public ObservableCollection<ColorBulbViewModel> ColorBulbVMs { get; set; }

        public ObservableCollection<GroupCaption> GroupCaptions { get; set; } = new ObservableCollection<GroupCaption>();

        GroupCaption _selectedGroup;
        public GroupCaption SelectedGroup
        {
            get { return _selectedGroup; }
            set
            {
                _selectedGroup = value;
                OnPropertyChanged("SelectedGroup");
            }
        }

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

            Mediator.GroupCreated += (group) => Context.Post((state) => OnGroupCreated(group), new object());
            Mediator.GroupUpdated += (group) => Context.Post((state) => OnGroupUpdated(group), new object());
            Mediator.GroupDeleted += (group) => Context.Post((state) => OnGroupDeleted(group), new object());

            Mediator.BulbCreated += (bulb) => Context.Post((state) => OnBulbCreated(bulb), new object());
            Mediator.BulbDeleted += (bulb) => Context.Post((state) => OnBulbDeleted(bulb), new object());
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
                Mediator.AddBulbToGroup(SelectedGroup.Id, bulbVM.Id);
            }
        }

        bool CanExecuteAddToGroup(object parametr)
        {
            return GroupCaptions.Count != 0;
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

        void OnBulbCreated(BulbDTO bulb)
        {
            ColorBulbVMs.Add(new ColorBulbViewModel(bulb, Mediator));
        }

        private void OnBulbDeleted(BulbDTO bulb)
        {
            ColorBulbViewModel bulbVmToRemove = null;
            foreach (var bulbVM in ColorBulbVMs)
            {
                if (bulbVM.Id == bulb.Id)
                    bulbVmToRemove = bulbVM;
            }
            if (bulbVmToRemove != null)
                ColorBulbVMs.Remove(bulbVmToRemove);
        }

        private void OnGroupUpdated(GroupDTO group)
        {
            foreach (var caption in GroupCaptions)
            {
                if (caption.Id == group.Id && caption.Name != group.Name)
                    caption.Name = group.Name;
            }
        }

        private void OnGroupCreated(GroupDTO group)
        {
            var isContains = false;
            foreach (var caption in GroupCaptions)
            {
                if (caption.Id == group.Id)
                    isContains = true;
            }
            if (isContains == false)
                GroupCaptions.Add(new GroupCaption { Id = group.Id, Name = group.Name });
        }

        private void OnGroupDeleted(GroupDTO group)
        {
            GroupCaption captionToRemove = null;
            foreach (var caption in GroupCaptions)
            {
                if (caption.Id == group.Id)
                    captionToRemove = caption;
            }
            if (captionToRemove != null)
                GroupCaptions.Remove(captionToRemove);
        }
    }
}
