using System;
using System.Collections.Generic;
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
        public RegisterInIl2CppAttribute(params Type[] interfaces)
        {
            Interfaces = interfaces;
        }

        private Type[] Interfaces { get; }
        
        [Obsolete("You don't need to call this anymore", true)]
        public static void Register()
        {
        }

        private static readonly AccessTools.FieldRef<object, HashSet<string>> _injectedTypes
            = AccessTools.FieldRefAccess<HashSet<string>>(typeof(ClassInjector), "InjectedTypes");

        private static readonly Func<Type, IntPtr> _readClassPointerForType = AccessTools.MethodDelegate<Func<Type, IntPtr>>(
            AccessTools.Method(typeof(ClassInjector), "ReadClassPointerForType")
        );

        private static bool IsInjected(Type type)
        {
            if (_readClassPointerForType(type) != IntPtr.Zero)
            {
                return true;
            }

            var injectedTypes = _injectedTypes();

            lock (injectedTypes)
            {
                if (injectedTypes.Contains(type.FullName))
                {
                    return true;
                }
            }

            return false;
        }

        private static unsafe void Register(Type type, Type[] interfaces)
        {
            var baseTypeAttribute = type.BaseType?.GetCustomAttribute<RegisterInIl2CppAttribute>();
            if (baseTypeAttribute != null)
            {
                Register(type.BaseType, baseTypeAttribute.Interfaces);
            }

            if (IsInjected(type))
            {
                return;
            }

            try
            {
                var nativeClassStructs = new INativeClassStruct[interfaces.Length];
                for (int index = 0; index < interfaces.Length; index++)
                {
                    var attributeInterface = interfaces[index];

                    IntPtr klassPtr = (IntPtr) typeof(Il2CppClassPointerStore<>).MakeGenericType(attributeInterface).GetField(nameof(Il2CppClassPointerStore<Il2CppObjectBase>.NativeClassPtr)).GetValue(null);
                    IL2CPP.il2cpp_runtime_class_init(klassPtr);
                    nativeClassStructs[index] = UnityVersionHandler.Wrap((Il2CppClass*)klassPtr);
                }

                ClassInjector.RegisterTypeInIl2Cpp(type, nativeClassStructs);
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
