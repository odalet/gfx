using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace ScanPlayer.OpenGL;

public readonly struct UniformFieldInfo : IEquatable<UniformFieldInfo>
{
    public UniformFieldInfo(int location, string name, int size, UniformType type)
    {
        Location = location;
        Name = name ?? "";
        Size = size;
        Type = type;
    }

    public int Location { get; }
    public string Name { get; }
    public int Size { get; }
    public UniformType Type { get; }

    public override bool Equals(object? obj) => obj is UniformFieldInfo info && Equals(info);
    public bool Equals(UniformFieldInfo other) => Location == other.Location && Name == other.Name && Size == other.Size && Type == other.Type;
    public override int GetHashCode() => HashCode.Combine(Location, Name, Size, Type);

    public static bool operator ==(UniformFieldInfo left, UniformFieldInfo right) => left.Equals(right);
    public static bool operator !=(UniformFieldInfo left, UniformFieldInfo right) => !(left == right);
}

public class Shader : IDisposable
{
    private bool disposed;
    private readonly Dictionary<string, int> UniformToLocation = new();

    public Shader(GL gl, string name, string vertexShader, string fragmentShader)
    {
        GL = gl ?? throw new ArgumentNullException(nameof(gl));
        Name = name;
        Program = CreateProgram(GL, Name, new[]
        {
            (ShaderType.VertexShader, vertexShader),
            (ShaderType.FragmentShader, fragmentShader),
        });
    }

    protected GL GL { get; }
    public string Name { get; }
    public uint Program { get; }

    public void UseShader() => GL.UseProgram(Program);

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposed) return;

        GL.DeleteProgram(Program);
        disposed = true;
    }

    ~Shader() => Dispose(false);

    public UniformFieldInfo[] GetUniforms()
    {
        var uniformCount = GL.GetProgram(Program, ProgramPropertyArb.ActiveUniforms);
        var uniforms = new UniformFieldInfo[uniformCount];

        for (var i = 0; i < uniformCount; i++)
        {
            var name = GL.GetActiveUniform(Program, i, out var size, out var type);
            uniforms[i] = new UniformFieldInfo(
                GetUniformLocation(name), name, size, type);
        }

        return uniforms;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int GetUniformLocation(string uniform)
    {
        if (!UniformToLocation.TryGetValue(uniform, out var location))
        {
            location = GL.GetUniformLocation(Program, uniform);
            UniformToLocation.Add(uniform, location);

            if (location == -1)
                Console.WriteLine($"The uniform '{uniform}' does not exist in the shader '{Name}'!");
        }

        return location;
    }

    private static uint CreateProgram(GL gl, string name, params (ShaderType Type, string source)[] code)
    {
        var shaders = new uint[code.Length];
        for (var i = 0; i < code.Length; i++)
            shaders[i] = CompileShader(gl, name, code[i].Type, code[i].source);

        var program = gl.CreateProgram();

        foreach (var shader in shaders)
            gl.AttachShader(program, shader);

        gl.LinkProgram(program);
        var success = gl.GetProgram(program, ProgramPropertyArb.LinkStatus);
        if (success == (int)GLEnum.False)
        {
            var info = gl.GetProgramInfoLog(program);
            Console.WriteLine($"LinkProgram for '{name}' failed: {info}");
        }

        foreach (var shader in shaders)
        {
            gl.DetachShader(program, shader);
            gl.DeleteShader(shader);
        }

        return program;
    }

    private static uint CompileShader(GL gl, string name, ShaderType type, string source)
    {
        var shader = gl.CreateShader(type);
        gl.ShaderSource(shader, source);
        gl.CompileShader(shader);
        var success = gl.GetShader(shader, ShaderParameterName.CompileStatus);
        if (success == (int)GLEnum.False)
        {
            var info = gl.GetShaderInfoLog(shader);
            Console.WriteLine($"CompileShader for {type} '{name}' failed: {info}");
        }

        return shader;
    }
}
