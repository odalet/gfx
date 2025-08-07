using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using Sys = System;

namespace ScanPlayer.OpenGL;

public enum NativeStringEncoding
{
    BStr = 19,
    LPStr = 20,
    LPTStr = 22,
    LPUTF8Str = 48,
    LPWStr = 21,
    Ansi = 20,
    Auto = 22,
    Uni = 21,
    UTF8 = 48
}

// Adapted from Silk.NET
internal static class Marshalling
{
    // Store the GlobalMemory instances so that on .NET 5 the pinned object heap isn't prematurely garbage collected
    // This means that the GlobalMemory is only freed when the user calls Free.
    private static readonly ConcurrentDictionary<nint, GlobalMemory> marshalledMemory = new();

    // In addition, we should keep track of the memory we allocate dedicated to string arrays. If we don't, we won't
    // know to free the individual strings allocated within memory.
    private static readonly ConcurrentDictionary<GlobalMemory, int> stringArrays = new();

    // Other kinds of GCHandle-pinned pointers may be passed into Free, like delegate pointers for example which
    // must have GCHandles allocated on older runtimes to avoid an ExecutionEngineException.
    // We should keep track of those.
    private static readonly ConcurrentDictionary<nint, GCHandle> otherGCHandles = new();

    public static bool Free(nint pointer)
    {
        var removed = otherGCHandles.TryRemove(pointer, out var gcHandle);
        if (removed)
            gcHandle.Free();

        removed = marshalledMemory.TryRemove(pointer, out var val);
        if (val is null)
            return removed;

        if (stringArrays.TryRemove(val, out var numStrings))
        {
            var span = val.AsSpan<nint>();
            for (var i = 0; i < numStrings; i++)
                _ = Free(span[i]);
        }

        val.Dispose();
        return removed;
    }

    public static string? PtrToString(nint input, NativeStringEncoding encoding = NativeStringEncoding.LPStr)
    {
        unsafe static string AnsiToString(nint ptr) => new((sbyte*)ptr);
        unsafe static string BStrToString(nint ptr) => new((char*)ptr, 0, (int)(*(uint*)(ptr - 4) / 2u));
        unsafe static string WideToString(nint ptr) => new((char*)ptr);
        unsafe static string Utf8PtrToString(nint ptr)
        {
            var span = new Span<byte>((void*)ptr, int.MaxValue);
            span = span.Slice(0, span.IndexOf<byte>(0));
            fixed (byte* bytes = span)
                return Encoding.UTF8.GetString(bytes, span.Length);
        }

        return input == 0 ? null : encoding switch
        {
            NativeStringEncoding.BStr => BStrToString(input),
            NativeStringEncoding.LPStr => AnsiToString(input),
            NativeStringEncoding.LPTStr => Utf8PtrToString(input),
            NativeStringEncoding.LPUTF8Str => Utf8PtrToString(input),
            NativeStringEncoding.LPWStr => WideToString(input),
            _ => throw new ArgumentException($"Invalid string encoding: {encoding}", nameof(encoding))
        };
    }

    public static nint StringToPtr(string? input, NativeStringEncoding encoding = NativeStringEncoding.LPStr) =>
        input == null ? 0 : RegisterMemory(StringToMemory(input, encoding));

    public static nint StringArrayToPtr(IReadOnlyList<string> input, NativeStringEncoding encoding = NativeStringEncoding.LPStr)
    {
        var globalMemory = StringArrayToMemory(input, encoding);
        _ = stringArrays.TryAdd(globalMemory, input.Count);
        return RegisterMemory(globalMemory);
    }

    public static nint AllocateString(int length, NativeStringEncoding encoding = NativeStringEncoding.LPStr) => encoding switch
    {
        NativeStringEncoding.BStr => AllocBStr(length),
        NativeStringEncoding.LPStr => Allocate(length),
        NativeStringEncoding.LPTStr => Allocate(length),
        NativeStringEncoding.LPUTF8Str => Allocate(length),
        NativeStringEncoding.LPWStr => Allocate(length),
        _ => throw new ArgumentOutOfRangeException(nameof(encoding))
    };

