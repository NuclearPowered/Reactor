using System;
using System.Collections.Generic;
using System.Reflection;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using Reactor.Networking.Rpc;
using Reactor.Utilities;

namespace Reactor.Networking.Attributes;

/// <summary>
/// Automatically registers a rpc.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public sealed class RegisterCustomRpcAttribute : Attribute
{
    private static readonly HashSet<Assembly> _registeredAssemblies = new();

    /// <summary>
    /// Gets the id of the rpc.
    /// </summary>
    public uint Id { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="RegisterCustomRpcAttribute"/> class.
    /// </summary>
    /// <param name="id">The id of the rpc.</param>
    public RegisterCustomRpcAttribute(uint id)
    {
        Id = id;
    }

    /// <summary>
    /// Registers all rpc's annotated with <see cref="RegisterCustomRpcAttribute"/> in the specified <paramref name="assembly"/>.
    /// </summary>
    /// <remarks>This is called automatically on plugin assemblies so you probably don't need to call this.</remarks>
    /// <param name="assembly">The assembly to search.</param>
    /// <param name="plugin">The plugin to register the rpc to.</param>
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
