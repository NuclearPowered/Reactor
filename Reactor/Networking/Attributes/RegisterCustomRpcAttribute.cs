using System;
using System.Collections.Generic;
using System.Reflection;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using Reactor.Networking.Rpc;
using Reactor.Utilities;

namespace Reactor.Networking.Attributes;

/// <summary>
/// Utility attribute for automatically registering CustomRpc
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public sealed class RegisterCustomRpcAttribute : Attribute
{
    private static readonly HashSet<Assembly> _registeredAssemblies = new();

    public uint Id { get; }

    public RegisterCustomRpcAttribute(uint id)
    {
        Id = id;
    }

    /// <summary>
    /// Registers all types marked with <see cref="RegisterCustomRpcAttribute"/> from the specified assembly as custom rpcs
    /// </summary>
    /// <param name="assembly"></param>
    /// <param name="plugin"></param>
    public static void Register(Assembly assembly, BasePlugin plugin)
    {
        if (_registeredAssemblies.Contains(assembly)) return;
        _registeredAssemblies.Add(assembly);

        foreach (var type in assembly.GetTypes())
        {
            var attribute = type.GetCustomAttribute<RegisterCustomRpcAttribute>();

            if (attribute != null)
            {
                if (!type.IsSubclassOf(typeof(UnsafeCustomRpc)))
                {
                    throw new InvalidOperationException($"Type {type.FullDescription()} has {nameof(RegisterCustomRpcAttribute)} but doesn't extend {nameof(UnsafeCustomRpc)}.");
                }

                var customRpc = (UnsafeCustomRpc) Activator.CreateInstance(type, plugin, attribute.Id)!;
                PluginSingleton<ReactorPlugin>.Instance.CustomRpcManager.Register(customRpc);
            }
        }
    }

    internal static void Initialize()
    {
        IL2CPPChainloader.Instance.PluginLoad += (_, assembly, plugin) => Register(assembly, plugin);
    }
}
