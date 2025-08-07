using System.Collections.Generic;
using System.Linq;
using ReactiveUI;

namespace ScanPlayer.Models;

public sealed class SceneOptions : ReactiveObject
{
    private readonly Dictionary<int, bool> enabledHeadIds = new();

    public SceneOptions()
    {
        // Defaults
        drawJumps = true;
        drawMarks = true;
        drawHulls = true;
        drawPartBounds = true;
        drawPlatform = true;
        drawHeads = true;
        drawHeadFields = true;
        drawReference = true;
        drawWobbling = true;
    }

    private bool drawJumps;
    public bool DrawJumps
    {
        get => drawJumps;
        set => this.RaiseAndSetIfChanged(ref drawJumps, value);
    }

    private bool drawMarks;
    public bool DrawMarks
    {
        get => drawMarks;
        set => this.RaiseAndSetIfChanged(ref drawMarks, value);
    }

    private bool drawHulls;
    public bool DrawHulls
    {
        get => drawHulls;
        set => this.RaiseAndSetIfChanged(ref drawHulls, value);
    }

    private bool drawPartBounds;
    public bool DrawPartBounds
    {
        get => drawPartBounds;
        set => this.RaiseAndSetIfChanged(ref drawPartBounds, value);
    }

    private bool drawPlatform;
    public bool DrawPlatform
    {
        get => drawPlatform;
        set => this.RaiseAndSetIfChanged(ref drawPlatform, value);
    }

    private bool drawHeads;
    public bool DrawHeads
    {
        get => drawHeads;
        set => this.RaiseAndSetIfChanged(ref drawHeads, value);
    }

    private bool drawHeadFields;
    public bool DrawHeadFields
    {
        get => drawHeadFields;
        set => this.RaiseAndSetIfChanged(ref drawHeadFields, value);
    }

    private bool drawReference;
    public bool DrawReference
    {
        get => drawReference;
        set => this.RaiseAndSetIfChanged(ref drawReference, value);
    }

    private bool drawWobbling;
    public bool DrawWobbling
    {
        get => drawWobbling;
        set => this.RaiseAndSetIfChanged(ref drawWobbling, value);
    }

    public int[] EnabledHeadIds => enabledHeadIds.Where(kvp => kvp.Value).Select(kvp => kvp.Key).ToArray();

    public void EnableHead(int id, bool enable)
    {
        if (!enabledHeadIds.ContainsKey(id))
            enabledHeadIds.Add(id, enable);
        else
            enabledHeadIds[id] = enable;

        this.RaisePropertyChanged(nameof(EnabledHeadIds));
    }
}
