using System;
using System.Reflection;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;

namespace Reactor.Networking.Serialization;

[AttributeUsage(AttributeTargets.Class)]
public class MessageConverterAttribute : Attribute
{
    public static void Register(Assembly assembly)
    {
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
