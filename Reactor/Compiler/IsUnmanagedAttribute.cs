// ReSharper disable CheckNamespace

namespace System.Runtime.CompilerServices;

/// <summary>
/// This attribute is required by the compiler for the "unmanaged" generic type constraint.
/// It should be generated automatically, but for whatever reason its not.
/// </summary>
[Obsolete("Do not use directly.", true)]
internal class IsUnmanagedAttribute : Attribute
{
}
