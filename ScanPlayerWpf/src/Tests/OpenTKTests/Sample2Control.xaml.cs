using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTKTests.Rendering;

namespace OpenTKTests
{
    /// <summary>
    /// Interaction logic for Sample2Control.xaml
    /// </summary>
    public partial class Sample2Control : UserControl
    {
        public Sample2Control()
        {
            InitializeComponent();
            if (DesignerProperties.GetIsInDesignMode(this)) return;

            tkControl.Continuous = false;

            Trackball = new Trackball();
            InitializeTrackball();

            Redraw();
        }

        private CursorScope cursorScope;
        private bool firstMouseMove = true;
        private Point? initialMouseLocation = null;
        private Point previousMouseLocation = new Point();

        protected virtual double ScalingRatio => 1.0 / 300.0;
        private Trackball Trackball { get; }
        protected bool MouseLeftButtonPressed { get; set; }
        protected bool MouseRighButtonPressed { get; set; }
        protected bool MouseMiddleButtonPressed { get; set; }

        private void InitializeTrackball()
        {
            tkControl.MouseDown += (s, e) =>
            {
                _ = Mouse.Capture(tkControl);

                initialMouseLocation = e.GetPosition(tkControl);
                switch (e.ChangedButton)
                {
                    case MouseButton.Left:
                        cursorScope = new CursorScope(tkControl, Cursors.Hand);
                        MouseLeftButtonPressed = true;
                        break;
                    case MouseButton.Middle:
                        MouseMiddleButtonPressed = true;
                        break;
                    case MouseButton.Right:
                        cursorScope = new CursorScope(tkControl, Cursors.ScrollAll);
                        MouseRighButtonPressed = true;
                        break;
                }
            };

            tkControl.MouseUp += (s, e) =>
            {
                _ = Mouse.Capture(null);

                if (cursorScope != null)
                {
                    cursorScope.Dispose();
                    cursorScope = null;
                }

                double getSquaredDistance(Point? p1, Point? p2)
                {
                    if (!p1.HasValue || !p2.HasValue) return 0.0;
                    var dx = p2.Value.X - p1.Value.X;
                    var dy = p2.Value.Y - p1.Value.Y;
                    return dx * dx + dy * dy;
                }

                switch (e.ChangedButton)
                {
                    case MouseButton.Left:
                        MouseLeftButtonPressed = false;
                        break;
                    case MouseButton.Middle:
                        if (e.ClickCount == 1 && getSquaredDistance(e.GetPosition(tkControl), initialMouseLocation) < 4.0)
                        {
                            Trackball.Reset();
                            Redraw();
                        }

                        MouseMiddleButtonPressed = false;
                        break;
                    case MouseButton.Right:
                        MouseRighButtonPressed = false;
                        break;
                }

                initialMouseLocation = null;
            };

            tkControl.MouseMove += (s, e) =>
            {
                var location = e.GetPosition(tkControl);
                if (firstMouseMove)
                {
                    firstMouseMove = false;
                    previousMouseLocation = initialMouseLocation.HasValue ? initialMouseLocation.Value : location;
                    return;
                }

                var dx = location.X - previousMouseLocation.X;
                var dy = location.Y - previousMouseLocation.Y;
                previousMouseLocation = location;

                if (MouseLeftButtonPressed)
                {
                    Trackball.X += (float)dx / Trackball.Radius;
                    Trackball.Y -= (float)dy / Trackball.Radius;
                }

                if (MouseRighButtonPressed)
                {
                    Trackball.Phi += (float)Math.PI * ((float)dx / 1024);
                    Trackball.Theta += (float)Math.PI * ((float)dy / 1024);
                }

                Redraw();
            };

            tkControl.MouseWheel += (s, e) =>
            {
                if (Keyboard.Modifiers == ModifierKeys.Control)
                {
                    var delta = e.Delta / System.Windows.Forms.SystemInformation.MouseWheelScrollDelta;
                    var scale = 1f + delta * 0.1f;
                    var radius = Trackball.Radius * scale;
                    radius = radius < 0.02f ? 0.02f : radius;
                    radius = radius > 50f ? 50f : radius;
                    Trackball.Radius = radius;
                }

                Redraw();
            };
        }

        private void Redraw() => _ = tkControl.RequestRepaintAsync();

        private void SetupViewport()
        {
            var length = Math.Max(tkControl.ActualWidth, tkControl.ActualHeight);
            if (length == tkControl.ActualWidth)
                GL.Viewport(0, (int)((tkControl.ActualHeight - length) / 2.0), (int)length, (int)length);
            else
                GL.Viewport((int)((tkControl.ActualWidth - length) / 2.0), 0, (int)length, (int)length);
        }

        private void DrawSceneBounds()
        {
            GL.Color4(0.9, 0.9, 0.9, 1.0);
            GL.LineWidth(1f);
            GL.Begin(PrimitiveType.Quads);

            const double z = 0.1;
            var length = 1.1 / ScalingRatio;

            GL.Vertex3(-length, -length, z);
            GL.Vertex3(length, -length, z);
            GL.Vertex3(length, length, z);
            GL.Vertex3(-length, length, z);

            GL.End();
        }

        private void DrawReference()
        {
            const double z = 0.0;
            var length = 0.2 / ScalingRatio;

            // X
            GL.Color4(1.0, 0.0, 0.0, 1.0);
            GL.LineWidth(1f);
            GL.Begin(PrimitiveType.LineStrip);
            GL.Vertex3(0.0, 0.0, z);
            GL.Vertex3(length, 0.0, z);
            GL.End();

            // Y
            GL.Color4(0.0, 1.0, 0.0, 1.0);
            GL.LineWidth(1f);
            GL.Begin(PrimitiveType.LineStrip);
            GL.Vertex3(0.0, 0.0, z);
            GL.Vertex3(0.0, length, z);
            GL.End();

            // Z
            GL.Color4(0.0, 0.0, 1.0, 1.0);
            GL.LineWidth(1f);
            GL.Begin(PrimitiveType.LineStrip);
            GL.Vertex3(0.0, 0.0, z);
            GL.Vertex3(0.0, 0.0, z + length);
            GL.End();
        }

        private void tkControl_GLRender(object sender, GlRenderEventArgs e)
        {
            SetupViewport();

            var modelView = Matrix4.LookAt(Vector3.Zero, Vector3.UnitZ, Vector3.UnitY);

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.Enable(EnableCap.DepthTest);
            GL.PushMatrix();
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadMatrix(ref modelView);
            GL.Rotate(Trackball.Phi * 180.0 / Math.PI, 0.0f, 1.0f, 0.0f);
            GL.Rotate(Trackball.Theta * 180.0 / Math.PI, -1.0f, 0.0f, 0.0f);

            var scale = Trackball.Radius * ScalingRatio;
            GL.Scale(scale, scale, scale);

            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
            GL.Enable(EnableCap.Blend);

            GL.Translate(Trackball.X, Trackball.Y, 0.0);
            GL.Color4(0.0, 0.0, 1.0, 1.0);

            //-------------------------------

            //DrawSceneBounds();
            DrawReference();


            //-------------------------------

            GL.PopMatrix();
        }

        private void tkControl_ExceptionOccurred(object sender, UnhandledExceptionEventArgs e) => Debug.WriteLine(e.ExceptionObject);
    }
}
