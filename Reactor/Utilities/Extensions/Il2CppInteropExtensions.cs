using System;
using Il2CppInterop.Runtime.InteropTypes.Arrays;

namespace Reactor.Utilities.Extensions;

public static class Il2CppInteropExtensions
{
    public static unsafe Span<T> ToSpan<T>(this Il2CppStructArray<T> array) where T : unmanaged
    {
        return new Span<T>(IntPtr.Add(array.Pointer, IntPtr.Size * 4).ToPointer(), array.Length);
    }
}
