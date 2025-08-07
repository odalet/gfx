using System.Windows;

namespace OpenTKTests
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow() => InitializeComponent();

        private void Grid_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var foo = 42;
        }

        private void Grid_PreviewMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var foo = 42;
        }

        ////private void Sample2Control_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        ////{
        ////    var foo = 42;
        ////}

        ////private void Sample2Control_PreviewMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        ////{
        ////    var foo = 42;
        ////}

        ////private void Window_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        ////{
        ////    var foo = 42;
        ////}

        ////private void Window_PreviewMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        ////{
        ////    var foo = 42;
        ////    e.Handled = false;
        ////}
    }
}
