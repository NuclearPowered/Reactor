using System;
using System.Collections.Generic;
using System.Reflection;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using Reactor.Networking.Serialization;

namespace Reactor.Networking.Attributes;

[AttributeUsage(AttributeTargets.Class)]
public sealed class MessageConverterAttribute : Attribute
{
    private static readonly HashSet<Assembly> _registeredAssemblies = new();

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
                    var messageConverter = (UnsafeMessageConverter) Activator.CreateInstance(type)!;
                    MessageSerializer.Register(messageConverter);
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
    }
}
