using System;
using System.Collections.Generic;
using System.Reflection;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using Reactor.Networking.Serialization;

namespace Reactor.Networking.Attributes;

/// <summary>
/// Automatically registers a <see cref="MessageConverter{T}"/>.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public sealed class MessageConverterAttribute : Attribute
{
    private static readonly HashSet<Assembly> _registeredAssemblies = new();

    /// <summary>
    /// Registers all <see cref="MessageConverter{T}"/>s annotated with <see cref="MessageConverterAttribute"/> in the specified <paramref name="assembly"/>.
    /// </summary>
    /// <remarks>This is called automatically on plugin assemblies so you probably don't need to call this.</remarks>
    /// <param name="assembly">The assembly to search.</param>
    public static void Register(Assembly assembly)
    {
        if (_registeredAssemblies.Contains(assembly)) return;
        _registeredAssemblies.Add(assembly);

        foreach (var type in assembly.GetTypes())
        {
            var attribute = type.GetCustomAttribute<MessageConverterAttribute>();

            if (attribute != null)
            {
                if (!type.IsSubclassOf(typeof(UnsafeMessageConverter)))
                {
                    throw new InvalidOperationException($"Type {type.FullDescription()} has {nameof(MessageConverterAttribute)} but doesn't extend {nameof(UnsafeMessageConverter)}.");
                }

                try
                {
                    MessageSerializer.Register(type);
                }
                catch (Exception e)
                {
                    Warning($"Failed to register {type.FullDescription()}: {e}");
                }
            }
        }
    }

    internal static void Initialize()
    {
        IL2CPPChainloader.Instance.PluginLoad += (_, assembly, _) => Register(assembly);
        IL2CPPChainloader.Instance.Finished += MessageSerializer.ClearMaps;
    }
}
