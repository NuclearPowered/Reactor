using System;

namespace Reactor.Networking.Rpc;

public static class Rpc<T> where T : UnsafeCustomRpc
{
    private static T? _instance;

    public static T Instance
    {
        get
        {
            if (_instance == null)
            {
                throw new Exception($"{typeof(T).FullName} isn't registered");
            }

            return _instance;
        }

        internal set
        {
            if (_instance != null)
            {
                throw new Exception($"{typeof(T).FullName} is already registered");
            }

            _instance = value;
        }
    }
}
