using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Platform;
using PixelFormat = OpenTK.Graphics.OpenGL.PixelFormat;

namespace OpenTKTests.Rendering
{
    // Adapted from https://github.com/jayhf/OpenTkControl
    public partial class OpenTKControl : UserControl
    {
        private readonly ConcurrentQueue<Tuple<TaskCompletionSource<uint[,]>, int, int>> _screenshotQueue = new ConcurrentQueue<Tuple<TaskCompletionSource<uint[,]>, int, int>>();
        private readonly ConcurrentQueue<TaskCompletionSource<object>> _repaintRequestQueue = new ConcurrentQueue<TaskCompletionSource<object>>();
        private readonly ManualResetEvent ManualRepaintEvent = new ManualResetEvent(false);
        private readonly ManualResetEvent _becameVisibleEvent = new ManualResetEvent(false);

        private IGraphicsContext _context;
        private volatile WriteableBitmap _bitmap;
        private int _bitmapWidth;
        private int _bitmapHeight;
        private IntPtr _backBuffer = IntPtr.Zero;
        private IWindowInfo _windowInfo;
        private Task _previousUpdateImageTask;

        private bool _newContext;
        private DateTime _lastFrameTime = DateTime.MinValue;
        private int _frameBuffer;
        private int _renderBuffer;
        private int _depthBuffer;
        private bool _alreadyLoaded;

        private Thread _renderThread;
        private CancellationTokenSource _endThreadCts;

        public OpenTKControl()
        {
            InitializeComponent();

            if (DesignerProperties.GetIsInDesignMode(this)) return;

            InitializeDependencyProperties();
            WireEvents();
        }

        public event EventHandler<GlRenderEventArgs> GLRender;
        public event EventHandler<UnhandledExceptionEventArgs> ExceptionOccurred;

        #region Dependency Properties

        private volatile string volatileOpenGLVersion = (string)OpenGLVersionProperty.DefaultMetadata.DefaultValue;

        public static readonly DependencyProperty OpenGLVersionProperty = DependencyProperty.Register(
            nameof(OpenGLVersion), typeof(string), typeof(OpenTKControl), new PropertyMetadata("3.0"));

        /// <summary>
        /// Gets or sets the OpenGL Version to use. Should be formatted as X.X
        /// </summary>
        public string OpenGLVersion
        {
            get => (string)GetValue(OpenGLVersionProperty);
            set => SetValue(OpenGLVersionProperty, value);
        }

        private volatile float volatileFrameRateLimit = (float)FrameRateLimitProperty.DefaultMetadata.DefaultValue;

        public static readonly DependencyProperty FrameRateLimitProperty = DependencyProperty.Register(
            nameof(FrameRateLimit), typeof(float), typeof(OpenTKControl), new PropertyMetadata(float.PositiveInfinity));

        /// <summary>
        /// Gets or sets the maximum frame rate to render at. Anything over 1000 is treated as unlimited.
        /// </summary>
        public float FrameRateLimit
        {
            get => (float)GetValue(FrameRateLimitProperty);
            set => SetValue(FrameRateLimitProperty, value);
        }

        private volatile float volatilePixelScale = (float)PixelScaleProperty.DefaultMetadata.DefaultValue;
        public static readonly DependencyProperty PixelScaleProperty = DependencyProperty.Register(
            nameof(PixelScale), typeof(float), typeof(OpenTKControl), new PropertyMetadata(1f));

        /// <summary>
        /// Scales the pixel size to change the number of pixels rendered. Mainly useful for improving performance.
        /// A scale greater than 1 means that pixels will be bigger and the resolution will decrease.
        /// </summary>
        public float PixelScale
        {
            get => (float)GetValue(PixelScaleProperty);
            set => SetValue(PixelScaleProperty, value);
        }

        private volatile uint volatileMaxPixels = (uint)MaxPixelsProperty.DefaultMetadata.DefaultValue;
        public static readonly DependencyProperty MaxPixelsProperty = DependencyProperty.Register(
            nameof(MaxPixels), typeof(uint), typeof(OpenTKControl), new PropertyMetadata(uint.MaxValue));

