using System;
using System.ComponentModel;
using System.Windows;
using GalaSoft.MvvmLight.Ioc;
using ScanPlayerWpf.Configuration;
using ScanPlayerWpf.Controls;
using ScanPlayerWpf.ViewModels;

namespace ScanPlayerWpf
{
    public partial class MainWindow : Window
    {
        private static readonly bool askBeforeClosing = false;

        public MainWindow()
        {
            InitializeComponent();
            if (DesignerProperties.GetIsInDesignMode(this)) return;

            ViewModel = new MainWindowModel(this);
            DataContext = ViewModel;
            ApplyUserConfiguration();

            ViewModel.OpenFile(@"D:\WORK\data\ail\plateau-old.ailb");
        }

        private MainWindowModel ViewModel { get; }

        protected override void OnClosing(CancelEventArgs e)
        {
            if (askBeforeClosing) e.Cancel = QuestionBox.Show(
                this, "Are-you sure?") != MessageBoxResult.Yes;
            base.OnClosing(e);
        }

        protected override void OnClosed(EventArgs e)
        {
            UpdateUserConfiguration();
            base.OnClosed(e);
            Application.Current.Shutdown(0);
        }

        private string VerifyDraggedItems(DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effects = DragDropEffects.None;
                e.Handled = true;
                return null;
            }

            var items = (string[])e.Data.GetData(DataFormats.FileDrop, true);
            if (items.Length == 0)
            {
                e.Effects = DragDropEffects.None;
                e.Handled = true;
                return null;
            }

            if (items.Length > 1)
            {
                e.Effects = DragDropEffects.None;
                e.Handled = true;
                return null;
            }

            if (!ViewModel.CanOpenFile(items[0]))
            {
                e.Effects = DragDropEffects.None;
                e.Handled = true;
                return null;
            }

            return items[0];
        }

        private void ProcessDropping(DragEventArgs e)
        {
            var droppedItem = VerifyDraggedItems(e);
            if (e.Effects!= DragDropEffects.None)
                ViewModel.OpenFile(droppedItem);
        }

        private void ApplyUserConfiguration()
        {
            // We must not load the layout too soon
            DockingManager.Loaded += (s, e) =>
            {
                var configurationManager = SimpleIoc.Default.GetInstance<IConfigurationManager>();
                configurationManager.LoadLayout(DockingManager);
            };

            var userConfiguration = SimpleIoc.Default.GetInstance<UserConfiguration>();
            if (userConfiguration?.MainWindowConfiguration == null)
            {
                Width = 640.0;
                Height = 480.0;
                WindowStartupLocation = WindowStartupLocation.CenterScreen;
                WindowState = WindowState.Normal;
            }
            else
            {
                var config = userConfiguration.MainWindowConfiguration;

                Left = Math.Max(config.X, SystemParameters.VirtualScreenLeft);
                Top = Math.Max(config.Y, SystemParameters.VirtualScreenTop);
                Width = config.Width;
                Height = config.Height;

                WindowStartupLocation = WindowStartupLocation.Manual;
                WindowState = config.IsMaximized ? WindowState.Maximized : WindowState.Normal;
            }
        }

        private void UpdateUserConfiguration()
        {
            var configurationManager = SimpleIoc.Default.GetInstance<IConfigurationManager>();
            configurationManager.SaveLayout(DockingManager);

            var userConfiguration = SimpleIoc.Default.GetInstance<UserConfiguration>();
            if (userConfiguration == null) return;

            userConfiguration.MainWindowConfiguration = new MainWindowConfiguration(
                Left, Top, Width, Height, WindowState == WindowState.Maximized);
        }

        private void OnDragEnter(object sender, DragEventArgs e) => VerifyDraggedItems(e);
        private void OnDragOver(object sender, DragEventArgs e) => VerifyDraggedItems(e);
        private void OnDrop(object sender, DragEventArgs e) => ProcessDropping(e);
    }
}
