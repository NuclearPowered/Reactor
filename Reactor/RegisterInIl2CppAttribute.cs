using System;
using System.Reflection;
using HarmonyLib;
using UnhollowerBaseLib;
using UnhollowerBaseLib.Runtime;
using UnhollowerBaseLib.Runtime.VersionSpecific.Class;
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

        private static readonly Func<Type, IntPtr> _readClassPointerForType = AccessTools.MethodDelegate<Func<Type, IntPtr>>(
            AccessTools.Method(typeof(ClassInjector), "ReadClassPointerForType")
        );

        private static unsafe void Register(Type type, Type[] interfaces)
        {
            var baseTypeAttribute = type.BaseType?.GetCustomAttribute<RegisterInIl2CppAttribute>();
            if (baseTypeAttribute != null)
            {
                Register(type.BaseType, baseTypeAttribute.Interfaces);
            }

            if (ClassInjector.IsTypeRegisteredInIl2Cpp(type))
            {
                return;
            }

            try
            {
                var nativeInterfaces = new INativeClassStruct[interfaces.Length];
                for (var index = 0; index < interfaces.Length; index++)
                {
                    var interfaceType = interfaces[index];
                    var klassPtr = _readClassPointerForType(interfaceType);
                    IL2CPP.il2cpp_runtime_class_init(klassPtr);
                    nativeInterfaces[index] = UnityVersionHandler.Wrap((Il2CppClass*) klassPtr);
                }

                ClassInjector.RegisterTypeInIl2Cpp(type, nativeInterfaces);
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
