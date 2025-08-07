using System.Threading;
using System.Threading.Tasks;
using Avalonia.Threading;
using MessageBox.Avalonia;
using MessageBox.Avalonia.BaseWindows.Base;
using MessageBox.Avalonia.Enums;

namespace ScanPlayer.Controls;

internal static class MessageBox
{
    public static T? Show<T>(IMsBoxWindow<T> dialog)
    {
        T? result = default;
        using (var source = new CancellationTokenSource())
        {
            // Inspiration: https://github.com/AvaloniaUI/Avalonia/issues/4810#issuecomment-704259221
            _ = dialog
                .ShowDialog(App.Desktop.MainWindow)
                .ContinueWith(t =>
                {
                    result = t.Result;
                    source.Cancel();
                }, TaskScheduler.FromCurrentSynchronizationContext());

            Dispatcher.UIThread.MainLoop(source.Token);
        }

        return result;
    }
}

internal static class InformationBox
{
    public static void Show(string message) => MessageBox.Show(
        MessageBoxManager.GetMessageBoxStandardWindow("Information", message, ButtonEnum.Ok, Icon.Info));
}

internal static class QuestionBox
{
    public static bool Show(string message) => MessageBox.Show(
        MessageBoxManager.GetMessageBoxStandardWindow("Question", message, ButtonEnum.YesNo, Icon.Setting)) == ButtonResult.Yes;
}
