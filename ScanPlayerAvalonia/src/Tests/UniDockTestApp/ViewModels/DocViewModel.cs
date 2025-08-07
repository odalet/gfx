using ReactiveUI;

namespace UniDockTestApp.ViewModels;

public class DocViewModel : ViewModelBase
{
    private int docNumber;
    public int DocNumber
    {
        get => docNumber;
        set => this.RaiseAndSetIfChanged(ref docNumber, value);
    }
}
