using System.Collections.Generic;
using Il2CppSystem.Runtime.CompilerServices;

namespace Reactor.Utilities;

public sealed class Il2CppEqualityComparer<T> : IEqualityComparer<T> where T : Il2CppSystem.Object
{
    private static Il2CppEqualityComparer<T>? _instance;

    public static Il2CppEqualityComparer<T> Instance
    {
        get
        {
            _instance ??= new Il2CppEqualityComparer<T>();
            return _instance;
        }
    }

    private Il2CppEqualityComparer()
    {
    }

    /// <inheritdoc/>
    public int GetHashCode(T obj)
    {
        return RuntimeHelpers.GetHashCode(obj);
    }

    /// <inheritdoc/>
    public bool Equals(T? x, T? y)
    {
        if (x == null || y == null)
        {
            return x == null && y == null;
        }

        return x.Equals(y);
    }
}
