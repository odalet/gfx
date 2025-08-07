using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Logging;
using Avalonia.Media;
using Avalonia.OpenGL;
using Avalonia.OpenGL.Imaging;
using ScanPlayer.OpenGL;

namespace ScanPlayer.Controls;

// Heavily inspired by Avalonia.OpenGL.Controls.OpenGlControlBase
// But mainly based on (and exposing) ScanPlayer.OpenGL.GL.
public abstract class GLSurface : Control
{
    private bool isInitialized;
    private IGlContext? glContext;
    private GL? glInstance;
    private OpenGlBitmap? openglBitmap;
    private IOpenGlBitmapAttachment? attachment;
    private PixelSize depthBufferSize;
    private uint fb;
    private uint depthBuffer;

    public GlVersion GlVersion { get; private set; }
    public IGlContext Context => glContext ?? throw new InvalidOperationException("OpenGL Context is null");
    public GL GL => glInstance ?? throw new InvalidOperationException("OpenGL API is null");

    // This is needed so that resizing the control redraws correctly the scene
    // However, it is weird, because OpenGLControlBase does not seem to need this...
    public GLSurface() => EffectiveViewportChanged += (s, e) => Cleanup();

    public PixelSize GetPixelSize()
    {
        var scaling = VisualRoot!.RenderScaling;
        return new PixelSize(
            Math.Max(1, (int)(Bounds.Width * scaling)),
            Math.Max(1, (int)(Bounds.Height * scaling)));
    }

    public sealed override void Render(DrawingContext context)
    {
        if (!EnsureInitialized())
            return;

        using (glContext!.MakeCurrent())
        {
            var gl = glInstance!;
            gl.BindFramebuffer(FramebufferTarget.Framebuffer, fb);
            EnsureTextureAttachment(glContext, gl);
            EnsureDepthBufferAttachment(gl);
            if (!CheckFramebufferStatus(gl))
                return;

            OnRender(gl, fb);
            attachment!.Present();
        }

        context.DrawImage(openglBitmap, new Rect(openglBitmap!.Size), Bounds);
        //base.Render(context);
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        Cleanup();
        base.OnDetachedFromVisualTree(e);
    }

    protected virtual void OnInitializeRendering(GL gl, uint fb) { }
    protected virtual void OnRender(GL gl, uint fb) { }
    protected virtual void OnCleanupRendering(GL gl, uint fb) { }

    protected void CheckGLError([CallerMemberName] string? caller = null, [CallerFilePath] string? sourceFile = null, [CallerLineNumber] int line = 0) =>
        CheckGLErrorImpl(caller, sourceFile, line);

    private bool EnsureInitialized()
    {
        if (isInitialized) return true;

        glContext = InitializeOpenGLContext();
        if (glContext == null)
        {
            isInitialized = false;
            return false;
        }

        using (glContext.MakeCurrent())
            OnInitializeRendering(glInstance!, fb);

        isInitialized = true;
        return true;
    }

    private IGlContext? InitializeOpenGLContext()
    {
        if (glContext != null) return glContext;

        var feature = AvaloniaLocator.Current.GetService<IPlatformOpenGlInterface>();
        if (feature == null) return null;

        if (!feature.CanShareContexts)
        {
            LogGLError("Unable to initialize OpenGL. Current platform does not support multithreaded context sharing");
            return null;
        }

        IGlContext? context;
        try
        {
            context = feature.CreateSharedContext();

            // Create the gl API instance
            glInstance = new GL(context.GlInterface.GetProcAddress);
        }
        catch (Exception ex)
        {
            LogGLError($"Unable to initialize OpenGL. Cannot create additional OpenGL context: {ex.Message}");
            return null;
        }

        GlVersion = context.Version;
        var currentPixelSize = GetPixelSize();
        try
        {
            openglBitmap = new OpenGlBitmap(currentPixelSize, new Vector(96, 96));
            if (!openglBitmap.SupportsContext(context))
            {
                LogGLError($"Unable to initialize OpenGL. Cannot create an OpenGlBitmap: OpenGL context is not compatible");
                return null;
            }
        }
        catch (Exception ex)
        {
            context.Dispose();
            context = null;
            LogGLError($"Unable to initialize OpenGL. Cannot create an OpenGlBitmap: {ex.Message}");
            return null;
        }

        using (context.MakeCurrent())
        {
            try
            {
                fb = glInstance.GenFramebuffer();
                glInstance.BindFramebuffer(FramebufferTarget.Framebuffer, fb);

                depthBufferSize = currentPixelSize;

                EnsureDepthBufferAttachment(glInstance, firstCall: true);
                CheckGLError();

                EnsureTextureAttachment(context, glInstance);
                CheckGLError();

                var ok = CheckFramebufferStatus(glInstance);
                CheckGLError();

                if (!ok)
                {
                    context.Dispose();
                    context = null;
                }
            }
            catch (Exception ex)
            {
                context?.Dispose();
                context = null;
                LogGLError($"Unable to initialize OpenGL. Cannot create an FBO: {ex.Message}");
            }
        }

        return context;
    }

