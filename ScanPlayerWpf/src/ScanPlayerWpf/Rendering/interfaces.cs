using System;
using ScanPlayerWpf.Models;

namespace ScanPlayerWpf.Rendering
{
    public interface ISceneRenderer
    {
        void Render(double surfaceWidth, double surfaceHeight, IScene scene);
    }

    public interface IScene
    {
        ISceneOptions Options { get; }
        IPrinterDefinition Printer { get; }
        IDrawingProgram Program { get; }
    }

    public interface ISceneOptions
    {
        event EventHandler EnabledHeadsChanged;

        bool ShowPlatform { get;}
        bool ShowHeadReferences { get; }
        bool ShowHeadFields { get; }
        bool ShowReference { get; }
        bool ShowJumps { get; }
        bool ShowMarks { get; }
        bool ShowPoints { get; }
        bool ShowHulls { get; }
        
        bool IsHeadEnabled(int id);
        void EnableHead(int id, bool enable);
    }

    public interface ITrackball
    {
        float Theta { get; }
        float Phi { get; }
        float Radius { get; }
        float X { get; }
        float Y { get; }
    }
}
