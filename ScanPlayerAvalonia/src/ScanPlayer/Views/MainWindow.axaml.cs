using System;
using System.ComponentModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using ScanPlayer.Controls;

namespace ScanPlayer.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            e.Cancel = !QuestionBox.Show("Exit?");
            base.OnClosing(e);
        }

        protected override void OnClosed(EventArgs e)
        {
            UpdateUserConfiguration();
            base.OnClosed(e);
            App.Desktop.Shutdown(0);

            //MenuItem
        }

        private void InitializeComponent() => AvaloniaXamlLoader.Load(this);

        private void UpdateUserConfiguration() { /* TODO */ }
    }
}
