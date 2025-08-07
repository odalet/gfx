using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Logging;
using Avalonia.OpenGL;
using Avalonia.OpenGL.Controls;
using Avalonia.Threading;

namespace ScanPlayer.Controls;

public class TeapotControl1 : OpenGlControlBase
{
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    private struct Vertex
    {
        public Vector3 Position;
        public Vector3 Normal;
    }

    private sealed class GlExtrasInterface : GlInterfaceBase<GlInterface.GlContextInfo>
    {
        public GlExtrasInterface(GlInterface gl) : base(gl.GetProcAddress, gl.ContextInfo) { }

        public delegate void GlDeleteVertexArrays(int count, int[] buffers);
        [GlMinVersionEntryPoint("glDeleteVertexArrays", 3, 0), GlExtensionEntryPoint("glDeleteVertexArraysOES", "GL_OES_vertex_array_object")]
        public GlDeleteVertexArrays? DeleteVertexArrays { get; }

        public delegate void GlBindVertexArray(int array);
        [GlMinVersionEntryPoint("glBindVertexArray", 3, 0), GlExtensionEntryPoint("glBindVertexArrayOES", "GL_OES_vertex_array_object")]
        public GlBindVertexArray? BindVertexArray { get; }

        public delegate void GlGenVertexArrays(int n, int[] rv);
        [GlMinVersionEntryPoint("glGenVertexArrays", 3, 0), GlExtensionEntryPoint("glGenVertexArraysOES", "GL_OES_vertex_array_object")]
        public GlGenVertexArrays? GenVertexArrays { get; }

        public int GenVertexArray()
        {
            var rv = new int[1];
            GenVertexArrays!(1, rv);
            return rv[0];
        }
    }

    private static readonly bool enableLogging = false;
    private static readonly bool useWriteLine = false;
    private static readonly Stopwatch stopwatch = Stopwatch.StartNew();

    private readonly Vertex[] points;
    private readonly ushort[] indices;
    private readonly float minY;
    private readonly float maxY;
    private int vertexShader;
    private int fragmentShader;
    private int shaderProgram;
    private int vertexBufferObject;
    private int indexBufferObject;
    private int vertexArrayObject;
    private GlExtrasInterface? glExt;

    static TeapotControl1() => AffectsRender<TeapotControl1>(YawProperty, PitchProperty, RollProperty, DiscoProperty);

    public TeapotControl1()
    {
        var name = typeof(TeapotControl1).Assembly.GetManifestResourceNames().First(x => x.Contains("teapot.bin"));
        var stream = typeof(TeapotControl1).Assembly.GetManifestResourceStream(name);
        if (stream == null)
        {
            points = Array.Empty<Vertex>();
            indices = Array.Empty<ushort>();
            return;
        }

        using var sr = new BinaryReader(stream);

        var buffer = new byte[sr.ReadInt32()];

        _ = sr.Read(buffer, 0, buffer.Length);
        var readPoints = new float[buffer.Length / 4];
        Buffer.BlockCopy(buffer, 0, readPoints, 0, buffer.Length);

        buffer = new byte[sr.ReadInt32()];
        _ = sr.Read(buffer, 0, buffer.Length);
        indices = new ushort[buffer.Length / 2];
        Buffer.BlockCopy(buffer, 0, indices, 0, buffer.Length);
        points = new Vertex[readPoints.Length / 3];

        for (var primitive = 0; primitive < readPoints.Length / 3; primitive++)
        {
            var srci = primitive * 3;
            points[primitive] = new Vertex
            {
                Position = new Vector3(readPoints[srci], readPoints[srci + 1], readPoints[srci + 2])
            };
        }

        for (var i = 0; i < indices.Length; i += 3)
        {
            var a = points[indices[i]].Position;
            var b = points[indices[i + 1]].Position;
            var c = points[indices[i + 2]].Position;
            var normal = Vector3.Normalize(Vector3.Cross(c - b, a - b));

            points[indices[i]].Normal += normal;
            points[indices[i + 1]].Normal += normal;
            points[indices[i + 2]].Normal += normal;
        }

        for (var i = 0; i < points.Length; i++)
        {
            points[i].Normal = Vector3.Normalize(points[i].Normal);
            maxY = Math.Max(maxY, points[i].Position.Y);
            minY = Math.Min(minY, points[i].Position.Y);
        }
    }