        /// <summary>
        /// Gets or sets the maximum number of pixels to draw. If the control size is larger than this, the scale will
        /// be changed as necessary to stay under this limit.
        /// </summary>
        public uint MaxPixels
        {
            get => (uint)GetValue(MaxPixelsProperty);
            set => SetValue(MaxPixelsProperty, value);
        }

        private volatile bool volatileContinuous = (bool)ContinuousProperty.DefaultMetadata.DefaultValue;
        public static readonly DependencyProperty ContinuousProperty = DependencyProperty.Register(
                nameof(Continuous), typeof(bool), typeof(OpenTKControl), new PropertyMetadata(true));

        /// <summary>
        /// Gets or sets a value indicating whether this control is in continuous mode. 
        /// If set to <c>false</c>, <see cref="RequestRepaintAsync"/> must be called to get the control to render. 
        /// Otherwise, it will automatically Render as fast as possible, up to the <see cref="FrameRateLimit"/>.
        /// </summary>
        public bool Continuous
        {
            get => (bool)GetValue(ContinuousProperty);
            set => SetValue(ContinuousProperty, value);
        }

        public static readonly DependencyProperty ThreadNameProperty = DependencyProperty.Register(
            nameof(ThreadName), typeof(string), typeof(OpenTKControl), new PropertyMetadata("OpenTK Render Thread"));

        /// <summary>
        /// Gets or sets the name of the background thread that does the OpenGL rendering
        /// </summary>
        public string ThreadName
        {
            get => (string)GetValue(ThreadNameProperty);
            set => SetValue(ThreadNameProperty, value);
        }

        #endregion

        public Task RequestRepaintAsync()
        {
            var tcs = new TaskCompletionSource<object>();
            _repaintRequestQueue.Enqueue(tcs);
            ManualRepaintEvent.Set();
            return tcs.Task;
        }

        /// <summary>
        /// Renders a screenshot of the frame with the specified dimensions. It will be in bgra format with
        /// [0,0] at the bottom left corner. Note that this is not meant for taking screenshots of what is
        /// displayed on the screen each frame. To do that, just use GL.ReadPixels.
        /// </summary>
        /// <param name="width">The width of the screenshot in pixels or 0 to use the current width</param>
        /// <param name="height">The height of the screenshot in pixels or 0 to use the current height</param>
        /// <returns>A task that completes when the screenshot is ready</returns>
        public Task<uint[,]> GrabScreenshotAsync(int width = 0, int height = 0)
        {
            var tcs = new TaskCompletionSource<uint[,]>();
            _screenshotQueue.Enqueue(new Tuple<TaskCompletionSource<uint[,]>, int, int>(tcs, width, height));
            return tcs.Task;
        }

        protected virtual void OnLoaded(object sender, RoutedEventArgs args)
        {
            _windowInfo = Utilities.CreateWindowsWindowInfo(new WindowInteropHelper(Window.GetWindow(this)).Handle);

            _endThreadCts = new CancellationTokenSource();
            _renderThread = new Thread(RenderThread)
            {
                IsBackground = true,
                Priority = ThreadPriority.Highest,
                Name = ThreadName
            };
            _renderThread.Start(_endThreadCts.Token);
        }

        protected virtual async void OnUnloaded(object sender, RoutedEventArgs args)
        {
            try
            {
                var task = _previousUpdateImageTask;
                if (task != null)
                    await task;
            }
            catch (TaskCanceledException) { /* Nothing to do here */ }
            catch (Exception ex)
            {
                ExceptionOccurred?.Invoke(this, new UnhandledExceptionEventArgs(ex, false));
            }

            _previousUpdateImageTask = null;
            _windowInfo = null;
            _backBuffer = IntPtr.Zero;
            _bitmap = null;
            _lastFrameTime = DateTime.MinValue;

            _endThreadCts.Cancel();
            _renderThread.Join();
        }

        #region Initializations

