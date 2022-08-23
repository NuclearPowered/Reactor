using System.Collections.Generic;
using Il2CppSystem.Runtime.CompilerServices;

namespace Reactor;

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

    public int GetHashCode(T value)
    {
        return RuntimeHelpers.GetHashCode(value);
    }

    public bool Equals(T? left, T? right)
    {
        if (left == null || right == null)
        {
            return left == null && right == null;
        }

        return left.Equals(right);
    }
}