    public static void FreeString(nint ptr, NativeStringEncoding encoding = NativeStringEncoding.LPStr)
    {
        switch (encoding)
        {
            case NativeStringEncoding.BStr:
                FreeBStr(ptr);
                break;
            case NativeStringEncoding.LPStr:
            case NativeStringEncoding.LPWStr:
            case NativeStringEncoding.LPTStr:
            case NativeStringEncoding.LPUTF8Str:
                _ = Free(ptr);
                break;
            default: throw new ArgumentOutOfRangeException(nameof(encoding));
        }
    }

#nullable disable

    public unsafe static void CopyPtrToStringArray(nint ptr, string[] arr, NativeStringEncoding encoding = NativeStringEncoding.LPStr)
    {
        for (var i = 0; i < arr.Length; i++)
            arr[i] = PtrToString(((nint*)ptr)![i]);
    }

#nullable enable

    private static GlobalMemory StringToMemory(string input, NativeStringEncoding encoding = NativeStringEncoding.LPStr)
    {
        if (encoding == NativeStringEncoding.BStr)
            return BStrToMemory(Marshal.StringToBSTR(input), input.Length);

        var globalMemory = GlobalMemory.Allocate(GetMaxSizeOf(input));
        _ = StringIntoSpan(input, globalMemory.AsSpan<byte>(), encoding);
        return globalMemory;
    }

    private static GlobalMemory BStrToMemory(nint bStr, int length) => GlobalMemory.FromBStr(bStr, length);

    internal static nint AllocBStr(int length) => Marshal.StringToBSTR(new string('\0', length));
    public static nint Allocate(int length) => RegisterMemory(GlobalMemory.Allocate(length));

    public static void FreeBStr(nint ptr) => Marshal.FreeBSTR(ptr);

    private static GlobalMemory StringArrayToMemory(IReadOnlyList<string> input, NativeStringEncoding encoding = NativeStringEncoding.LPStr)
    {
        var globalMemory = GlobalMemory.Allocate(input.Count * IntPtr.Size);
        var span = globalMemory.AsSpan<IntPtr>();
        for (var i = 0; i < input.Count; i++)
            span[i] = StringToPtr(input[i], encoding);
        return globalMemory;
    }

    private static nint RegisterMemory(GlobalMemory memory) => (marshalledMemory[memory.Handle] = memory).Handle;

    private unsafe static int StringIntoSpan(string? input, Span<byte> span, NativeStringEncoding encoding = NativeStringEncoding.LPStr)
    {
        if (input == null)
        {
            span[0] = 0;
            return 1;
        }

        switch (encoding)
        {
            case NativeStringEncoding.BStr:
                throw new InvalidOperationException("BSTR encoded strings can only be marshalled into known BSTR memory.");
            case NativeStringEncoding.LPStr:
            case NativeStringEncoding.LPTStr:
            case NativeStringEncoding.LPUTF8Str:
                {
                    int count;
                    fixed (char* firstChar = input)
                    fixed (byte* bytes = span)
                        count = Encoding.UTF8.GetBytes(firstChar, input.Length, bytes, span.Length - 1);

                    span[count] = 0;
                    return ++count;
                }
            case NativeStringEncoding.LPWStr:
                {
                    fixed (char* firstChar = input)
                    fixed (byte* bytes = span)
                        Sys.Buffer.MemoryCopy(firstChar, bytes, span.Length, input.Length + 1);

                    return input.Length + 1;
                }
            default: throw new ArgumentOutOfRangeException(nameof(encoding));
        }
    }

    private static int GetMaxSizeOf(string? input, NativeStringEncoding encoding = NativeStringEncoding.LPStr) => encoding switch
    {
        NativeStringEncoding.BStr => -1,
        NativeStringEncoding.LPStr => ((input?.Length ?? 0) + 1) * Marshal.SystemMaxDBCSCharSize,
        NativeStringEncoding.LPTStr => ((input != null) ? Encoding.UTF8.GetMaxByteCount(input!.Length) : 0) + 1,
        NativeStringEncoding.LPUTF8Str => ((input != null) ? Encoding.UTF8.GetMaxByteCount(input!.Length) : 0) + 1,
        NativeStringEncoding.LPWStr => ((input?.Length ?? 0) + 1) * 2,
        _ => -1,
    };
}
