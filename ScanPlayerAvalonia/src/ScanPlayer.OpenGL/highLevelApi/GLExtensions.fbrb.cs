namespace ScanPlayer.OpenGL;

// Framebuffers and Renderbuffers
unsafe partial class GLExtensions
{
    // Framebuffer

    public static void BindFramebuffer(this GL gl, FramebufferTarget target, uint framebuffer) => gl.Api.glBindFramebuffer((uint)target, framebuffer);

    public static void FramebufferRenderbuffer(this GL gl, FramebufferTarget target, FramebufferAttachment attachment, RenderbufferTarget renderbufferTarget, uint renderbuffer) =>
        gl.Api.glFramebufferRenderbuffer((uint)target, (uint)attachment, (uint)renderbufferTarget, renderbuffer);

    public static uint GenFramebuffer(this GL gl)
    {
        uint framebuffer;
        gl.Api.glGenFramebuffers(1, &framebuffer);
        return framebuffer;
    }

    public static void DeleteFramebuffer(this GL gl, uint framebuffer)
    {
        var fb = framebuffer;
        gl.Api.glDeleteVertexArrays(1, &fb);
    }

    public static void DeleteFramebuffers(this GL gl, uint[] framebuffers)
    {
        var temp = framebuffers;
        fixed (uint* ptr = &temp[0])
            gl.Api.glDeleteVertexArrays(framebuffers.Length, ptr);
    }

    public static FramebufferStatus CheckFramebufferStatus(this GL gl, FramebufferTarget target) => 
        (FramebufferStatus)gl.Api.glCheckFramebufferStatus((uint)target);

    // Renderbuffer

    public static void BindRenderbuffer(this GL gl, RenderbufferTarget target, uint Renderbuffer) => gl.Api.glBindRenderbuffer((uint)target, Renderbuffer);

    public static void RenderbufferStorage(this GL gl, RenderbufferTarget target, InternalFormat format, int width, int height) =>
        gl.Api.glRenderbufferStorage((uint)target, (uint)format, width, height);

    public static uint GenRenderbuffer(this GL gl)
    {
        uint Renderbuffer;
        gl.Api.glGenRenderbuffers(1, &Renderbuffer);
        return Renderbuffer;
    }

    public static void DeleteRenderbuffer(this GL gl, uint Renderbuffer)
    {
        var fb = Renderbuffer;
        gl.Api.glDeleteVertexArrays(1, &fb);
    }

    public static void DeleteRenderbuffers(this GL gl, uint[] Renderbuffers)
    {
        var temp = Renderbuffers;
        fixed (uint* ptr = &temp[0])
            gl.Api.glDeleteVertexArrays(Renderbuffers.Length, ptr);
    }
}