    public static readonly DirectProperty<TeapotControl1, float> YawProperty =
        AvaloniaProperty.RegisterDirect<TeapotControl1, float>(nameof(Yaw), o => o.Yaw, (o, v) => o.Yaw = v);

    private float yaw;
    public float Yaw
    {
        get => yaw;
        set => SetAndRaise(YawProperty, ref yaw, value);
    }

    public static readonly DirectProperty<TeapotControl1, float> PitchProperty =
        AvaloniaProperty.RegisterDirect<TeapotControl1, float>(nameof(Pitch), o => o.Pitch, (o, v) => o.Pitch = v);

    private float pitch;
    public float Pitch
    {
        get => pitch;
        set => SetAndRaise(PitchProperty, ref pitch, value);
    }

    public static readonly DirectProperty<TeapotControl1, float> RollProperty =
        AvaloniaProperty.RegisterDirect<TeapotControl1, float>(nameof(Roll), o => o.Roll, (o, v) => o.Roll = v);

    private float roll;
    public float Roll
    {
        get => roll;
        set => SetAndRaise(RollProperty, ref roll, value);
    }

    public static readonly DirectProperty<TeapotControl1, float> DiscoProperty =
        AvaloniaProperty.RegisterDirect<TeapotControl1, float>(nameof(Disco), o => o.Disco, (o, v) => o.Disco = v);

    private float disco;
    public float Disco
    {
        get => disco;
        set => SetAndRaise(DiscoProperty, ref disco, value);
    }

    public static readonly DirectProperty<TeapotControl1, string> InfoProperty = AvaloniaProperty.RegisterDirect<TeapotControl1, string>(
        nameof(Info), o => o.Info, (o, v) => o.Info = v);

    private string info = "";
    public string Info
    {
        get => info;
        private set => SetAndRaise(InfoProperty, ref info, value);
    }

