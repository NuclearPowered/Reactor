using System;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Double = Il2CppSystem.Double;
using Int16 = Il2CppSystem.Int16;
using Int32 = Il2CppSystem.Int32;
using Int64 = Il2CppSystem.Int64;
using Object = Il2CppSystem.Object;
using Single = Il2CppSystem.Single;
using UInt16 = Il2CppSystem.UInt16;
using UInt32 = Il2CppSystem.UInt32;
using UInt64 = Il2CppSystem.UInt64;

namespace Reactor.Utilities.Extensions;

/// <summary>
/// Provides extension methods for Il2CppInterop.
/// </summary>
public static class Il2CppInteropExtensions
{
    /// <summary>
    /// Utility class used for calling IL2CPP methods with boxed paramters.
    /// </summary>
    public readonly struct Il2CppBoxedPrimitive
    {
        /// <summary>
        /// The boxed <see cref="Il2CppSystem.Object"/>.
        /// </summary>
        internal readonly Object Object;

        private Il2CppBoxedPrimitive(Object obj) => Object = obj;

        /// <summary>
        /// Returns the <see cref="Object"/> boxed by this <see cref="Il2CppBoxedPrimitive"/>.
        /// </summary>
        /// <param name="parsable">The <see cref="Il2CppBoxedPrimitive"/>.</param>
        /// <returns>The boxed <see cref="Il2CppSystem.Object"/>.</returns>
        public static implicit operator Object(Il2CppBoxedPrimitive parsable) => parsable.Object;

        /// <summary>
        /// Creates a new instance of <see cref="Il2CppBoxedPrimitive"/> from a <see cref="short"/>.
        /// </summary>
        /// <param name="value">The <see cref="short"/> to box.</param>
        /// <returns>The <see cref="Il2CppBoxedPrimitive"/> instance.</returns>
        public static implicit operator Il2CppBoxedPrimitive(short value) => new(new Int16 { m_value = value }.BoxIl2CppObject());

        /// <summary>
        /// Creates a new instance of <see cref="Il2CppBoxedPrimitive"/> from an <see cref="int"/>.
        /// </summary>
        /// <param name="value">The <see cref="int"/> to box.</param>
        /// <returns>The <see cref="Il2CppBoxedPrimitive"/> instance.</returns>
        public static implicit operator Il2CppBoxedPrimitive(int value) => new(new Int32 { m_value = value }.BoxIl2CppObject());

        /// <summary>
        /// Creates a new instance of <see cref="Il2CppBoxedPrimitive"/> from a <see cref="long"/>.
        /// </summary>
        /// <param name="value">The <see cref="long"/> to box.</param>
        /// <returns>The <see cref="Il2CppBoxedPrimitive"/> instance.</returns>
        public static implicit operator Il2CppBoxedPrimitive(long value) => new(new Int64 { m_value = value }.BoxIl2CppObject());

        /// <summary>
        /// Creates a new instance of <see cref="Il2CppBoxedPrimitive"/> from a <see cref="ushort"/>.
        /// </summary>
        /// <param name="value">The <see cref="ushort"/> to box.</param>
        /// <returns>The <see cref="Il2CppBoxedPrimitive"/> instance.</returns>
        public static implicit operator Il2CppBoxedPrimitive(ushort value) => new(new UInt16 { m_value = value }.BoxIl2CppObject());

        /// <summary>
        /// Creates a new instance of <see cref="Il2CppBoxedPrimitive"/> from a <see cref="uint"/>.
        /// </summary>
        /// <param name="value">The <see cref="uint"/> to box.</param>
        /// <returns>The <see cref="Il2CppBoxedPrimitive"/> instance.</returns>
        public static implicit operator Il2CppBoxedPrimitive(uint value) => new(new UInt32 { m_value = value }.BoxIl2CppObject());

        /// <summary>
        /// Creates a new instance of <see cref="Il2CppBoxedPrimitive"/> from a <see cref="ulong"/>.
        /// </summary>
        /// <param name="value">The <see cref="ulong"/> to box.</param>
        /// <returns>The <see cref="Il2CppBoxedPrimitive"/> instance.</returns>
        public static implicit operator Il2CppBoxedPrimitive(ulong value) => new(new UInt64 { m_value = value }.BoxIl2CppObject());

        /// <summary>
        /// Creates a new instance of <see cref="Il2CppBoxedPrimitive"/> from a <see cref="float"/>.
        /// </summary>
        /// <param name="value">The <see cref="float"/> to box.</param>
        /// <returns>The <see cref="Il2CppBoxedPrimitive"/> instance.</returns>
        public static implicit operator Il2CppBoxedPrimitive(float value) => new(new Single { m_value = value }.BoxIl2CppObject());

        /// <summary>
        /// Creates a new instance of <see cref="Il2CppBoxedPrimitive"/> from a <see cref="double"/>.
        /// </summary>
        /// <param name="value">The <see cref="double"/> to box.</param>
        /// <returns>The <see cref="Il2CppBoxedPrimitive"/> instance.</returns>
        public static implicit operator Il2CppBoxedPrimitive(double value) => new(new Double { m_value = value }.BoxIl2CppObject());

        /// <summary>
        /// Creates a new instance of <see cref="Il2CppBoxedPrimitive"/> from a <see cref="string"/>.
        /// </summary>
        /// <param name="value">The <see cref="string"/> to box.</param>
        /// <returns>The <see cref="Il2CppBoxedPrimitive"/> instance.</returns>
        public static implicit operator Il2CppBoxedPrimitive(string value) => new(value);
    }

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
