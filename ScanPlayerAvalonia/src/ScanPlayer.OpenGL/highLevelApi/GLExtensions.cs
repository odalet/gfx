namespace ScanPlayer.OpenGL;

public static unsafe partial class GLExtensions
{
    // Misc

    public static void Clear(this GL gl, ClearBufferMask mask) => gl.Api.glClear((uint)mask);
    public static void ClearColor(this GL gl, float red, float green, float blue, float alpha) => gl.Api.glClearColor(red, green, blue, alpha);
    public static void ClearColor(this GL gl, byte red, byte green, byte blue, byte alpha) => gl.Api.glClearColor(red / 255f, green / 255f, blue / 255f, alpha / 255f);

    public static bool IsEnabled(this GL gl, EnableCap cap) => gl.Api.glIsEnabled((uint)cap) != 0;
    public static void Enable(this GL gl, EnableCap cap) => gl.Api.glEnable((uint)cap);
    public static void Disable(this GL gl, EnableCap cap) => gl.Api.glDisable((uint)cap);

    public static int GetInteger(this GL gl, GLEnum pname)
    {
        var result = 0;
        gl.Api.glGetIntegerv((uint)pname, &result);
        return result;
    }

    public static int[] GetIntegers(this GL gl, GLEnum pname, int count)
    {
        var result = new int[count];
        fixed (int* pointer = &result[0])
            gl.Api.glGetIntegerv((uint)pname, pointer);
        return result;
    }

    public static unsafe string? GetString(this GL gl, StringName name)
    {
        var ptr = gl.Api.glGetString((uint)name);
        return Marshalling.PtrToString((nint)ptr);
    }

    public static unsafe string? GetString(this GL gl, StringName name, int index)
    {
        var ptr = gl.Api.glGetStringi((uint)name, (uint)index);
        return Marshalling.PtrToString((nint)ptr);
    }

    public static ErrorCode GetError(this GL gl) => (ErrorCode)gl.Api.glGetError();

    public static void BlendEquation(this GL gl, BlendEquationModeExt mode) => gl.Api.glBlendEquation((uint)mode);
    public static void BlendEquationSeparate(this GL gl, BlendEquationModeExt modeRgb, BlendEquationModeExt modeAlpha) =>
        gl.Api.glBlendEquationSeparate((uint)modeRgb, (uint)modeAlpha);

    public static void BlendFunc(this GL gl, BlendingFactor src, BlendingFactor dest) => gl.Api.glBlendFunc((uint)src, (uint)dest);
    public static void BlendFuncSeparate(this GL gl, BlendingFactor srcFactorRgb, BlendingFactor destFactorRgb, BlendingFactor srcFactorAlpha, BlendingFactor destFactorAlpha) =>
        gl.Api.glBlendFuncSeparate((uint)srcFactorRgb, (uint)destFactorRgb, (uint)srcFactorAlpha, (uint)destFactorAlpha);

    public static void PolygonMode(this GL gl, MaterialFace face, PolygonMode mode) => gl.Api.glPolygonMode((uint)face, (uint)mode);

    public static void Viewport(this GL gl, int x, int y, int width, int height) => gl.Api.glViewport(x, y, width, height);

    public static void Scissor(this GL gl, int x, int y, int width, int height) => gl.Api.glScissor(x, y, width, height);

    public static void ClipControl(this GL gl, ClipControlOrigin origin, ClipControlDepth depth) => gl.Api.glClipControl((uint)origin, (uint)depth);

    public static void DrawElements(this GL gl, PrimitiveType mode, int count, DrawElementsType type, nint indices) => DrawElements(gl, mode, count, type, (void*)indices);
    public static void DrawElements(this GL gl, PrimitiveType mode, int count, DrawElementsType type, void* indices) =>
        gl.Api.glDrawElements((uint)mode, count, (uint)type, indices);

    // Textures

    public static uint GenTexture(this GL gl)
    {
        uint texture;
        gl.Api.glGenTextures(1, &texture);
        return texture;
    }

    public static void ActiveTexture(this GL gl, TextureUnit texture) => ActiveTexture(gl, (uint)texture);
    public static void ActiveTexture(this GL gl, uint texture) => gl.Api.glActiveTexture(texture);
    public static void BindTexture(this GL gl, TextureTarget target, uint texture) => gl.Api.glBindTexture((uint)target, texture);

    public static void TexImage2D(this GL gl, TextureTarget target, int level, InternalFormat internalFormat, int width, int height, int border, PixelFormat format, PixelType type, byte[] data)
    {
        fixed (void* pixels = &data[0])
            gl.Api.glTexImage2D((uint)target, level, (int)internalFormat, width, height, border, (uint)format, (uint)type, pixels);
    }

