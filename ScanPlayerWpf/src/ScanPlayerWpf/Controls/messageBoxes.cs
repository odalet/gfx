using System.Windows;

namespace ScanPlayerWpf.Controls
{
    public static class ErrorBox
    {
        public static void Show(string text) => Show(null, text);
        public static void Show(Window owner, string text) => 
            _ = MessageBox.Show(owner, text, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
    }

    public static class WarningBox
    {
        public static void Show(string text) => Show(null, text);
        public static void Show(Window owner, string text) =>
            _ = MessageBox.Show(owner, text, "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
    }

    public static class InformationBox
    {
        public static void Show(string text) => Show(null, text);
        public static void Show(Window owner, string text) =>
            _ = MessageBox.Show(owner, text, "Error", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    public static class QuestionBox
    {
        public static MessageBoxResult Show(string text) => Show(null, text);
        public static MessageBoxResult Show(Window owner, string text) =>
            MessageBox.Show(owner, text, "Question", MessageBoxButton.YesNo, MessageBoxImage.Question);
    }
}
