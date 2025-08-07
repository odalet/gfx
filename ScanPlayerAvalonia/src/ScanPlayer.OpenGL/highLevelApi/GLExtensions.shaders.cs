using System.Numerics;

namespace ScanPlayer.OpenGL;

unsafe partial class GLExtensions
{
    public static uint CreateProgram(this GL gl) => gl.Api.glCreateProgram();
    public static uint CreateShader(this GL gl, ShaderType type) => gl.Api.glCreateShader((uint)type);

    public static void ShaderSource(this GL gl, uint shader, string source)
    {
        var length = source.Length;
        var strings = new string[] { source };
        var ptr = (byte**)Marshalling.StringArrayToPtr(strings);
        gl.Api.glShaderSource(shader, 1, ptr, &length);
        Marshalling.CopyPtrToStringArray((nint)ptr, strings);
        _ = Marshalling.Free((nint)ptr);
    }

    public static int GetShader(this GL gl, uint shader, ShaderParameterName parameterName)
    {
        var result = 0;
        gl.Api.glGetShaderiv(shader, (uint)parameterName, &result);
        return result;
    }

    public static string GetShaderInfoLog(this GL gl, uint shader)
    {
        var logLength = GetShader(gl, shader, ShaderParameterName.InfoLogLength);
        if (logLength <= 0) return string.Empty;

        var bufSize = logLength * 2; // To be on the safe side
        var length = 0;
        var ptr = (byte*)Marshalling.AllocateString(bufSize);
        try
        {
            gl.Api.glGetShaderInfoLog(shader, bufSize, &length, ptr);
            return Marshalling.PtrToString((nint)ptr) ?? "";
        }
        finally { Marshalling.FreeString((nint)ptr); }
    }

    public static void CompileShader(this GL gl, uint shader) => gl.Api.glCompileShader(shader);

    public static unsafe string? CompileShaderAndGetError(this GL gl, uint shader, string source)
    {
        gl.ShaderSource(shader, source);
        gl.CompileShader(shader);

        var compiled = gl.GetShader(shader, ShaderParameterName.CompileStatus);
        return compiled != (int)Boolean.True ? null : gl.GetShaderInfoLog(shader);
    }

    public static void AttachShader(this GL gl, uint program, uint shader) => gl.Api.glAttachShader(program, shader);
    public static void DetachShader(this GL gl, uint program, uint shader) => gl.Api.glDetachShader(program, shader);

    public static void LinkProgram(this GL gl, uint program) => gl.Api.glLinkProgram(program);

    public static string? LinkProgramAndGetError(this GL gl, uint program)
    {
        gl.LinkProgram(program);

        var compiled = gl.GetProgram(program, ProgramPropertyArb.LinkStatus);
        return compiled != (int)Boolean.True ? null : gl.GetProgramInfoLog(program);
    }

    public static void UseProgram(this GL gl, uint program) => gl.Api.glUseProgram(program);

    public static int GetProgram(this GL gl, uint program, ProgramPropertyArb parameterName)
    {
        var result = 0;
        gl.Api.glGetProgramiv(program, (uint)parameterName, &result);
        return result;
    }

    public static string GetProgramInfoLog(this GL gl, uint program)
    {
        var logLength = GetProgram(gl, program, ProgramPropertyArb.InfoLogLength);
        if (logLength <= 0) return string.Empty;

        var bufSize = logLength * 2; // To be on the safe side
        var length = 0;
        var ptr = (byte*)Marshalling.AllocateString(bufSize);
        try
        {
            gl.Api.glGetProgramInfoLog(program, bufSize, &length, ptr);
            return Marshalling.PtrToString((nint)ptr) ?? "";
        }
        finally { Marshalling.FreeString((nint)ptr); }
    }

    public static void DeleteProgram(this GL gl, uint program) => gl.Api.glDeleteProgram(program);
    public static void DeleteShader(this GL gl, uint shader) => gl.Api.glDeleteShader(shader);

    public static void Uniform1(this GL gl, int location, int value) => gl.Api.glUniform1i(location, value);
    public static void Uniform1(this GL gl, int location, float value) => gl.Api.glUniform1f(location, value);
    public static void Uniform1(this GL gl, int location, double value) => gl.Api.glUniform1d(location, value);