        private void InitializeOpenGL()
        {
            try
            {
                var version = Version.Parse(volatileOpenGLVersion);
                var mode = new GraphicsMode(DisplayDevice.Default.BitsPerPixel, 16, 0, 4, 0, 2, false);
                _context = new GraphicsContext(mode, _windowInfo, version.Major, version.Minor, GraphicsContextFlags.Default);
                _newContext = true;
                _context.LoadAll();
                _context.MakeCurrent(_windowInfo);
            }
            catch (Exception ex)
            {
                ExceptionOccurred?.Invoke(this, new UnhandledExceptionEventArgs(ex, false));
            }
        }

        private void UninitializeOpenGL()
        {
            try
            {
                UninitializeOpenGLBuffers();

                _context.Dispose();
                _context = null;

                while (_screenshotQueue.TryDequeue(out var tuple))
                {
                    tuple.Item1.SetCanceled();
                }
            }
            catch (Exception ex)
            {
                ExceptionOccurred?.Invoke(this, new UnhandledExceptionEventArgs(ex, false));
            }
        }

        private void InitializeDependencyProperties()
        {
            // Update all of the volatile copies the variables
            // This is a workaround for the WPF threading restrictions on DependencyProperties
            // that allows other threads to read the values.
            DependencyPropertyDescriptor.FromProperty(OpenGLVersionProperty, typeof(OpenTKControl)).AddValueChanged(this,
                (s, e) => volatileOpenGLVersion = OpenGLVersion);

            DependencyPropertyDescriptor.FromProperty(FrameRateLimitProperty, typeof(OpenTKControl)).AddValueChanged(this,
                (s, e) => volatileFrameRateLimit = FrameRateLimit);

            DependencyPropertyDescriptor.FromProperty(PixelScaleProperty, typeof(OpenTKControl)).AddValueChanged(this,
                (s, e) => volatilePixelScale = PixelScale);

            DependencyPropertyDescriptor.FromProperty(MaxPixelsProperty, typeof(OpenTKControl)).AddValueChanged(this,
                (s, e) => volatileMaxPixels = MaxPixels);

            DependencyPropertyDescriptor.FromProperty(ContinuousProperty, typeof(OpenTKControl)).AddValueChanged(this, (s, e) =>
            {
                volatileContinuous = Continuous;
                // Handle the case where we switched to continuous, but the thread is still waiting for a request
                if (volatileContinuous)
                    _ = RequestRepaintAsync();
            });
        }

        private void WireEvents()
        {
            Loaded += (sender, args) =>
            {
                if (_alreadyLoaded)
                    return;

                _alreadyLoaded = true;
                OnLoaded(sender, args);
            };

            Unloaded += (sender, args) =>
            {
                if (!_alreadyLoaded)
                    return;

                _alreadyLoaded = false;
                OnUnloaded(sender, args);
            };

            IsVisibleChanged += (sender, args) =>
            {
                var visible = (bool)args.NewValue;
                if (visible)
                    _ = _becameVisibleEvent.Set();
            };
        }

        #endregion

        #region Rendering

        private void RenderThread(object boxedToken)
        {
            var token = (CancellationToken)boxedToken;

            InitializeOpenGL();

            WaitHandle[] notContinousHandles = { token.WaitHandle, ManualRepaintEvent };
            WaitHandle[] notVisibleHandles = { token.WaitHandle, _becameVisibleEvent };
            while (!token.IsCancellationRequested)
            {
                if (!volatileContinuous)
                    _ = WaitHandle.WaitAny(notContinousHandles);
                else if (!IsVisible)
                {
                    _ = WaitHandle.WaitAny(notVisibleHandles);
                    _ = _becameVisibleEvent.Reset();

                    if (!volatileContinuous)
                        continue;
                }

                if (token.IsCancellationRequested)
                    break;

                _ = ManualRepaintEvent.Reset();

                var sleepTime = Render();
                if (sleepTime.CompareTo(TimeSpan.Zero) > 0)
                    Thread.Sleep(sleepTime);
            }

            UninitializeOpenGL();
        }

