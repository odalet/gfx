using System.Drawing;
using System.Numerics;

namespace ScanPlayer.OpenGL;

unsafe partial class GLExtensions
{
    public static void Begin(this GL gl, PrimitiveType mode) => gl.Api.glBegin((uint)mode);
    public static void End(this GL gl) => gl.Api.glEnd();

    public static void PushMatrix(this GL gl) => gl.Api.glPushMatrix();
    public static void PopMatrix(this GL gl) => gl.Api.glPopMatrix();
    public static void MatrixMode(this GL gl, MatrixMode mode) => gl.Api.glMatrixMode((uint)mode);

    public static void LoadMatrix(this GL gl, Matrix4x4 value) => gl.LoadMatrix((float*)&value);
    public static void LoadMatrix(this GL gl, float* value) => gl.Api.glLoadMatrixf(value);

    public static void PushAttrib(this GL gl, AttribMask mask) => gl.Api.glPushAttrib((uint)mask);
    public static void PopAttrib(this GL gl) => gl.Api.glPopAttrib();

    public static void Rotate(this GL gl, int angle, int x, int y, int z) => gl.Api.glRotatex(angle, x, y, z);
    public static void Rotate(this GL gl, float angle, float x, float y, float z) => gl.Api.glRotatef(angle, x, y, z);
    public static void Rotate(this GL gl, double angle, double x, double y, double z) => gl.Api.glRotated(angle, x, y, z);
    
    public static void Scale(this GL gl, int x, int y, int z) => gl.Api.glScalex(x, y, z);
    public static void Scale(this GL gl, float x, float y, float z) => gl.Api.glScalef(x, y, z);
    public static void Scale(this GL gl, double x, double y, double z) => gl.Api.glScaled(x, y, z);

    public static void Translate(this GL gl, int x, int y, int z) => gl.Api.glTranslatex(x, y, z);
    public static void Translate(this GL gl, float x, float y, float z) => gl.Api.glTranslatef(x, y, z);
    public static void Translate(this GL gl, double x, double y, double z) => gl.Api.glTranslated(x, y, z);

    public static void Color(this GL gl, double red, double green, double blue) => gl.Api.glColor3d(red, green, blue);
    public static void Color(this GL gl, float red, float green, float blue) => gl.Api.glColor3f(red, green, blue);
    public static void Color(this GL gl, byte red, byte green, byte blue) => gl.Api.glColor3ub(red, green, blue);
    public static void Color(this GL gl, double red, double green, double blue, double alpha) => gl.Api.glColor4d(red, green, blue, alpha);
    public static void Color(this GL gl, float red, float green, float blue, float alpha) => gl.Api.glColor4f(red, green, blue, alpha);
    public static void Color(this GL gl, byte red, byte green, byte blue, byte alpha) => gl.Api.glColor4ub(red, green, blue, alpha);

    public static void LineWidth(this GL gl, int width) => gl.Api.glLineWidthx(width);
    public static void LineWidth(this GL gl, float width) => gl.Api.glLineWidth(width);

    public static void LineStipple(this GL gl, int factor, ushort pattern) => gl.Api.glLineStipple(factor, pattern);

    public static void Vertex(this GL gl, int x, int y) => gl.Api.glVertex2i(x, y);
    public static void Vertex(this GL gl, float x, float y) => gl.Api.glVertex2f(x, y);
    public static void Vertex(this GL gl, double x, double y) => gl.Api.glVertex2d(x, y);
    public static void Vertex(this GL gl, int x, int y, int z) => gl.Api.glVertex3i(x, y, z);
    public static void Vertex(this GL gl, float x, float y, float z) => gl.Api.glVertex3f(x, y, z);
    public static void Vertex(this GL gl, double x, double y, double z) => gl.Api.glVertex3d(x, y, z);
    public static void Vertex(this GL gl, int x, int y, int z, int w) => gl.Api.glVertex4i(x, y, z, w);
    public static void Vertex(this GL gl, float x, float y, float z, float w) => gl.Api.glVertex4f(x, y, z, w);
    public static void Vertex(this GL gl, double x, double y, double z, double w) => gl.Api.glVertex4d(x, y, z, w);
}
