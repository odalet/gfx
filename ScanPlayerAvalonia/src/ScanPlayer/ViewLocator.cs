using System;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using ScanPlayer.ViewModels;

namespace ScanPlayer
{
    public sealed class ViewLocator : IDataTemplate
    {
        public IControl Build(object data)
        {
            // That's convention based!
            var name = data.GetType().FullName!.Replace("ViewModel", "View");
            var type = Type.GetType(name);

            return type == null 
                ? new TextBlock { Text = "Not Found: " + name }
                : (Control)Activator.CreateInstance(type)!;
        }

        public bool Match(object data) => data is ViewModelBase;
    }
}
