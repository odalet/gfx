using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.OpenGL;
using Avalonia.Threading;
using ScanPlayer.OpenGL;

namespace ScanPlayer.Controls;

public class TeapotControl3 : GLSurface
{
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    private struct Vertex
    {
        public Vector3 Position;
        public Vector3 Normal;
    }

    private static readonly Stopwatch stopwatch = Stopwatch.StartNew();
    private readonly Vertex[] points;
    private readonly ushort[] indices;
    private readonly float minY;
    private readonly float maxY;
    private uint vertexShader;
    private uint fragmentShader;
    private uint shaderProgram;
    private uint vertexBufferObject;
    private uint indexBufferObject;
    private uint vertexArrayObject;

    static TeapotControl3() => AffectsRender<TeapotControl3>(YawProperty, PitchProperty, RollProperty, DiscoProperty);

    public TeapotControl3()
    {
        var name = typeof(TeapotControl3).Assembly.GetManifestResourceNames().First(x => x.Contains("teapot.bin"));
        var stream = typeof(TeapotControl3).Assembly.GetManifestResourceStream(name);
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
        System.Buffer.BlockCopy(buffer, 0, readPoints, 0, buffer.Length);

        buffer = new byte[sr.ReadInt32()];
        _ = sr.Read(buffer, 0, buffer.Length);
        indices = new ushort[buffer.Length / 2];
        System.Buffer.BlockCopy(buffer, 0, indices, 0, buffer.Length);
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

    public static readonly DirectProperty<TeapotControl3, float> YawProperty =
        AvaloniaProperty.RegisterDirect<TeapotControl3, float>(nameof(Yaw), o => o.Yaw, (o, v) => o.Yaw = v);

    private float yaw;
    public float Yaw
    {
        get => yaw;
        set => SetAndRaise(YawProperty, ref yaw, value);
    }

    public static readonly DirectProperty<TeapotControl3, float> PitchProperty =
        AvaloniaProperty.RegisterDirect<TeapotControl3, float>(nameof(Pitch), o => o.Pitch, (o, v) => o.Pitch = v);

    private float pitch;
    public float Pitch
    {
        get => pitch;
        set => SetAndRaise(PitchProperty, ref pitch, value);
    }

    public static readonly DirectProperty<TeapotControl3, float> RollProperty =
        AvaloniaProperty.RegisterDirect<TeapotControl3, float>(nameof(Roll), o => o.Roll, (o, v) => o.Roll = v);

    private float roll;
    public float Roll
    {
        get => roll;
        set => SetAndRaise(RollProperty, ref roll, value);
    }

    public static readonly DirectProperty<TeapotControl3, float> DiscoProperty =
        AvaloniaProperty.RegisterDirect<TeapotControl3, float>(nameof(Disco), o => o.Disco, (o, v) => o.Disco = v);

    private float disco;
    public float Disco
    {
        get => disco;
        set => SetAndRaise(DiscoProperty, ref disco, value);
    }

    public static readonly DirectProperty<TeapotControl3, string> InfoProperty = AvaloniaProperty.RegisterDirect<TeapotControl3, string>(
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

    protected override unsafe void OnRender(GL gl, uint fb)
    {
        gl.ClearColor(0f, 0f, 0f, 0f);
        gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        gl.Enable(EnableCap.DepthTest);
        gl.Viewport(0, 0, (int)Bounds.Width, (int)Bounds.Height);
        gl.BindBuffer(BufferTargetArb.ArrayBuffer, vertexBufferObject);
        gl.BindBuffer(BufferTargetArb.ElementArrayBuffer, indexBufferObject);
        gl.BindVertexArray(vertexArrayObject);
        gl.UseProgram(shaderProgram);
        CheckGLError();

        var projection = Matrix4x4.CreatePerspectiveFieldOfView(
            (float)(Math.PI / 4), (float)(Bounds.Width / Bounds.Height), 0.01f, 1000f);

        var view = Matrix4x4.CreateLookAt(new Vector3(25, 25, 25), new Vector3(), new Vector3(0, -1, 0));
        var model = Matrix4x4.CreateFromYawPitchRoll(yaw, pitch, roll);
        var modelLoc = gl.GetUniformLocation(shaderProgram, "uModel");
        var viewLoc = gl.GetUniformLocation(shaderProgram, "uView");
        var projectionLoc = gl.GetUniformLocation(shaderProgram, "uProjection");
        var maxYLoc = gl.GetUniformLocation(shaderProgram, "uMaxY");
        var minYLoc = gl.GetUniformLocation(shaderProgram, "uMinY");
        var timeLoc = gl.GetUniformLocation(shaderProgram, "uTime");
        var discoLoc = gl.GetUniformLocation(shaderProgram, "uDisco");

        gl.UniformMatrix4(modelLoc, false, model);
        gl.UniformMatrix4(viewLoc, false, view);
        gl.UniformMatrix4(projectionLoc, false, projection);
        gl.Uniform1(maxYLoc, maxY);
        gl.Uniform1(minYLoc, minY);
        gl.Uniform1(timeLoc, (float)stopwatch.Elapsed.TotalSeconds);
        gl.Uniform1(discoLoc, disco);
        CheckGLError();

        gl.DrawElements(PrimitiveType.Triangles, indices.Length, DrawElementsType.UnsignedShort, 0);
        CheckGLError();

        if (disco > 0.01)
            Dispatcher.UIThread.Post(InvalidateVisual, DispatcherPriority.Background);
    }

    protected override unsafe void OnInitializeRendering(GL gl, uint fb)
    {
        CheckGLError();

        static string format(GlVersion glv) => $"{glv.Type} v{glv.Major}.{glv.Minor}";
        Info = $"{GetType().Name}, FB: {fb}\r\nRenderer: {gl.GetString(StringName.Renderer)}\r\nVersion: {gl.GetString(StringName.Version)}\r\nGL: {format(GlVersion)}";

        // Load the source of the vertex shader and compile it.
        vertexShader = gl.CreateShader(ShaderType.VertexShader);
        var vertexLog = gl.CompileShaderAndGetError(vertexShader, VertexShaderSource);
        LogShaderCompilationResult("Vertex Shader Compilation", vertexLog);

        // Load the source of the fragment shader and compile it.
        fragmentShader = gl.CreateShader(ShaderType.FragmentShader);
        var fragmentLog = gl.CompileShaderAndGetError(fragmentShader, FragmentShaderSource);
        LogShaderCompilationResult("Fragment Shader Compilation", fragmentLog);

        shaderProgram = gl.CreateProgram();
        gl.AttachShader(shaderProgram, vertexShader);
        gl.AttachShader(shaderProgram, fragmentShader);

        const int positionLocation = 0;
        gl.BindAttribLocation(shaderProgram, positionLocation, "aPos");
        const int normalLocation = 1;
        gl.BindAttribLocation(shaderProgram, normalLocation, "aNormal");
        var programLog = gl.LinkProgramAndGetError(shaderProgram);
        LogShaderCompilationResult("Shader Program Compilation", programLog);
        CheckGLError();

        // Create the vertex buffer object (VBO) for the vertex data, then bind the VBO and copy the vertex data into it.
        vertexBufferObject = gl.GenBuffer();
        gl.BindBuffer(BufferTargetArb.ArrayBuffer, vertexBufferObject);
        CheckGLError();

        var vertexSize = Marshal.SizeOf<Vertex>();
        fixed (void* pdata = points) gl.BufferData(
            BufferTargetArb.ArrayBuffer, points.Length * vertexSize, pdata, BufferUsageArb.StaticDraw);
        CheckGLError();

        indexBufferObject = gl.GenBuffer();
        gl.BindBuffer(BufferTargetArb.ElementArrayBuffer, indexBufferObject);
        CheckGLError();

        fixed (void* pdata = indices) gl.BufferData(
            BufferTargetArb.ElementArrayBuffer, indices.Length * sizeof(ushort), pdata, BufferUsageArb.StaticDraw);
        CheckGLError();

        vertexArrayObject = gl.GenVertexArray();
        gl.BindVertexArray(vertexArrayObject);
        CheckGLError();

        gl.VertexAttribPointer(positionLocation, 3, VertexAttribPointerType.Float, false, vertexSize, 0);
        gl.VertexAttribPointer(normalLocation, 3, VertexAttribPointerType.Float, false, vertexSize, 12);
        gl.EnableVertexAttribArray(positionLocation);
        gl.EnableVertexAttribArray(normalLocation);
        CheckGLError();
    }

    protected override void OnCleanupRendering(GL gl, uint fb)
    {
        // Unbind everything
        gl.BindBuffer(BufferTargetArb.ArrayBuffer, 0);
        gl.BindBuffer(BufferTargetArb.ElementArrayBuffer, 0);
        gl.BindVertexArray(0);
        gl.UseProgram(0);

        // Delete all resources.
        gl.DeleteBuffers(vertexBufferObject, indexBufferObject);
        gl.DeleteVertexArray(vertexArrayObject);
        gl.DeleteProgram(shaderProgram);
        gl.DeleteShader(fragmentShader);
        gl.DeleteShader(vertexShader);
    }

    private static readonly bool forceShaderVersion = true;

    private string GetShader(bool fragment, string shader)
    {
        int version;
        if (forceShaderVersion && RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            // With my configuration on Windows (AMD Radeon (supports GL up to 4.5), GLSL 150 is what prevents the
            // "Explicit version number 120 not supported by GL3 forward compatible context" warning
            version = 150;
        }
        else
        {
            // Original logic
            if (GlVersion.Type == GlProfileType.OpenGL)
                version = RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ? 150 : 120;
            else
                version = 100;
        }

        var data = "#version " + version + "\n";
        if (GlVersion.Type == GlProfileType.OpenGLES)
            data += "precision mediump float;\n";
        //var data = "";

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

    private void LogShaderCompilationResult(string header, string? compilationLog)
    {
        // Null string = failure
        if (compilationLog == null)
        {
            LogGLError($"{header}: Failed");
            return;
        }

        // Empty string = success
        if (compilationLog == "")
        {
            LogGLInfo($"{header}: Succeeded");
            return;
        }

        // Remove eventual trailing CR/LF/CRLF
        var log = compilationLog;
        while (log.EndsWith("\r") || log.EndsWith("\n"))
            log = log[0..^1];

        // Otherwise, let's analyze the log
        var lowered = log.ToLowerInvariant();
        var message = $"{header}: {log}";

        if (lowered.StartsWith("error"))
            LogGLError(message);
        else if (lowered.StartsWith("warn"))
            LogGLWarning(message);
        else
            LogGLInfo(message);
    }
}
