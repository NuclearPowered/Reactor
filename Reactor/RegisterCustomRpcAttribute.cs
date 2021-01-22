using System;
using System.Reflection;
using BepInEx.IL2CPP;
using HarmonyLib;

namespace Reactor
{
    /// <summary>
    /// Utility attribute for automatically registering CustomRpc
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class RegisterCustomRpcAttribute : Attribute
    {
        public static void Register(BasePlugin plugin)
        {
            Register(Assembly.GetCallingAssembly(), plugin);
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

                    var customRpc = (UnsafeCustomRpc) Activator.CreateInstance(type, plugin);
                    PluginSingleton<ReactorPlugin>.Instance.CustomRpcManager.Register(customRpc);
                }
            }
        }
    }
}
