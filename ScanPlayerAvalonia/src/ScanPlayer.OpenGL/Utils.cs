////using System;
////using System.Diagnostics;

////namespace ScanPlayer.OpenGL;

////public static class Utils
////{
////    [Conditional("DEBUG")]
////    public static void CheckGLError(this GL gl, string where)
////    {
////        var error = gl.GetError();
////        if (error != ErrorCode.NoError)
////            Console.WriteLine($"OpenGL Error in {where}: {error} ({(int)error})");
////    }
////}

