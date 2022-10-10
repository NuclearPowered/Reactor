using System;

namespace Reactor.Networking.Rpc;

/// <summary>
/// Provides access to singleton custom rpc's.
/// </summary>
/// <typeparam name="T">The type of the custom rpc.</typeparam>
public static class Rpc<T> where T : UnsafeCustomRpc
{
    private static T? _instance;

    /// <summary>
    /// Gets an instance of <typeparamref name="T"/> rpc.
    /// </summary>
    public static T Instance
    {
        get
        {
            if (_instance == null)
            {
                throw new InvalidOperationException($"{typeof(T).FullName} isn't registered");
            }

            return _instance;
        }

        internal set
        {
            if (_instance != null)
            {
                throw new InvalidOperationException($"{typeof(T).FullName} is already registered");
            }

            _instance = value;
        }
    }
}
