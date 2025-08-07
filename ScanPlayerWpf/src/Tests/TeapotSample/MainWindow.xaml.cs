using System;
using System.Windows;
using System.Windows.Input;
using SharpGL;
using SharpGL.SceneGraph;
using SharpGL.SceneGraph.Primitives;

namespace Example1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MainWindow"/> class.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            Trackball = new Trackball();
            InitializeTrackball();
        }

        private bool firstMouseMove = true;
        private Point previousMouseLocation = new Point();

        protected virtual double ScalingRatio => 1.0 / 300.0;
        private Trackball Trackball { get; }
        protected bool MouseLeftButtonPressed { get; set; }
        protected bool MouseRighButtonPressed { get; set; }
        protected bool MouseMiddleButtonPressed { get; set; }

        private void InitializeTrackball()
        {
            glControl.MouseDown += (s, e) =>
            {
                switch (e.ChangedButton)
                {
                    case MouseButton.Left:
                        ////cursorScope = new CursorScope(glControl, Cursors.Hand);
                        MouseLeftButtonPressed = true;
                        break;
                    case MouseButton.Middle:
                        MouseMiddleButtonPressed = true;
                        break;
                    case MouseButton.Right:
                        ////cursorScope = new CursorScope(glControl, Cursors.NoMove2D);
                        MouseRighButtonPressed = true;
                        break;
                }
            };

            glControl.MouseUp += (s, e) =>
            {
                ////if (cursorScope != null)
                ////{
                ////    cursorScope.Dispose();
                ////    cursorScope = null;
                ////}

                switch (e.ChangedButton)
                {
                    case MouseButton.Left:
                        MouseLeftButtonPressed = false;
                        break;
                    case MouseButton.Middle:
                        MouseMiddleButtonPressed = false;
                        break;
                    case MouseButton.Right:
                        MouseRighButtonPressed = false;
                        break;
                }
            };

            glControl.MouseMove += (s, e) =>
            {
                var location = e.GetPosition(glControl);
                if (firstMouseMove)
                {
                    firstMouseMove = false;
                    previousMouseLocation = location;
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

                ////glControl.Invalidate();
                Redraw();
            };

            glControl.MouseWheel += (s, e) =>
            {
                if (Keyboard.Modifiers == ModifierKeys.Control)
                ////if (Control.ModifierKeys == Keys.Control)
                {
                    ////var delta = e.Delta / SystemInformation.MouseWheelScrollDelta;
                    var delta = e.Delta / SystemParameters.WheelScrollLines;
                    var scale = 1f + delta * 0.1f;
                    var radius = Trackball.Radius * scale;
                    radius = radius < 0.02f ? 0.02f : radius;
                    radius = radius > 50f ? 50f : radius;
                    Trackball.Radius = radius;
                }

                ////glControl.Invalidate();
                Redraw();
            };
        }

        private void Redraw()
        {
            //glControl.RenderTrigger = RenderTrigger.Manual;
            //glControl.DoRender();
        }

        /// <summary>
        /// Handles the OpenGLDraw event of the OpenGLControl control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="args">The <see cref="SharpGL.SceneGraph.OpenGLEventArgs"/> instance containing the event data.</param>
        private void OpenGLControl_OpenGLDraw(object sender, OpenGLEventArgs args)
        {
            var gl = args.OpenGL;

            InitializeGL(gl);

            ////// Clear The Screen And The Depth Buffer
            ////gl.Clear(OpenGL.GL_COLOR_BUFFER_BIT | OpenGL.GL_DEPTH_BUFFER_BIT);            
            ////// Move Left And Into The Screen
            ////gl.LoadIdentity();
            ////gl.Translate(0.0f, 0.0f, -6.0f);	
            ////gl.Rotate(rotation, 0.0f, 1.0f, 0.0f);


            ////var modelView = Matrix4.LookAt(Vector3.Zero, Vector3.UnitZ, Vector3.UnitY);

            gl.Clear(OpenGL.GL_COLOR_BUFFER_BIT | OpenGL.GL_DEPTH_BUFFER_BIT);
            gl.Enable(OpenGL.GL_DEPTH_TEST);
            gl.PushMatrix();
            gl.MatrixMode(OpenGL.GL_MODELVIEW);
            ////gl.LoadMatrix(ref modelView);
            gl.LookAt(0, 0, 0, 0, 0, 1, 0, 1, 0);
            gl.Rotate(Trackball.Phi * 180.0 / Math.PI, 0.0f, 1.0f, 0.0f);
            gl.Rotate(Trackball.Theta * 180.0 / Math.PI, -1.0f, 0.0f, 0.0f);

            var scale = Trackball.Radius * ScalingRatio;
            gl.Scale(scale, scale, scale);

            ////gl.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
            gl.BlendFunc(OpenGL.GL_SRC_ALPHA, OpenGL.GL_ONE_MINUS_SRC_ALPHA);
            ////gl.Enable(EnableCap.Blend);
            gl.Enable(OpenGL.GL_BLEND);

            gl.Translate(Trackball.X, Trackball.Y, 0.0);
            ////gl.Color4(0.0, 0.0, 1.0, 1.0);
            gl.Color(0.0, 0.0, 1.0, 1.0);



            DrawReference(gl);





            //var tp = new Teapot();
            //tp.Draw(gl, 14, 1, OpenGL.GL_FILL);

            ////rotation += 3.0f;
            ///





            gl.PopMatrix();
            //glControl.SwapBuffers();
        }

        private void DrawReference(OpenGL gl)
        {
            const double z = 0.0;
            var length = 10; // 0.2 / ScalingRatio;

            // X
            gl.Color(1.0, 0.0, 0.0, 1.0);
            gl.LineWidth(1f);
            gl.Begin(OpenGL.GL_LINE_STRIP);
            gl.Vertex(0.0, 0.0, z);
            gl.Vertex(length, 0.0, z);
            gl.End();

            // Y
            gl.Color(0.0, 1.0, 0.0, 1.0);
            gl.LineWidth(1f);
            gl.Begin(OpenGL.GL_LINE_STRIP);
            gl.Vertex(0.0, 0.0, z);
            gl.Vertex(0.0, length, z);
            gl.End();

            // Z
            gl.Color(0.0, 0.0, 1.0, 1.0);
            gl.LineWidth(1f);
            gl.Begin(OpenGL.GL_LINE_STRIP);
            gl.Vertex(0.0, 0.0, z);
            gl.Vertex(0.0, 0.0, z + length);
            gl.End();
        }

        //private void DrawSceneBounds()
        //{
        //    GL.Color4(0.9, 0.9, 0.9, 1.0);
        //    GL.LineWidth(1f);
        //    GL.Begin(PrimitiveType.Quads);

        //    const double z = 0.1;
        //    var length = 1.1 / ScalingRatio;

        //    GL.Vertex3(-length, -length, z);
        //    GL.Vertex3(length, -length, z);
        //    GL.Vertex3(length, length, z);
        //    GL.Vertex3(-length, length, z);

        //    GL.End();
        //}

        //float rotation = 0;

        private void InitializeGL(OpenGL gl)
        {
            //gl.ClearColor(1f, 0, 0, 0);
            gl.ClearColor((float)0x87 / 255f, (float)0xCE / 255f, (float)0xEB / 255f, 255f);

            ////SetupViewport();

            var length = Math.Max(glControl.ActualWidth, glControl.ActualHeight);
            if (length == glControl.ActualWidth)
                gl.Viewport(0, (int)((glControl.ActualHeight - length) / 2.0), (int)length, (int)length);
            else
                gl.Viewport((int)((glControl.ActualWidth - length) / 2.0), 0, (int)length, (int)length);

            gl.Viewport(0, 0, (int)glControl.ActualWidth, (int)glControl.ActualHeight);

            ////gl.Enable(OpenGL.GL_DEPTH_TEST);

            ////float[] global_ambient = new float[] { 0.5f, 0.5f, 0.5f, 1.0f };
            ////float[] light0pos = new float[] { 0.0f, 5.0f, 10.0f, 1.0f };
            ////float[] light0ambient = new float[] { 0.2f, 0.2f, 0.2f, 1.0f };
            ////float[] light0diffuse = new float[] { 0.3f, 0.3f, 0.3f, 1.0f };
            ////float[] light0specular = new float[] { 0.8f, 0.8f, 0.8f, 1.0f };

            ////float[] lmodel_ambient = new float[] { 0.2f, 0.2f, 0.2f, 1.0f };
            ////gl.LightModel(OpenGL.GL_LIGHT_MODEL_AMBIENT, lmodel_ambient);

            ////gl.LightModel(OpenGL.GL_LIGHT_MODEL_AMBIENT, global_ambient);
            ////gl.Light(OpenGL.GL_LIGHT0, OpenGL.GL_POSITION, light0pos);
            ////gl.Light(OpenGL.GL_LIGHT0, OpenGL.GL_AMBIENT, light0ambient);
            ////gl.Light(OpenGL.GL_LIGHT0, OpenGL.GL_DIFFUSE, light0diffuse);
            ////gl.Light(OpenGL.GL_LIGHT0, OpenGL.GL_SPECULAR, light0specular);
            ////gl.Enable(OpenGL.GL_LIGHTING);
            ////gl.Enable(OpenGL.GL_LIGHT0);

            ////gl.ShadeModel(OpenGL.GL_SMOOTH);
        }

        /// <summary>
        /// Handles the OpenGLInitialized event of the OpenGLControl control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="args">The <see cref="SharpGL.SceneGraph.OpenGLEventArgs"/> instance containing the event data.</param>
        private void OpenGLControl_OpenGLInitialized(object sender, OpenGLEventArgs args)
        {
            var gl = args.OpenGL;

            //InitializeGL(gl);
        }
    }
}
