using System.Windows;
using OpenGL;

namespace WpfApp2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static readonly float[] positions = new[]
        {
            0f, 0f,
            .5f, 1f,
            1f, 0f
        };

        private static readonly float[] colors = new[]
        {
            1f, 0f, 0f,
            0f, 1f, 0f,
            0f, 0f, 1f
        };

        public MainWindow() => InitializeComponent();

        private void HostControl_Loaded(object sender, RoutedEventArgs e)
        {
            // Nothing to do here
        }

        private void GlControl_ContextCreated(object sender, GlControlEventArgs e)
        {
            Gl.Enable(EnableCap.DepthTest);

            Gl.MatrixMode(MatrixMode.Projection);
            Gl.LoadIdentity();
            Gl.Ortho(0.0, 1.0f, 0.0, 1.0, 0.0, 1.0);

            Gl.MatrixMode(MatrixMode.Modelview);
            Gl.LoadIdentity();
        }

        private void GlControl_Render(object sender, GlControlEventArgs e)
        {
            var senderControl = (GlControl)sender;

            //Draw2(senderControl.ClientSize.Width, senderControl.ClientSize.Height);
            Draw1(senderControl.ClientSize.Width, senderControl.ClientSize.Height);
        }

        private void Draw1(int width, int height)
        {
            var vpx = 0;
            var vpy = 0;
            var vpw = width;
            var vph = height;

            Gl.Viewport(vpx, vpy, vpw, vph);
            Gl.Clear(ClearBufferMask.ColorBufferBit);

            if (Gl.CurrentVersion >= Gl.Version_110)
            {
                // Old school OpenGL 1.1
                // Setup & enable client states to specify vertex arrays, and use Gl.DrawArrays instead of Gl.Begin/End paradigm
                using (var vertexArrayLock = new MemoryLock(positions))
                using (var vertexColorLock = new MemoryLock(colors))
                {
                    // Note: the use of MemoryLock objects is necessary to pin vertex arrays since they can be reallocated by GC
                    // at any time between the Gl.VertexPointer execution and the Gl.DrawArrays execution

                    Gl.VertexPointer(2, VertexPointerType.Float, 0, vertexArrayLock.Address);
                    Gl.EnableClientState(EnableCap.VertexArray);

                    Gl.ColorPointer(3, ColorPointerType.Float, 0, vertexColorLock.Address);
                    Gl.EnableClientState(EnableCap.ColorArray);

                    Gl.DrawArrays(PrimitiveType.Triangles, 0, 3);
                }
            }
            else
            {
                // Old school OpenGL
                Gl.Begin(PrimitiveType.Triangles);
                Gl.Color3(1f, 0f, 0f); Gl.Vertex2(0f, 0f);
                Gl.Color3(0f, 1f, 0f); Gl.Vertex2(.5f, 1f);
                Gl.Color3(0f, 0f, 1f); Gl.Vertex2(1f, 0f);
                Gl.End();
            }
        }

        private float rotatePyramid = 0f;
        private float rquad = 0f;

        // Adapted from https://www.codeproject.com/Articles/265903/Using-OpenGL-in-a-WPF-Application
        private void Draw2(int width, int height)
        {
            var vpx = 0;
            var vpy = 0;
            var vpw = width;
            var vph = height;

            Gl.Viewport(vpx, vpy, vpw, vph);

            //  Get the OpenGL instance that's been passed to us.
            //var Gl = Gl;

            //  Clear the color and depth buffers.
            Gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit); // OpenGL.GL_COLOR_BUFFER_BIT | OpenGL.GL_DEPTH_BUFFER_BIT


            Gl.MatrixMode(MatrixMode.Projection);
            Gl.LoadIdentity();
            Gl.Ortho(-10.0, 10.0f, -10.0, 10.0, 0.0, 1.0);

            Gl.MatrixMode(MatrixMode.Modelview);
            Gl.LoadIdentity();


            //////  Reset the modelview matrix.
            ////Gl.LoadIdentity();

            //////  Move the geometry into a fairly central position.
            ////Gl.Translate(-1.5f, 0.0f, -6.0f);

            //  Draw a pyramid. First, rotate the modelview matrix.
            Gl.Rotate(rotatePyramid, 0.0f, 1.0f, 0.0f);

            //  Start drawing triangles.
            Gl.Begin(PrimitiveType.Triangles); // OpenGL.GL_TRIANGLES

            Gl.Color3(1.0f, 0.0f, 0.0f);
            Gl.Vertex3(0.0f, 1.0f, 0.0f);
            Gl.Color3(0.0f, 1.0f, 0.0f);
            Gl.Vertex3(-1.0f, -1.0f, 1.0f);
            Gl.Color3(0.0f, 0.0f, 1.0f);
            Gl.Vertex3(1.0f, -1.0f, 1.0f);

            Gl.Color3(1.0f, 0.0f, 0.0f);
            Gl.Vertex3(0.0f, 1.0f, 0.0f);
            Gl.Color3(0.0f, 0.0f, 1.0f);
            Gl.Vertex3(1.0f, -1.0f, 1.0f);
            Gl.Color3(0.0f, 1.0f, 0.0f);
            Gl.Vertex3(1.0f, -1.0f, -1.0f);

            Gl.Color3(1.0f, 0.0f, 0.0f);
            Gl.Vertex3(0.0f, 1.0f, 0.0f);
            Gl.Color3(0.0f, 1.0f, 0.0f);
            Gl.Vertex3(1.0f, -1.0f, -1.0f);
            Gl.Color3(0.0f, 0.0f, 1.0f);
            Gl.Vertex3(-1.0f, -1.0f, -1.0f);

            Gl.Color3(1.0f, 0.0f, 0.0f);
            Gl.Vertex3(0.0f, 1.0f, 0.0f);
            Gl.Color3(0.0f, 0.0f, 1.0f);
            Gl.Vertex3(-1.0f, -1.0f, -1.0f);
            Gl.Color3(0.0f, 1.0f, 0.0f);
            Gl.Vertex3(-1.0f, -1.0f, 1.0f);

            Gl.End();

            ////  Reset the modelview.
            //Gl.LoadIdentity();

            ////  Move into a more central position.
            //Gl.Translate(1.5f, 0.0f, -7.0f);

            //  Rotate the cube.
            Gl.Rotate(rquad, 1.0f, 1.0f, 1.0f);

            //  Provide the cube colors and geometry.
            Gl.Begin(PrimitiveType.Quads); // OpenGL.GL_QUADS

            Gl.Color3(0.0f, 1.0f, 0.0f);
            Gl.Vertex3(1.0f, 1.0f, -1.0f);
            Gl.Vertex3(-1.0f, 1.0f, -1.0f);
            Gl.Vertex3(-1.0f, 1.0f, 1.0f);
            Gl.Vertex3(1.0f, 1.0f, 1.0f);

            Gl.Color3(1.0f, 0.5f, 0.0f);
            Gl.Vertex3(1.0f, -1.0f, 1.0f);
            Gl.Vertex3(-1.0f, -1.0f, 1.0f);
            Gl.Vertex3(-1.0f, -1.0f, -1.0f);
            Gl.Vertex3(1.0f, -1.0f, -1.0f);

            Gl.Color3(1.0f, 0.0f, 0.0f);
            Gl.Vertex3(1.0f, 1.0f, 1.0f);
            Gl.Vertex3(-1.0f, 1.0f, 1.0f);
            Gl.Vertex3(-1.0f, -1.0f, 1.0f);
            Gl.Vertex3(1.0f, -1.0f, 1.0f);

            Gl.Color3(1.0f, 1.0f, 0.0f);
            Gl.Vertex3(1.0f, -1.0f, -1.0f);
            Gl.Vertex3(-1.0f, -1.0f, -1.0f);
            Gl.Vertex3(-1.0f, 1.0f, -1.0f);
            Gl.Vertex3(1.0f, 1.0f, -1.0f);

            Gl.Color3(0.0f, 0.0f, 1.0f);
            Gl.Vertex3(-1.0f, 1.0f, 1.0f);
            Gl.Vertex3(-1.0f, 1.0f, -1.0f);
            Gl.Vertex3(-1.0f, -1.0f, -1.0f);
            Gl.Vertex3(-1.0f, -1.0f, 1.0f);

            Gl.Color3(1.0f, 0.0f, 1.0f);
            Gl.Vertex3(1.0f, 1.0f, -1.0f);
            Gl.Vertex3(1.0f, 1.0f, 1.0f);
            Gl.Vertex3(1.0f, -1.0f, 1.0f);
            Gl.Vertex3(1.0f, -1.0f, -1.0f);

            Gl.End();

            //  Flush OpenGL.
            Gl.Flush();

            //  Rotate the geometry a bit.
            rotatePyramid += 3.0f;
            rquad -= 3.0f;
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e) => Application.Current.Shutdown();
    }
}
