using System;
using System.Collections.Generic;
using Il2CppInterop.Generator.Extensions;

namespace Reactor.Networking.Rpc;

public class CustomRpcManager
{
    public const byte CallId = byte.MaxValue;

    private readonly List<UnsafeCustomRpc> _list = new();
    internal readonly Dictionary<Type, Dictionary<Mod, Dictionary<uint, UnsafeCustomRpc>>> _map = new();

    public IReadOnlyList<UnsafeCustomRpc> List => _list.AsReadOnly();

    public UnsafeCustomRpc Register(UnsafeCustomRpc customRpc)
    {
        customRpc.Manager = this;
        _list.Add(customRpc);
        _map.GetOrCreate(customRpc.InnerNetObjectType, _ => new Dictionary<Mod, Dictionary<uint, UnsafeCustomRpc>>())
            .GetOrCreate(customRpc.Mod, _ => new Dictionary<uint, UnsafeCustomRpc>())
            .Add(customRpc.Id, customRpc);

        if (customRpc.IsSingleton)
        {
            typeof(Rpc<>).MakeGenericType(customRpc.GetType()).GetProperty("Instance")!.SetValue(null, customRpc);
        }

        return customRpc;
    }
}
