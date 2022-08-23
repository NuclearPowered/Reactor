using System;
using System.Reflection;
using BepInEx.IL2CPP;
using HarmonyLib;
using Reactor.Networking;

namespace Reactor;

/// <summary>
/// Utility attribute for automatically registering CustomRpc
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class RegisterCustomRpcAttribute : Attribute
{
    public uint Id { get; }

    public RegisterCustomRpcAttribute(uint id)
    {
        Id = id;
    }

    [Obsolete("You don't need to call this anymore", true)]
    public static void Register(BasePlugin plugin)
    {
    }

    public static void Register(Assembly assembly, BasePlugin plugin)
    {
        foreach (var type in assembly.GetTypes())
        {
            var attribute = type.GetCustomAttribute<RegisterCustomRpcAttribute>();

            if (attribute != null)
            {
                if (!type.IsSubclassOf(typeof(UnsafeCustomRpc)))
                {
                    throw new InvalidOperationException($"Type {type.FullDescription()} has {nameof(RegisterCustomRpcAttribute)} but doesn't extend {nameof(UnsafeCustomRpc)}.");
                }

                var customRpc = (UnsafeCustomRpc) Activator.CreateInstance(type, plugin, attribute.Id);
                PluginSingleton<ReactorPlugin>.Instance.CustomRpcManager.Register(customRpc);
            }
        }
    }

    internal static void Initialize()
    {
        ChainloaderHooks.PluginLoad += plugin => Register(plugin.GetType().Assembly, plugin);
    }
}
