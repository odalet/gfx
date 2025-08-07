using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace ScanPlayer.OpenGL;

// Retrieved from Silk.NET
public sealed class GlobalMemory : IDisposable
{
    private interface IGlobalMemory
    {
        nint Handle { get; }
    }

    private readonly struct GCHandleByteArray : IGlobalMemory
    {
        public GCHandleByteArray(int length) => GCHandle = GCHandle.Alloc(new byte[length], GCHandleType.Pinned);

        public GCHandle GCHandle { get; }
        public nint Handle => GCHandle.AddrOfPinnedObject();
    }

    private readonly struct HGlobal : IGlobalMemory
    {
        public HGlobal(int length) => Handle = Marshal.AllocHGlobal(length);
        public HGlobal(nint handle) => Handle = handle;

        public nint Handle { get; }
    }

    private readonly struct BStr : IGlobalMemory
    {
        public BStr(int length) => Handle = Marshalling.AllocBStr(length);
        public BStr(nint handle) => Handle = handle;

        public nint Handle { get; }
    }

    private readonly struct Other : IGlobalMemory
    {
        public Other(nint handle) => Handle = handle;
        public nint Handle { get; }
    }

    private readonly object memoryObject;

    private GlobalMemory(object memory, int length)
    {
        memoryObject = memory;
        Length = length;
    }

    public int Length { get; }
    public ref byte this[int index] => ref Unsafe.Add(ref GetPinnableReference(), index);
    public ref byte this[Index index] => ref Unsafe.Add(ref GetPinnableReference(), index.GetOffset(Length));
    public Span<byte> this[Range range] => AsSpan().Slice(range.Start.GetOffset(Length), range.End.GetOffset(Length));
    public unsafe nint Handle => (nint)Unsafe.AsPointer(ref GetPinnableReference());

    public unsafe Span<byte> AsSpan() => memoryObject is IGlobalMemory globalMemory ?
        new Span<byte>((void*)globalMemory.Handle, Length) :
        new Span<byte>((byte[])memoryObject);

    public unsafe Span<T> AsSpan<T>() where T : unmanaged => new(AsPtr<T>(), Length / sizeof(T));
    public ref T AsRef<T>(int index = 0) where T : unmanaged => ref Unsafe.Add(ref Unsafe.As<byte, T>(ref GetPinnableReference()), index);
    public unsafe T* AsPtr<T>(int index = 0) where T : unmanaged => (T*)Unsafe.AsPointer(ref AsRef<T>(index));

    public static implicit operator Span<byte>(GlobalMemory left) => left.AsSpan();
    public unsafe static implicit operator void*(GlobalMemory left) => (void*)left.Handle;
    public static implicit operator nint(GlobalMemory left) => left.Handle;

    public unsafe ref byte GetPinnableReference() => ref memoryObject is IGlobalMemory globalMemory ?
        ref *(byte*)globalMemory.Handle :
        ref ((byte[])memoryObject)[0];

    private void Free()
    {
        var memory = memoryObject;
        if (memory is HGlobal global)
            Marshal.FreeHGlobal(global.Handle);
        else if (memory is BStr bstr)
            Marshal.FreeBSTR(bstr.Handle);
        else if (memory is GCHandleByteArray array)
            array.GCHandle.Free();
    }

    public void Dispose()
    {
        Free();
        GC.SuppressFinalize(this);
    }

    ~GlobalMemory()
    {
        Free();
    }

    public static GlobalMemory Allocate(int length) => new(GC.AllocateUninitializedArray<byte>(length <= 0 ? 1 : length, pinned: true), length);

    internal static GlobalMemory FromHGlobal(nint hGlobal, int len) => new(new HGlobal(hGlobal), len);
    internal static GlobalMemory FromBStr(nint bStr, int len) => new(new BStr(bStr), len);
    internal static GlobalMemory FromAnyPtr(nint val, int len) => new(new Other(val), len);
}