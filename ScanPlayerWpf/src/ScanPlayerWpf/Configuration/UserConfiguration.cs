namespace ScanPlayerWpf.Configuration
{
    internal sealed class UserConfiguration
    {
        public UserConfiguration() => MainWindowConfiguration = MainWindowConfiguration.Default;

        public MainWindowConfiguration MainWindowConfiguration { get; set; }        
    }
}