    public static void TexParameter(this GL gl, TextureTarget target, TextureParameterName parameterName, GLEnum parameterValue) =>
        TexParameter(gl, target, parameterName, (int)parameterValue);
    public static void TexParameter(this GL gl, TextureTarget target, TextureParameterName parameterName, TextureCompareMode parameterValue) =>
        TexParameter(gl, target, parameterName, (int)parameterValue);
    public static void TexParameter(this GL gl, TextureTarget target, TextureParameterName parameterName, TextureMagFilter parameterValue) =>
        TexParameter(gl, target, parameterName, (int)parameterValue);
    public static void TexParameter(this GL gl, TextureTarget target, TextureParameterName parameterName, TextureMinFilter parameterValue) =>
        TexParameter(gl, target, parameterName, (int)parameterValue);
    public static void TexParameter(this GL gl, TextureTarget target, TextureParameterName parameterName, TextureSwizzle parameterValue) =>
        TexParameter(gl, target, parameterName, (int)parameterValue);
    public static void TexParameter(this GL gl, TextureTarget target, TextureParameterName parameterName, TextureWrapMode parameterValue) =>
        TexParameter(gl, target, parameterName, (int)parameterValue);

    public static void TexParameter(this GL gl, TextureTarget target, TextureParameterName parameterName, int parameterValue) =>
        gl.Api.glTexParameteri((uint)target, (uint)parameterName, parameterValue);

    public static void DeleteTexture(this GL gl, uint texture)
    {
        var textures = texture;
        gl.Api.glDeleteTextures(1, &textures);
    }

    public static void DeleteTextures(this GL gl, uint[] textures)
    {
        var temp = textures;
        fixed (uint* ptr = &temp[0])
            gl.Api.glDeleteTextures(textures.Length, ptr);
    }

    // Samplers
    public static void BindSampler(this GL gl, TextureUnit unit, uint sampler) => BindSampler(gl, (uint)unit, sampler);
    public static void BindSampler(this GL gl, uint unit, uint sampler) => gl.Api.glBindSampler(unit, sampler);

    // VAO, VBO ...

    public static uint GenVertexArray(this GL gl) // Generates only 1 VAO
    {
        uint array;
        gl.Api.glGenVertexArrays(1, &array);
        return array;
    }

    public static void DeleteVertexArray(this GL gl, uint array)
    {
        var vao = array;
        gl.Api.glDeleteVertexArrays(1, &vao);
    }

    public static void DeleteVertexArrays(this GL gl, uint[] arrays)
    {
        var temp = arrays;
        fixed (uint* ptr = &temp[0])
            gl.Api.glDeleteVertexArrays(arrays.Length, ptr);
    }

    public static void BindVertexArray(this GL gl, uint array) => gl.Api.glBindVertexArray(array);
    public static void EnableVertexAttribArray(this GL gl, int index) => gl.Api.glEnableVertexAttribArray((uint)index);

    public static void VertexAttribPointer(this GL gl, int index, int size, VertexAttribPointerType type, bool normalized, int stride, nint pointer) =>
        VertexAttribPointer(gl, index, size, type, normalized, stride, (void*)pointer);
    public static void VertexAttribPointer(this GL gl, int index, int size, VertexAttribPointerType type, bool normalized, int stride, void* pointer) =>
        gl.Api.glVertexAttribPointer((uint)index, size, (uint)type, (byte)(normalized ? 1 : 0), stride, pointer);

    public static uint GenBuffer(this GL gl) // Generates only 1 Buffer Object
    {
        uint buffer;
        gl.Api.glGenBuffers(1, &buffer);
        return buffer;
    }

    public static void BindBuffer(this GL gl, BufferTargetArb target, uint buffer) => gl.Api.glBindBuffer((uint)target, buffer);

    public static void DeleteBuffer(this GL gl, uint buffer)
    {
        var buffers = buffer;
        gl.Api.glDeleteBuffers(1, &buffers);
    }

    public static void DeleteBuffers(this GL gl, params uint[] buffers)
    {
        var temp = buffers;
        fixed (uint* ptr = &temp[0])
            gl.Api.glDeleteBuffers(buffers.Length, ptr);
    }

    public static void BufferData(this GL gl, BufferTargetArb target, int size, float[] data, BufferUsageArb usage)
    {
        var temp = data;
        fixed (float* ptr = &temp[0])
            BufferData(gl, target, size, ptr, usage);
    }

    public static void BufferData(this GL gl, BufferTargetArb target, int size, uint[] data, BufferUsageArb usage)
    {
        var temp = data;
        fixed (uint* ptr = &temp[0])
            BufferData(gl, target, size, ptr, usage);
    }

    //// DOES NOT WORK!
    //public static void BufferData<T>(this GL gl, BufferTargetArb target, int size, T[] data, BufferUsageArb usage) where T : struct
    //{
    //    var reference = __makeref(data); // https://stackoverflow.com/questions/17156179/pointers-of-generic-type/37016731
    //    BufferData(gl, target, size, *(IntPtr*)&reference, usage);
    //}

    public static void BufferData(this GL gl, BufferTargetArb target, int size, nint data, BufferUsageArb usage) => BufferData(gl, target, size, (void*)data, usage);
    public static void BufferData(this GL gl, BufferTargetArb target, int size, void* data, BufferUsageArb usage) => gl.Api.glBufferData((uint)target, size, data, (uint)usage);
}