    public static void Uniform2(this GL gl, int location, int v0, int v1) => gl.Api.glUniform2i(location, v0, v1);
    public static void Uniform2(this GL gl, int location, float v0, float v1) => gl.Api.glUniform2f(location, v0, v1);
    public static void Uniform2(this GL gl, int location, double v0, double v1) => gl.Api.glUniform2d(location, v0, v1);

    public static void Uniform3(this GL gl, int location, int v0, int v1, int v2) => gl.Api.glUniform3i(location, v0, v1, v2);
    public static void Uniform3(this GL gl, int location, float v0, float v1, float v2) => gl.Api.glUniform3f(location, v0, v1, v2);
    public static void Uniform3(this GL gl, int location, double v0, double v1, double v2) => gl.Api.glUniform3d(location, v0, v1, v2);

    public static void Uniform4(this GL gl, int location, int v0, int v1, int v2, int v3) => gl.Api.glUniform4i(location, v0, v1, v2, v3);
    public static void Uniform4(this GL gl, int location, float v0, float v1, float v2, float v3) => gl.Api.glUniform4f(location, v0, v1, v2, v3);
    public static void Uniform4(this GL gl, int location, double v0, double v1, double v2, double v3) => gl.Api.glUniform4d(location, v0, v1, v2, v3);

    public static void UniformMatrix4(this GL gl, int location, float[][] value) => UniformMatrix4(gl, location, false, value);
    public static void UniformMatrix4(this GL gl, int location, bool transpose, float[][] value)
    {
        fixed (float* pointer = &value[0][0])
            UniformMatrix4(gl, location, transpose, pointer);
    }

    public static void UniformMatrix4(this GL gl, int location, float[] value) => UniformMatrix4(gl, location, false, value);
    public static void UniformMatrix4(this GL gl, int location, bool transpose, float[] value)
    {
        fixed (float* pointer = &value[0])
            UniformMatrix4(gl, location, transpose, pointer);
    }

    public static void UniformMatrix4(this GL gl, int location, Matrix4x4 value) => UniformMatrix4(gl, location, false, value);
    public static void UniformMatrix4(this GL gl, int location, bool transpose, Matrix4x4 value) => 
        UniformMatrix4(gl, location, transpose, (float*)&value);

    public static void UniformMatrix4(this GL gl, int location, float* value) => UniformMatrix4(gl, location, false, value);
    public static void UniformMatrix4(this GL gl, int location, bool transpose, float* value) =>
        gl.Api.glUniformMatrix4fv(location, 1, (byte)(transpose ? 1 : 0), value);

    public static string GetActiveUniform(this GL gl, uint program, int index, out int size, out UniformType type)
    {
        var maxLength = GetProgram(gl, program, ProgramPropertyArb.ActiveUniformMaxLength);
        var bufSize = maxLength == 0 ? 1 : maxLength;

        var typeValue = 0u;
        var nameLength = 0;
        fixed (int* sizePtr = &size)
        {
            var namePtr = (byte*)Marshalling.AllocateString(bufSize);
            gl.Api.glGetActiveUniform(program, (uint)index, bufSize, &nameLength, sizePtr, &typeValue, namePtr);

            type = (UniformType)typeValue;

            var name = Marshalling.PtrToString((nint)namePtr) ?? "";
            Marshalling.FreeString((nint)namePtr);

            return name[..nameLength];
        }
    }

    public static int GetUniformLocation(this GL gl, uint program, string name)
    {
        var namePtr = (byte*)Marshalling.StringToPtr(name);
        try
        {
            return gl.Api.glGetUniformLocation(program, namePtr);
        }
        finally { Marshalling.FreeString((nint)namePtr); }
    }

    public static void BindAttribLocation(this GL gl, uint program, uint index, string name)
    {
        var namePtr = (byte*)Marshalling.StringToPtr(name);
        try
        {
            gl.Api.glBindAttribLocation(program, index, namePtr);
        }
        finally { Marshalling.FreeString((nint)namePtr); }
    }

    public static int GetAttribLocation(this GL gl, uint program, string name)
    {
        var namePtr = (byte*)Marshalling.StringToPtr(name);
        try
        {
            return gl.Api.glGetAttribLocation(program, namePtr);
        }
        finally { Marshalling.FreeString((nint)namePtr); }
    }
}