    private string VertexShaderSource => GetShader(false, @"
        attribute vec3 aPos;
        attribute vec3 aNormal;
        uniform mat4 uModel;
        uniform mat4 uProjection;
        uniform mat4 uView;

        varying vec3 FragPos;
        varying vec3 VecPos;  
        varying vec3 Normal;
        uniform float uTime;
        uniform float uDisco;
        void main()
        {
            float discoScale = sin(uTime * 10.0) / 10.0;
            float distortionX = 1.0 + uDisco * cos(uTime * 20.0) / 10.0;
            
            float scale = 1.0 + uDisco * discoScale;
            
            vec3 scaledPos = aPos;
            scaledPos.x = scaledPos.x * distortionX;
            
            scaledPos *= scale;
            gl_Position = uProjection * uView * uModel * vec4(scaledPos, 1.0);
            FragPos = vec3(uModel * vec4(aPos, 1.0));
            VecPos = aPos;
            Normal = normalize(vec3(uModel * vec4(aNormal, 1.0)));
        }
");

    private string FragmentShaderSource => GetShader(true, @"
        varying vec3 FragPos; 
        varying vec3 VecPos; 
        varying vec3 Normal;
        uniform float uMaxY;
        uniform float uMinY;
        uniform float uTime;
        uniform float uDisco;
        //DECLAREGLFRAG

        void main()
        {
            float y = (VecPos.y - uMinY) / (uMaxY - uMinY);
            float c = cos(atan(VecPos.x, VecPos.z) * 20.0 + uTime * 40.0 + y * 50.0);
            float s = sin(-atan(VecPos.z, VecPos.x) * 20.0 - uTime * 20.0 - y * 30.0);

            vec3 discoColor = vec3(
                0.5 + abs(0.5 - y) * cos(uTime * 10.0),
                0.25 + (smoothstep(0.3, 0.8, y) * (0.5 - c / 4.0)),
                0.25 + abs((smoothstep(0.1, 0.4, y) * (0.5 - s / 4.0))));

            vec3 objectColor = vec3((1.0 - y), 0.40 +  y / 4.0, y * 0.75 + 0.25);
            objectColor = objectColor * (1.0 - uDisco) + discoColor * uDisco;

            float ambientStrength = 0.3;
            vec3 lightColor = vec3(1.0, 1.0, 1.0);
            vec3 lightPos = vec3(uMaxY * 2.0, uMaxY * 2.0, uMaxY * 2.0);
            vec3 ambient = ambientStrength * lightColor;


            vec3 norm = normalize(Normal);
            vec3 lightDir = normalize(lightPos - FragPos);  

            float diff = max(dot(norm, lightDir), 0.0);
            vec3 diffuse = diff * lightColor;

            vec3 result = (ambient + diffuse) * objectColor;
            gl_FragColor = vec4(result, 1.0);

        }
");

    protected override unsafe void OnOpenGlRender(GlInterface gl, int fb)
    {
        gl.ClearColor(0, 0, 0, 0);
        gl.Clear(GlConsts.GL_COLOR_BUFFER_BIT | GlConsts.GL_DEPTH_BUFFER_BIT);
        gl.Enable(GlConsts.GL_DEPTH_TEST);
        gl.Viewport(0, 0, (int)Bounds.Width, (int)Bounds.Height);
        gl.BindBuffer(GlConsts.GL_ARRAY_BUFFER, vertexBufferObject);
        gl.BindBuffer(GlConsts.GL_ELEMENT_ARRAY_BUFFER, indexBufferObject);
        glExt!.BindVertexArray!(vertexArrayObject);
        gl.UseProgram(shaderProgram);
        CheckGLError(gl);

        var projection = Matrix4x4.CreatePerspectiveFieldOfView(
            (float)(Math.PI / 4), (float)(Bounds.Width / Bounds.Height), 0.01f, 1000f);

        var view = Matrix4x4.CreateLookAt(new Vector3(25, 25, 25), new Vector3(), new Vector3(0, -1, 0));
        var model = Matrix4x4.CreateFromYawPitchRoll(yaw, pitch, roll);
        var modelLoc = gl.GetUniformLocationString(shaderProgram, "uModel");
        var viewLoc = gl.GetUniformLocationString(shaderProgram, "uView");
        var projectionLoc = gl.GetUniformLocationString(shaderProgram, "uProjection");
        var maxYLoc = gl.GetUniformLocationString(shaderProgram, "uMaxY");
        var minYLoc = gl.GetUniformLocationString(shaderProgram, "uMinY");
        var timeLoc = gl.GetUniformLocationString(shaderProgram, "uTime");
        var discoLoc = gl.GetUniformLocationString(shaderProgram, "uDisco");
        gl.UniformMatrix4fv(modelLoc, 1, false, &model);
        gl.UniformMatrix4fv(viewLoc, 1, false, &view);
        gl.UniformMatrix4fv(projectionLoc, 1, false, &projection);
        gl.Uniform1f(maxYLoc, maxY);
        gl.Uniform1f(minYLoc, minY);
        gl.Uniform1f(timeLoc, (float)stopwatch.Elapsed.TotalSeconds);
        gl.Uniform1f(discoLoc, disco);
        CheckGLError(gl);

        gl.DrawElements(GlConsts.GL_TRIANGLES, indices.Length, GlConsts.GL_UNSIGNED_SHORT, IntPtr.Zero);
        CheckGLError(gl);

        if (disco > 0.01)
            Dispatcher.UIThread.Post(InvalidateVisual, DispatcherPriority.Background);
    }

    protected override unsafe void OnOpenGlInit(GlInterface gl, int fb)
    {
        CheckGLError(gl);
        glExt = new GlExtrasInterface(gl);

        static string format(GlVersion glv) => $"{glv.Type} v{glv.Major}.{glv.Minor}";
        Info = $"{GetType().Name}, FB: {fb}\r\nRenderer: {gl.GetString(GlConsts.GL_RENDERER)}\r\nVersion: {gl.GetString(GlConsts.GL_VERSION)}\r\nGL: {format(GlVersion)}";

        // Load the source of the vertex shader and compile it.
        vertexShader = gl.CreateShader(GlConsts.GL_VERTEX_SHADER);
        Console.WriteLine(gl.CompileShaderAndGetError(vertexShader, VertexShaderSource));

        // Load the source of the fragment shader and compile it.
        fragmentShader = gl.CreateShader(GlConsts.GL_FRAGMENT_SHADER);
        Console.WriteLine(gl.CompileShaderAndGetError(fragmentShader, FragmentShaderSource));

        // Create the shader program, attach the vertex and fragment shaders and link the program.
        shaderProgram = gl.CreateProgram();
        gl.AttachShader(shaderProgram, vertexShader);
        gl.AttachShader(shaderProgram, fragmentShader);
        const int positionLocation = 0;
        const int normalLocation = 1;
        gl.BindAttribLocationString(shaderProgram, positionLocation, "aPos");
        gl.BindAttribLocationString(shaderProgram, normalLocation, "aNormal");
        Console.WriteLine(gl.LinkProgramAndGetError(shaderProgram));
        CheckGLError(gl);

        // Create the vertex buffer object (VBO) for the vertex data, then bind the VBO and copy the vertex data into it.
        vertexBufferObject = gl.GenBuffer();
        gl.BindBuffer(GlConsts.GL_ARRAY_BUFFER, vertexBufferObject);
        CheckGLError(gl);

        var vertexSize = Marshal.SizeOf<Vertex>();
        fixed (void* pdata = points) gl.BufferData(
            GlConsts.GL_ARRAY_BUFFER, new IntPtr(points.Length * vertexSize), new IntPtr(pdata), GlConsts.GL_STATIC_DRAW);
        CheckGLError(gl);

        indexBufferObject = gl.GenBuffer();
        gl.BindBuffer(GlConsts.GL_ELEMENT_ARRAY_BUFFER, indexBufferObject);
        CheckGLError(gl);

        fixed (void* pdata = indices) gl.BufferData(
            GlConsts.GL_ELEMENT_ARRAY_BUFFER, new IntPtr(indices.Length * sizeof(ushort)), new IntPtr(pdata), GlConsts.GL_STATIC_DRAW);
        CheckGLError(gl);

        vertexArrayObject = glExt.GenVertexArray();
        glExt.BindVertexArray!(vertexArrayObject);
        CheckGLError(gl);

        gl.VertexAttribPointer(positionLocation, 3, GlConsts.GL_FLOAT, 0, vertexSize, IntPtr.Zero);
        gl.VertexAttribPointer(normalLocation, 3, GlConsts.GL_FLOAT, 0, vertexSize, new IntPtr(12));
        gl.EnableVertexAttribArray(positionLocation);
        gl.EnableVertexAttribArray(normalLocation);
        CheckGLError(gl);
    }

    protected override void OnOpenGlDeinit(GlInterface gl, int fb)
    {
        // Unbind everything
        gl.BindBuffer(GlConsts.GL_ARRAY_BUFFER, 0);
        gl.BindBuffer(GlConsts.GL_ELEMENT_ARRAY_BUFFER, 0);
        glExt!.BindVertexArray!(0);
        gl.UseProgram(0);

        // Delete all resources.
        gl.DeleteBuffers(2, new[] { vertexBufferObject, indexBufferObject });
        glExt.DeleteVertexArrays!(1, new[] { vertexArrayObject });
        gl.DeleteProgram(shaderProgram);
        gl.DeleteShader(fragmentShader);
        gl.DeleteShader(vertexShader);
    }

    private string GetShader(bool fragment, string shader)
    {
        var version = GlVersion.Type == GlProfileType.OpenGL ?
            RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ? 150 : 120 : 100;

        var data = "#version " + version + "\n";
        if (GlVersion.Type == GlProfileType.OpenGLES)
            data += "precision mediump float;\n";

        if (version >= 150)
        {
            shader = shader.Replace("attribute", "in");
            shader = fragment
                ? shader
                    .Replace("varying", "in")
                    .Replace("//DECLAREGLFRAG", "out vec4 outFragColor;")
                    .Replace("gl_FragColor", "outFragColor")
                : shader
                    .Replace("varying", "out");
        }

        data += shader;

        return data;
    }

    private void CheckGLError(GlInterface gl)
    {
        int err;
        while ((err = gl.GetError()) != GlConsts.GL_NO_ERROR)
            LogError(err.ToString());
    }

    private void LogError(string message)
    {
        if (!enableLogging) return;
        Logger.TryGet(LogEventLevel.Error, "Misc")?.Log(this, message);
        if (useWriteLine)
            Console.WriteLine(message);
    }
}
