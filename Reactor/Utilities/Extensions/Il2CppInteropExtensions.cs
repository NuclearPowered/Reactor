using System;
using Il2CppInterop.Runtime.InteropTypes.Arrays;

namespace Reactor.Utilities.Extensions;

/// <summary>
/// Provides extension methods for Il2CppInterop.
/// </summary>
public static class Il2CppInteropExtensions
{
    /// <summary>
    /// Creates a span over a <see cref="Il2CppStructArray{T}"/>.
    /// </summary>
    /// <param name="array">The array to create a span over.</param>
    /// <typeparam name="T">The type of items in the <see cref="Il2CppStructArray{T}"/>.</typeparam>
    /// <returns>A span.</returns>
    public static unsafe Span<T> ToSpan<T>(this Il2CppStructArray<T> array) where T : unmanaged
    {
        return new Span<T>(IntPtr.Add(array.Pointer, IntPtr.Size * 4).ToPointer(), array.Length);
    }
}
