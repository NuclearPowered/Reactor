using System;
using System.Reflection;
using BepInEx.IL2CPP;
using HarmonyLib;

namespace Reactor.Networking.MethodRpc
{
    [AttributeUsage(AttributeTargets.Class)]
    public class ReaderWriterAttribute : System.Attribute
    {
        
        public static void Register(Assembly assembly, BasePlugin plugin)
        {
            foreach (var type in assembly.GetTypes())
            {
                var attribute = type.GetCustomAttribute<ReaderWriterAttribute>();

                if (attribute != null)
                {
                    if (!type.IsSubclassOf(typeof(UnsafeReaderWriter)))
                    {
                        throw new InvalidOperationException($"Type {type.FullDescription()} has {nameof(ReaderWriterAttribute)} but doesn't extend {nameof(UnsafeReaderWriter)}.");
                    }

                    var customRpc = (UnsafeReaderWriter) Activator.CreateInstance(type);
                    ReaderWriterManager.Instance.Register(customRpc);
                }
            }
        }
        
        internal static void Initialize()
        {
            ChainloaderHooks.PluginLoad += plugin => Register(plugin.GetType().Assembly, plugin);
        }
    }
}
