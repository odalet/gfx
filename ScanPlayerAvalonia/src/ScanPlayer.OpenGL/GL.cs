using System;

namespace ScanPlayer.OpenGL;

public unsafe partial interface IGLApi { }

public sealed partial class GL
{
    private sealed unsafe partial class GLApi : IGLApi
    {
        private sealed unsafe partial class VTable
        {
            private readonly Func<string, nint> getProcAddress;

            public VTable(Func<string, nint> getProcAddressFunc) => getProcAddress = getProcAddressFunc;

            private nint GetProcAddress(string name) => getProcAddress(name);
        }

        private readonly VTable vtable;

        public GLApi(Func<string, nint> getProcAddressFunc) => vtable = new VTable(getProcAddressFunc);
    }

    public GL(Func<string, nint> getProcAddressFunc) => Api = new GLApi(getProcAddressFunc);

    public IGLApi Api { get; }
}

