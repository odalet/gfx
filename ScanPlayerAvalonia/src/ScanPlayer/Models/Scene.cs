using System.Collections.Generic;
using AddUp.NCore.Printing;
using AddUp.NCore.Printing.ProjectModel;
using AddUp.NCore.Scan;
using AddUp.ThreeMF.AddprojExtension.Model.Addproj;
using ReactiveUI;

namespace ScanPlayer.Models;

internal sealed class Scene : ReactiveObject
{
    public Scene() => speed = 4000; // default

    private string? projectFileName;
    public string? ProjectFileName
    {
        get => projectFileName;
        private set => this.RaiseAndSetIfChanged(ref projectFileName, value);
    }

    private AddprojPackage? addprojPackage;
    public AddprojPackage? AddprojPackage
    {
        get => addprojPackage;
        private set => this.RaiseAndSetIfChanged(ref addprojPackage, value);
    }

    private IProject? project;
    public IProject? Project
    {
        get => project;
        private set => this.RaiseAndSetIfChanged(ref project, value);
    }

    private IPrinterCharacteristics? printerCharacteristics;
    public IPrinterCharacteristics? PrinterCharacteristics
    {
        get => printerCharacteristics;
        private set => this.RaiseAndSetIfChanged(ref printerCharacteristics, value);
    }

    // Current Time in ticks
    private long currentTime;
    public long CurrentTime
    {
        get => currentTime;
        set => this.RaiseAndSetIfChanged(ref currentTime, value);
    }

    private int speed;
    public int Speed
    {
        get => speed;
        set => this.RaiseAndSetIfChanged(ref speed, value);
    }

    private IReadOnlyDictionary<int, ScanJob>? scanjobsPerHead;
    public IReadOnlyDictionary<int, ScanJob>? ScanjobsPerHead
    {
        get => scanjobsPerHead;
        set => this.RaiseAndSetIfChanged(ref scanjobsPerHead, value);
    }

    private IPrinterDefinition? printerDefinition;
    public IPrinterDefinition? PrinterDefinition
    {
        get => printerDefinition;
        set
        {
            if (printerDefinition == value) return;
            _ = this.RaiseAndSetIfChanged(ref printerDefinition, value);
            OnPrinterDriverChanged();
        }
    }

    public void Dispose() => addprojPackage?.Dispose();

    public void OpenAddprojProject(IProject project, AddprojPackage package, string filename)
    {
        Project = project;
        AddprojPackage = package;
        ProjectFileName = filename;
    }

    public void OpenProject(IProject project, string filename)
    {
        Project = project;
        ProjectFileName = filename;
    }

    public void CloseProject()
    {
        ProjectFileName = null;

        AddprojPackage?.Dispose();
        AddprojPackage = null;

        Project?.Dispose();
        Project = null;
    }

    private void OnPrinterDriverChanged() =>
        PrinterCharacteristics = printerDefinition == null ? null : new PrinterCharacteristics(printerDefinition);
}

