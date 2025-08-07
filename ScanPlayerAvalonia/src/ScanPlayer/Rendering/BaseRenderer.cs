using System;
using System.Drawing;
using System.Numerics;
using Avalonia.Threading;
using ScanPlayer.Controls;
using ScanPlayer.OpenGL;

namespace ScanPlayer.Rendering;

internal abstract class BaseRenderer
{
    private readonly GLSurface glSurface;

    protected BaseRenderer(GLSurface owner)
    {
        glSurface = owner;
        Trackball = new Trackball();
    }

    protected bool MouseLeftButtonPressed { get; set; }
    protected bool MouseRighButtonPressed { get; set; }
    protected bool MouseMiddleButtonPressed { get; set; }
    protected virtual bool SupportsTrackballRotation => true;

    protected virtual float ScalingRatio => 1 / 300f;
    protected virtual (byte r, byte g, byte b, byte a) InitialClearColor => (0, 0, 0, 0);

    protected Trackball Trackball { get; }

    public void Initialize(GL gl)
    {
        glSurface.EffectiveViewportChanged += (s, e) => SetupViewport(gl);

        gl.ClearColor(InitialClearColor.r, InitialClearColor.g, InitialClearColor.b, InitialClearColor.a);
        SetupViewport(gl);
    }

    public void Cleanup(GL gl) =>
        glSurface.EffectiveViewportChanged -= (s, e) => SetupViewport(gl);

    public void Pan(int dx, int dy)
    {
        Trackball.X += dx / Trackball.Radius;
        Trackball.Y -= dy / Trackball.Radius;
        Invalidate();
    }

    public void Rotate(int dx, int dy)
    {
        Trackball.Theta += (float)Math.PI * ((float)dx / 1024);
        Trackball.Phi += (float)Math.PI * ((float)dy / 1024);
        Invalidate();
    }

    public void Zoom(int delta)
    {
        var scale = 1f + delta * 0.1f;
        var radius = Trackball.Radius * scale;
        radius = radius < 0.02f ? 0.02f : radius;
        radius = radius > 50f ? 50f : radius;
        Trackball.Radius = radius;
        Invalidate();
    }

    public void ResetTrackball() => Trackball.Reset();

    public void Render(GL gl)
    {
        OnBeforeRenderScene(gl);
        OnRenderScene(gl);
        OnAfterRenderScene(gl);
    }

    protected void Invalidate() => Dispatcher.UIThread.Post(() => glSurface.InvalidateVisual(), DispatcherPriority.Background);

    protected virtual void OnBeforeRenderScene(GL GL)
    {
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        GL.Enable(EnableCap.DepthTest);

        GL.PushMatrix();
        GL.MatrixMode(MatrixMode.Modelview);

        var modelView = Matrix4x4.CreateLookAt(Vector3.Zero, Vector3.UnitZ, Vector3.UnitY);
        GL.LoadMatrix(modelView);

        GL.Rotate(Trackball.Theta * 180.0 / Math.PI, 0.0f, 0.0f, 1.0f);
        GL.Rotate(Trackball.Phi * 180.0 / Math.PI, 1.0f, 0.0f, 0.0f);

        var scale = Trackball.Radius * ScalingRatio;
        GL.Scale(scale, scale, scale);

        GL.Translate(Trackball.X, Trackball.Y, 0.0);
    }

    protected abstract void OnRenderScene(GL gl);

    protected virtual void OnAfterRenderScene(GL gl) => gl.PopMatrix();

    private void SetupViewport(GL gl)
    {
        var size = glSurface.GetPixelSize();
        var length = Math.Max(size.Width, size.Height);
        var viewport = length == size.Width
            ? new Rectangle(0, (size.Height - length) / 2, length, length)
            : new Rectangle((size.Width - length) / 2, 0, length, length);
        gl.Viewport(viewport.X, viewport.Y, viewport.Width, viewport.Height);
    }
}
