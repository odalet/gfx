using ReactiveUI;

namespace ScanPlayer.Models;

public sealed class HeadsInfo : ReactiveObject
{
    private readonly bool[] activeHeads;

    public HeadsInfo()
    {
        activeHeads = new bool[MaxHeadCount];
        for (var i = 0; i < MaxHeadCount; i++)
            activeHeads[i] = true;
    }

    public int MaxHeadCount { get; } = 4;
    public bool SomethingChanged => true;

    public bool HasHead(int id) => id >= 1 && id <= MaxHeadCount;
    public bool IsHeadActive(int id) => HasHead(id) && activeHeads[id - 1];
    public void SetHeadActive(int id, bool active)
    {
        if (!HasHead(id)) return;
        if (activeHeads[id - 1] == active) return;

        activeHeads[id - 1] = active;
        this.RaisePropertyChanged(nameof(SomethingChanged));
    }
}