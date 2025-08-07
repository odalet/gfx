using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using System.Windows.Media;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Ioc;

namespace ScanPlayerWpf.ViewModels
{
    // See https://github.com/8/AvalonDockMVVM/blob/master/AvalonDockMVVM/ViewModel/MainViewModel.cs
    public class DockingManagerViewModel : ViewModelBase
    {
        public DockingManagerViewModel()
        {
            Documents = new ObservableCollection<DockWindowViewModel>();
            Anchorables = new ObservableCollection<DockWindowViewModel>();

            // Add the only Document
            Documents.Add(SimpleIoc.Default.GetInstance<SceneViewModel>());

            // Add Anchorables
            Anchorables.Add(SimpleIoc.Default.GetInstance<SceneOptionsViewModel>());
            Anchorables.Add(SimpleIoc.Default.GetInstance<HeadsViewModel>());
            Anchorables.Add(SimpleIoc.Default.GetInstance<TimeViewModel>());
            Anchorables.Add(SimpleIoc.Default.GetInstance<LogViewModel>());

            ViewMenuItemViewModel = new ViewMenuItemViewModel(this);
        }

        public ObservableCollection<DockWindowViewModel> Documents { get; }
        public ObservableCollection<DockWindowViewModel> Anchorables { get; }
        public ViewMenuItemViewModel ViewMenuItemViewModel { get; }
    }

    public abstract class DockWindowViewModel : ViewModelBase
    {
        protected DockWindowViewModel(string contentId)
        {
            ContentId = contentId ?? "Window";
            IsVisible = true;
        }

        public ICommand CloseCommand { get; }
        public string ContentId { get; }

        private bool isVisible;
        public bool IsVisible
        {
            get => isVisible;
            set => Set(ref isVisible, value);
        }
               
        private bool isSelected;
        public bool IsSelected
        {
            get => isSelected;
            set => Set(ref isSelected, value);
        }

        private bool isActive;
        public bool IsActive
        {
            get => isActive;
            set => Set(ref isActive, value);
        }

        private string title;
        public string Title
        {
            get => title;
            set => Set(ref title, value);
        }

        private ImageSource iconSource;
        public ImageSource IconSource
        {
            get => iconSource;
            set => Set(ref iconSource, value);
        }
    }

    public class ViewMenuItemViewModel : ViewModelBase
    {
        public ViewMenuItemViewModel(DockingManagerViewModel dockingManagerViewModel) =>
            Items = new List<DockWindowViewModelMenuItemViewModel>(
                dockingManagerViewModel.Anchorables.Select(a => new DockWindowViewModelMenuItemViewModel(a)));

        public bool IsCheckable => false;
        public IEnumerable<DockWindowViewModelMenuItemViewModel> Items { get; }
    }

    public class DockWindowViewModelMenuItemViewModel : ViewModelBase
    {
        private readonly DockWindowViewModel parent;

        public DockWindowViewModelMenuItemViewModel(DockWindowViewModel dockWindowViewModel)
        {
            parent = dockWindowViewModel;
            parent.PropertyChanged += (s, e) => RaisePropertyChanged(nameof(IsChecked));
            Header = parent.Title;
            Command = new RelayCommand(() => parent.IsVisible = !parent.IsVisible);
        }

        public ICommand Command { get; }
        public string Header { get; }
        public bool IsCheckable => true;
        public bool IsChecked => parent.IsVisible;
    }
}
