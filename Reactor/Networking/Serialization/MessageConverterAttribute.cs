using System;
using System.Reflection;
using HarmonyLib;

namespace Reactor.Networking.Serialization
{
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
                        var messageConverter = (UnsafeMessageConverter) Activator.CreateInstance(type);
                        MessageSerializer.Register(messageConverter);
                    }
                    catch (Exception e)
                    {
                        Logger<ReactorPlugin>.Warning($"Failed to register {type.FullDescription()}: {e}");
                    }
                }
            }
        }

        internal static void Initialize()
        {
            ChainloaderHooks.PluginLoad += plugin => Register(plugin.GetType().Assembly);
        }
    }
}
