// ReSharper disable CheckNamespace

namespace System.Runtime.CompilerServices;

/// <summary>
/// This attribute is required by the compiler for the "unmanaged" generic type constraint.
/// </summary>
[Obsolete("Do not use directly.", true)]
internal class IsUnmanagedAttribute : Attribute
{
}
