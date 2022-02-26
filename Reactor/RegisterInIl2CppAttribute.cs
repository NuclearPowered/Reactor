using System;
using System.Reflection;
using HarmonyLib;
using UnhollowerRuntimeLib;

namespace Reactor
{
    /// <summary>
    /// Utility attribute for automatically calling <see cref="ClassInjector.RegisterTypeInIl2Cpp{T}()"/>
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class RegisterInIl2CppAttribute : Attribute
    {
        public Type[] Interfaces { get; }

        public RegisterInIl2CppAttribute()
        {
            Interfaces = Type.EmptyTypes;
        }

        public RegisterInIl2CppAttribute(params Type[] interfaces)
        {
            Interfaces = interfaces;
        }

        [Obsolete("You don't need to call this anymore", true)]
        public static void Register()
        {
        }

        private static void Register(Type type, Type[] interfaces)
        {
            var baseTypeAttribute = type.BaseType?.GetCustomAttribute<RegisterInIl2CppAttribute>();
            if (baseTypeAttribute != null)
            {
                Register(type.BaseType!, baseTypeAttribute.Interfaces);
            }

            if (ClassInjector.IsTypeRegisteredInIl2Cpp(type))
            {
                return;
            }

            try
            {
                ClassInjector.RegisterTypeInIl2Cpp(type, new RegisterTypeOptions { Interfaces = interfaces });
            }
            catch (Exception e)
            {
                Logger<ReactorPlugin>.Warning($"Failed to register {type.FullDescription()}: {e}");
            }
        }

        public static void Register(Assembly assembly)
        {
            foreach (var type in assembly.GetTypes())
            {
                var attribute = type.GetCustomAttribute<RegisterInIl2CppAttribute>();
                if (attribute != null)
                {
                    Register(type, attribute.Interfaces);
                }
            }
        }

        internal static void Initialize()
        {
            ChainloaderHooks.PluginLoad += plugin => Register(plugin.GetType().Assembly);
        }
    }
}