        /// <summary>
        /// Handles generating screenshots and updating the display image
        /// </summary>
        protected TimeSpan Render()
        {
            try
            {
                RenderScreenshots(out var currentBufferWidth, out var currentBufferHeight);
                CalculateBufferSize(out var width, out var height);

                if (volatileContinuous && !IsVisible || width == 0 || height == 0)
                    return TimeSpan.FromMilliseconds(20);

                if (volatileContinuous && volatileFrameRateLimit > 0 && volatileFrameRateLimit < 1000)
                {
                    var now = DateTime.Now;
                    var delayTime = TimeSpan.FromSeconds(1 / volatileFrameRateLimit) - (now - _lastFrameTime);
                    if (delayTime.CompareTo(TimeSpan.Zero) > 0)
                        return delayTime;

                    _lastFrameTime = now;
                }
                else _lastFrameTime = DateTime.MinValue;

                if (!ReferenceEquals(GraphicsContext.CurrentContext, _context))
                    _context.MakeCurrent(_windowInfo);

                var resized = false;
                Task resizeBitmapTask = null;
                //Need Abs(...) > 1 to handle an edge case where resizing the bitmap causes the height to increase in an infinite loop
                if (_bitmap == null || Math.Abs(_bitmapWidth - width) > 1 || Math.Abs(_bitmapHeight - height) > 1)
                {
                    resized = true;
                    _bitmapWidth = width;
                    _bitmapHeight = height;
                    resizeBitmapTask = RunOnUIThread(() =>
                    {
                        _bitmap = new WriteableBitmap(width, height, 96, 96, PixelFormats.Pbgra32, null);
                        _backBuffer = _bitmap.BackBuffer;
                    });
                }

                if (currentBufferWidth != _bitmapWidth || currentBufferHeight != _bitmapHeight)
                    CreateOpenGLBuffers(_bitmapWidth, _bitmapHeight);

                List<TaskCompletionSource<object>> repaintRequests = null;
                while (_repaintRequestQueue.TryDequeue(out var tcs))
                {
                    if (repaintRequests == null)
                        repaintRequests = new List<TaskCompletionSource<object>>();
                    repaintRequests.Add(tcs);
                }

                var args = new GlRenderEventArgs(_bitmapWidth, _bitmapHeight, resized, false, CheckNewContext());
                try
                {
                    OnGLRender(args);
                }
                finally
                {
                    if (repaintRequests != null)
                    {
                        foreach (var taskCompletionSource in repaintRequests)
                            taskCompletionSource.SetResult(null);
                    }
                }

                var dirtyArea = args.RepaintRect;
                if (dirtyArea.Width <= 0 || dirtyArea.Height <= 0)
                    return TimeSpan.Zero;

                try
                {
                    resizeBitmapTask?.Wait();
                    try
                    {
                        _previousUpdateImageTask?.Wait();
                    }
                    finally
                    {
                        _previousUpdateImageTask = null;
                    }
                }
                catch (TaskCanceledException)
                {
                    return TimeSpan.Zero;
                }

                if (_backBuffer != IntPtr.Zero)
                    GL.ReadPixels(0, 0, _bitmapWidth, _bitmapHeight, PixelFormat.Bgra, PixelType.UnsignedByte, _backBuffer);

                _previousUpdateImageTask = RunOnUIThread(() => UpdateImage(dirtyArea));
            }
            catch (Exception e)
            {
                ExceptionOccurred?.Invoke(this, new UnhandledExceptionEventArgs(e, false));
            }

            return TimeSpan.Zero;
        }