    private void EnsureDepthBufferAttachment(GL gl, bool firstCall = false)
    {
        var currentPixelSize = GetPixelSize();
        if (currentPixelSize == depthBufferSize && depthBuffer != 0)
            return;

        var previousDepthBuffer = (uint)gl.GetInteger(GLEnum.RenderbufferBinding);
        if (firstCall)
        {
            // The first time, it is not really an error to have InvalidEnum:
            // This is because we don't yet have a "previous" depth buffer.
            var error = gl.GetError();
            if (error is not ErrorCode.InvalidEnum and not ErrorCode.NoError)
                LogGLError(BuildGLErrorLogMessage(error));
        }
        else CheckGLError();

        if (depthBuffer != 0u) gl.DeleteRenderbuffer(depthBuffer);

        depthBuffer = gl.GenRenderbuffer();
        gl.BindRenderbuffer(RenderbufferTarget.Renderbuffer, depthBuffer);
        var format = GlVersion.Type == GlProfileType.OpenGLES ? InternalFormat.DepthComponent16 : InternalFormat.DepthComponent;
        gl.RenderbufferStorage(RenderbufferTarget.Renderbuffer, format, currentPixelSize.Width, currentPixelSize.Height);
        gl.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, RenderbufferTarget.Renderbuffer, depthBuffer);
        gl.BindRenderbuffer(RenderbufferTarget.Renderbuffer, previousDepthBuffer);

        depthBufferSize = currentPixelSize; // ODT
    }

    private void EnsureTextureAttachment(IGlContext context, GL gl)
    {
        gl.BindFramebuffer(FramebufferTarget.Framebuffer, fb);
        var currentPixelSize = GetPixelSize();
        if (attachment == null || openglBitmap == null || openglBitmap.PixelSize != currentPixelSize)
        {
            attachment?.Dispose();
            attachment = null;
            openglBitmap?.Dispose();
            openglBitmap = null;

            openglBitmap = new OpenGlBitmap(currentPixelSize, new Vector(96, 96));
            attachment = openglBitmap.CreateFramebufferAttachment(context);
        }
    }

    private bool CheckFramebufferStatus(GL gl)
    {
        var status = gl.CheckFramebufferStatus(FramebufferTarget.Framebuffer);
        if (status == FramebufferStatus.FramebufferComplete)
            return true;

        var codes = new List<ErrorCode>();
        ErrorCode code;
        while ((code = gl.GetError()) != ErrorCode.NoError)
            codes.Add(code);

        LogGLError($"Unable to initialize OpenGL FBO: status is {status}; other error codes: {string.Join(", ", codes.Select(c => c.ToString()))}");
        return false;
    }

    private void Cleanup()
    {
        if (glContext == null) return;

        try
        {
            using (glContext.MakeCurrent())
            {
                var gl = glInstance!;

                gl.BindTexture(TextureTarget.Texture2d, 0u);
                gl.BindFramebuffer(FramebufferTarget.Framebuffer, 0u);
                gl.DeleteFramebuffer(fb);
                fb = 0u;

                gl.DeleteRenderbuffer(depthBuffer);
                depthBuffer = 0u;

                attachment?.Dispose();
                attachment = null;
                openglBitmap?.Dispose();
                openglBitmap = null;

                if (isInitialized)
                {
                    isInitialized = false;
                    OnCleanupRendering(glInstance!, fb);
                }
            }
        }
        finally
        {
            glContext.Dispose();
            glContext = null;
        }
    }

    [Conditional("DEBUG")]
    private void CheckGLErrorImpl(string? caller, string? sourceFile, int line)
    {
        if (glInstance == null) return;

        ErrorCode error;
        while ((error = glInstance.GetError()) != ErrorCode.NoError)
        {
            var message = BuildGLErrorLogMessage(error, caller, sourceFile, line);
            LogGLError(message);
        }
    }

    private static string BuildGLErrorLogMessage(
        ErrorCode error,
        [CallerMemberName] string? caller = null, 
        [CallerFilePath] string? sourceFile = null,
        [CallerLineNumber] int line = 0)
    {
        var location = caller ?? "?";
        if (sourceFile != null)
        {
            var filename = Path.GetFileName(sourceFile);
            location += $" ({filename}@{line})";
        }

        return $"OpenGL Error in {location}: {error} ({(uint)error})";
    }

    protected void LogGLError(string message) => Log(LogEventLevel.Error, "OpenGL", message);
    protected void LogGLWarning(string message) => Log(LogEventLevel.Warning, "OpenGL", message);
    protected void LogGLInfo(string message) => Log(LogEventLevel.Information, "OpenGL", message);

    private void Log(LogEventLevel level, string area, string message) =>
        Logger.TryGet(level, area)?.Log(this, message);
}
