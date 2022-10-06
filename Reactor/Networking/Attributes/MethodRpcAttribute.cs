using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using Hazel;
using Reactor.Networking.Rpc;
using Reactor.Utilities;

namespace Reactor.Networking.Attributes;

[AttributeUsage(AttributeTargets.Method)]
public class MethodRpcAttribute : Attribute
{
    private static readonly HashSet<Assembly> _registeredAssemblies = new();

    public uint Id { get; }
    public SendOption Option { get; set; } = SendOption.Reliable;
    public RpcLocalHandling LocalHandling { get; set; } = RpcLocalHandling.Before;
    public bool SendImmediately { get; set; }

    public MethodRpcAttribute(uint id)
    {
        Id = id;
    }

    public static void Register(Assembly assembly, BasePlugin plugin)
    {
        if (_registeredAssemblies.Contains(assembly)) return;
        _registeredAssemblies.Add(assembly);

        var methods = assembly.GetTypes()
            .SelectMany(t => t.GetMethods(BindingFlags.Static | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public));

        foreach (var method in methods)
        {
            var attribute = method.GetCustomAttribute<MethodRpcAttribute>();
            if (attribute == null)
            {
                continue;
            }

            try
            {
                var customRpc = new MethodRpc(plugin, method, attribute.Id, attribute.Option, attribute.LocalHandling, attribute.SendImmediately);
                PluginSingleton<ReactorPlugin>.Instance.CustomRpcManager.Register(customRpc);
            }
            catch (Exception e)
            {
                Warning($"Failed to register {method.FullDescription()}: {e}");
            }
        }
    }

    internal static void Initialize()
    {
        IL2CPPChainloader.Instance.PluginLoad += (_, assembly, plugin) => Register(assembly, plugin);
    }
}