        private void RenderScreenshots(out int currentWidth, out int currentHeight)
        {
            currentWidth = _bitmapWidth;
            currentHeight = _bitmapHeight;

            while (_screenshotQueue.TryDequeue(out var screenshotInfo))
            {
                var tcs = screenshotInfo.Item1;
                var screenshotWidth = screenshotInfo.Item2;
                var screenshotHeight = screenshotInfo.Item3;
                if (screenshotWidth <= 0) screenshotWidth = _bitmapWidth;
                if (screenshotHeight <= 0) screenshotHeight = _bitmapHeight;

                try
                {
                    var screenshot = new uint[screenshotHeight, screenshotWidth];

                    //Handle the case where the window has 0 width or height
                    if (screenshotHeight == 0 || screenshotWidth == 0)
                    {
                        tcs.SetResult(screenshot);
                        continue;
                    }

                    if (screenshotWidth != currentWidth || screenshotHeight != currentHeight)
                    {
                        currentWidth = screenshotWidth;
                        currentHeight = screenshotHeight;
                        CreateOpenGLBuffers(screenshotWidth, screenshotHeight);
                    }

                    OnGLRender(new GlRenderEventArgs(screenshotWidth, screenshotHeight, false, true, CheckNewContext()));
                    GL.ReadPixels(0, 0, screenshotWidth, screenshotHeight,
                        PixelFormat.Bgra, PixelType.UnsignedByte,
                        screenshot);
                    tcs.SetResult(screenshot);
                }
                catch (Exception e)
                {
                    tcs.SetException(e);
                }
            }
        }

        private void CalculateBufferSize(out int width, out int height)
        {
            width = (int)(ActualWidth / volatilePixelScale);
            height = (int)(ActualHeight / volatilePixelScale);

            if (width <= 0 || height <= 0)
                return;

            if (width * height > volatileMaxPixels)
            {
                var scale = (float)Math.Sqrt((float)volatileMaxPixels / width / height);
                width = (int)(width * scale);
                height = (int)(height * scale);
            }
        }

        private void UpdateImage(Int32Rect dirtyArea)
        {
            var bitmap = _bitmap;
            if (bitmap == null)
            {
                Image.Source = null;
                return;
            }

            bitmap.Lock();
            bitmap.AddDirtyRect(dirtyArea);
            bitmap.Unlock();

            Image.Source = bitmap;
        }

        private void OnGLRender(GlRenderEventArgs args)
        {
            GLRender?.Invoke(this, args);

            var error = GL.GetError();
            if (error != ErrorCode.NoError)
                throw new GraphicsErrorException($"Error while rendering: {error}");
        }

        /// <summary>
        /// Updates <see cref="_newContext"/>
        /// </summary>
        /// <returns>True if there is a new context</returns>
        private bool CheckNewContext()
        {
            if (!_newContext) return false;
            _newContext = false;
            return true;
        }

        #endregion

        #region OpenGL Buffers

        private void CreateOpenGLBuffers(int width, int height)
        {
            UninitializeOpenGLBuffers();

            _frameBuffer = GL.GenFramebuffer();
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, _frameBuffer);

            _depthBuffer = GL.GenRenderbuffer();
            GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, _depthBuffer);
            GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, RenderbufferStorage.DepthComponent24, width, height);
            GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, RenderbufferTarget.Renderbuffer, _depthBuffer);

            _renderBuffer = GL.GenRenderbuffer();
            GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, _renderBuffer);
            GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, RenderbufferStorage.Rgba8, width, height);
            GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, RenderbufferTarget.Renderbuffer, _renderBuffer);

            var error = GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer);
            if (error != FramebufferErrorCode.FramebufferComplete)
                throw new GraphicsErrorException($"Error creating frame buffer: {error}");
        }

        private void UninitializeOpenGLBuffers()
        {
            if (_frameBuffer != 0)
            {
                GL.DeleteFramebuffer(_frameBuffer);
                _frameBuffer = 0;
            }

            if (_depthBuffer != 0)
            {
                GL.DeleteRenderbuffer(_depthBuffer);
                _depthBuffer = 0;
            }

            if (_renderBuffer != 0)
            {
                GL.DeleteRenderbuffer(_renderBuffer);
                _renderBuffer = 0;
            }
        }

        #endregion

        #region Misc

        private Task RunOnUIThread(Action action) => Dispatcher.InvokeAsync(action).Task;

        #endregion
    }
}
