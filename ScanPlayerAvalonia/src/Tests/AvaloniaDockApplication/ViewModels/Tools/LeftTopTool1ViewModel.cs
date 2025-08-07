using Dock.Model.ReactiveUI.Controls;
using MessageBox.Avalonia.Enums;
using MessageBox.Avalonia;
using ReactiveUI;
using System.Windows.Input;

namespace AvaloniaDockApplication.ViewModels.Tools
{
    public class LeftTopTool1ViewModel : Tool
    {
        public LeftTopTool1ViewModel()
        {
            OpenCommand = ReactiveCommand.Create(Open);
        }

        public ICommand OpenCommand { get; }

        public string WhoAmI => "Top Left Tool 1";

        private void Open()
        {
            InformationBox.Show("Open");
        }
    }

    internal static class InformationBox
    {
        public static void Show(string message) =>
            MessageBoxManager.GetMessageBoxStandardWindow("Information", message, ButtonEnum.Ok, Icon.Info).Show();
    }
}
