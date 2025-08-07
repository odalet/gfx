using System.Windows;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Ioc;
using ScanPlayerWpf.Models;

namespace ScanPlayerWpf.ViewModels
{
    public class MainWindowModel : ViewModelBase
    {
        private readonly Workspace workspace;
        
        public MainWindowModel(Window window)
        {
            workspace = SimpleIoc.Default.GetInstance<Workspace>();
            workspace.MainWindow = window;
            OpenCommand = new RelayCommand(OpenFile);
            ExitCommand = new RelayCommand(() => Application.Current.MainWindow.Close());

            DockingManagerViewModel = new DockingManagerViewModel();
        }

        public ICommand OpenCommand { get; }
        public ICommand ExitCommand { get; }

        public string Title => "Scan Player";
        public DockingManagerViewModel DockingManagerViewModel { get; }

        public Window Owner { get; set; }
        
        public bool CanOpenFile(string filename) => workspace.ProjectLoader.CanLoadProject(filename);
        public void OpenFile() => workspace.LoadProject();
        public void OpenFile(string filename) => workspace.LoadProject(filename);
    }
}
