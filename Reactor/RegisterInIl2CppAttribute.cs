using System;
using System.Reflection;
using HarmonyLib;
using UnhollowerRuntimeLib;

namespace Reactor
{
    /// <summary>
    /// Utility attribute for automatically calling <see cref="UnhollowerRuntimeLib.ClassInjector.RegisterTypeInIl2Cpp{T}"/>
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class RegisterInIl2CppAttribute : Attribute
    {
        public static void Register()
        {
            Register(Assembly.GetCallingAssembly());
        }

        public static void Register(Assembly assembly)
        {
            foreach (var type in assembly.GetTypes())
            {
                var attribute = type.GetCustomAttribute<RegisterInIl2CppAttribute>();

                if (attribute != null)
                {
                    try
                    {
                        typeof(ClassInjector).GetMethod(nameof(ClassInjector.RegisterTypeInIl2Cpp))!
                            .MakeGenericMethod(type)
                            .Invoke(null, new object[0]);
                    }
                    catch (Exception e)
                    {
                        PluginSingleton<ReactorPlugin>.Instance.Log.LogWarning($"Failed to register {type.FullDescription()}: {e}");
                    }
                }
            }
        }
    }
}
