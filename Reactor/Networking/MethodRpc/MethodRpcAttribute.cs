using System;
using System.Linq;
using System.Reflection;
using BepInEx.IL2CPP;
using HarmonyLib;
using Hazel;

namespace Reactor.Networking.MethodRpc;

[AttributeUsage(AttributeTargets.Method)]
public class MethodRpcAttribute : Attribute
{
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
        var methods = assembly.GetTypes()
            .SelectMany(t => t.GetMethods(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public));

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
                Logger<ReactorPlugin>.Warning($"Failed to register {method.FullDescription()}: {e}");
            }
        }
    }

    internal static void Initialize()
    {
        ChainloaderHooks.PluginLoad += plugin => Register(plugin.GetType().Assembly, plugin);
    }
}
