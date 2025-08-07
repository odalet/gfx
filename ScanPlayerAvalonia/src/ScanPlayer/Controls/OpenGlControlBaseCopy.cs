using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Logging;
using Avalonia.Media;
using Avalonia.OpenGL;
using Avalonia.OpenGL.Imaging;

namespace ScanPlayer.Controls
{
    public abstract class OpenGlControlBaseCopy : Control
    {
        private IGlContext _context;

        private int _fb;

        private int _depthBuffer;

        private OpenGlBitmap _bitmap;

        private IOpenGlBitmapAttachment _attachment;

        private PixelSize _depthBufferSize;

        private bool _glFailed;

        private bool _initialized;

        ////public OpenGlControlBaseCopy() => EffectiveViewportChanged += (s, e) => DoCleanup();

        protected GlVersion GlVersion
        {
            get;
            private set;
        }

        public sealed override void Render(DrawingContext context)
        {
            if (!EnsureInitialized())
            {
                return;
            }

            using (_context.MakeCurrent())
            {
                _context.GlInterface.BindFramebuffer(36160, _fb);
                EnsureTextureAttachment();
                EnsureDepthBufferAttachment(_context.GlInterface);
                if (!CheckFramebufferStatus(_context.GlInterface))
                {
                    return;
                }

                OnOpenGlRender(_context.GlInterface, _fb);
                _attachment.Present();
            }

            context.DrawImage(_bitmap, new Rect(_bitmap.Size), base.Bounds);
            base.Render(context);
        }

        private void CheckError(GlInterface gl)
        {
            int value;
            while ((value = gl.GetError()) != 0)
            {
                Console.WriteLine(value);
            }
        }

        private void EnsureTextureAttachment()
        {
            _context.GlInterface.BindFramebuffer(36160, _fb);
            if (_bitmap == null || _attachment == null || _bitmap.PixelSize != GetPixelSize())
            {
                _attachment?.Dispose();
                _attachment = null;
                _bitmap?.Dispose();
                _bitmap = null;
                _bitmap = new OpenGlBitmap(GetPixelSize(), new Vector(96.0, 96.0));
                _attachment = _bitmap.CreateFramebufferAttachment(_context);
            }
        }

        private void EnsureDepthBufferAttachment(GlInterface gl)
        {
            PixelSize pixelSize = GetPixelSize();
            if (!(pixelSize == _depthBufferSize) || _depthBuffer == 0)
            {
                gl.GetIntegerv(36007, out int rv);
                if (_depthBuffer != 0)
                {
                    gl.DeleteRenderbuffers(1, new int[1]
                    {
                        _depthBuffer
                    });
                }

                int[] array = new int[1];
                gl.GenRenderbuffers(1, array);
                _depthBuffer = array[0];
                gl.BindRenderbuffer(36161, _depthBuffer);
                gl.RenderbufferStorage(36161, (GlVersion.Type == GlProfileType.OpenGLES) ? 33189 : 6402, pixelSize.Width, pixelSize.Height);
                gl.FramebufferRenderbuffer(36160, 36096, 36161, _depthBuffer);
                gl.BindRenderbuffer(36161, rv);

                ////_depthBufferSize = pixelSize; // ODT
            }
        }

        private void DoCleanup()
        {
            if (_context == null)
            {
                return;
            }

            using (_context.MakeCurrent())
            {
                GlInterface glInterface = _context.GlInterface;
                glInterface.BindTexture(3553, 0);
                glInterface.BindFramebuffer(36160, 0);
                glInterface.DeleteFramebuffers(1, new int[1]
                {
                    _fb
                });

                ////_fb = 0; // ODT

                glInterface.DeleteRenderbuffers(1, new int[1]
                {
                    _depthBuffer
                });

                _depthBuffer = 0; // ODT

                _attachment?.Dispose();
                _attachment = null;
                _bitmap?.Dispose();
                _bitmap = null;
                try
                {
                    if (_initialized)
                    {
                        _initialized = false;
                        OnOpenGlDeinit(_context.GlInterface, _fb);
                    }
                }
                finally
                {
                    _context.Dispose();
                    _context = null;
                }
            }
        }

        protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
        {
            DoCleanup();
            base.OnDetachedFromVisualTree(e);
        }

        private bool EnsureInitializedCore()
        {
            if (_context != null)
            {
                return true;
            }

            if (_glFailed)
            {
                return false;
            }

            IPlatformOpenGlInterface service = AvaloniaLocator.Current.GetService<IPlatformOpenGlInterface>();
            if (service == null)
            {
                return false;
            }

            if (!service.CanShareContexts)
            {
                Logger.TryGet(LogEventLevel.Error, "OpenGL")?.Log("OpenGlControlBase", "Unable to initialize OpenGL: current platform does not support multithreaded context sharing");
                return false;
            }

            try
            {
                _context = service.CreateSharedContext();
            }
            catch (Exception propertyValue)
            {
                Logger.TryGet(LogEventLevel.Error, "OpenGL")?.Log("OpenGlControlBase", "Unable to initialize OpenGL: unable to create additional OpenGL context: {exception}", propertyValue);
                return false;
            }

            GlVersion = _context.Version;
            try
            {
                _bitmap = new OpenGlBitmap(GetPixelSize(), new Vector(96.0, 96.0));
                if (!_bitmap.SupportsContext(_context))
                {
                    Logger.TryGet(LogEventLevel.Error, "OpenGL")?.Log("OpenGlControlBase", "Unable to initialize OpenGL: unable to create OpenGlBitmap: OpenGL context is not compatible");
                    return false;
                }
            }
            catch (Exception propertyValue2)
            {
                _context.Dispose();
                _context = null;
                Logger.TryGet(LogEventLevel.Error, "OpenGL")?.Log("OpenGlControlBase", "Unable to initialize OpenGL: unable to create OpenGlBitmap: {exception}", propertyValue2);
                return false;
            }

            using (_context.MakeCurrent())
            {
                try
                {
                    _depthBufferSize = GetPixelSize();
                    GlInterface glInterface = _context.GlInterface;
                    int[] array = new int[1];
                    glInterface.GenFramebuffers(1, array);
                    _fb = array[0];
                    glInterface.BindFramebuffer(36160, _fb);
                    EnsureDepthBufferAttachment(glInterface);
                    EnsureTextureAttachment();
                    return CheckFramebufferStatus(glInterface);
                }
                catch (Exception propertyValue3)
                {
                    Logger.TryGet(LogEventLevel.Error, "OpenGL")?.Log("OpenGlControlBase", "Unable to initialize OpenGL FBO: {exception}", propertyValue3);
                    return false;
                }
            }
        }

        private bool CheckFramebufferStatus(GlInterface gl)
        {
            if (gl.CheckFramebufferStatus(36160) != 36053)
            {
                int propertyValue;
                while ((propertyValue = gl.GetError()) != 0)
                {
                    Logger.TryGet(LogEventLevel.Error, "OpenGL")?.Log("OpenGlControlBase", "Unable to initialize OpenGL FBO: {code}", propertyValue);
                }

                return false;
            }

            return true;
        }

        private bool EnsureInitialized()
        {
            if (_initialized)
            {
                return true;
            }

            _glFailed = !(_initialized = EnsureInitializedCore());
            if (_glFailed)
            {
                return false;
            }

            using (_context.MakeCurrent())
            {
                OnOpenGlInit(_context.GlInterface, _fb);
            }

            return true;
        }

        private PixelSize GetPixelSize()
        {
            double renderScaling = base.VisualRoot!.RenderScaling;
            return new PixelSize(Math.Max(1, (int)(base.Bounds.Width * renderScaling)), Math.Max(1, (int)(base.Bounds.Height * renderScaling)));
        }

        protected virtual void OnOpenGlInit(GlInterface gl, int fb)
        {
        }

        protected virtual void OnOpenGlDeinit(GlInterface gl, int fb)
        {
        }

        protected abstract void OnOpenGlRender(GlInterface gl, int fb);
    }
}
