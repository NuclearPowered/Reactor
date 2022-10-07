using System;
using System.Collections.Generic;
using System.Reflection;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using Il2CppInterop.Runtime.Injection;

namespace Reactor.Utilities.Attributes;

/// <summary>
/// Utility attribute for automatically calling <see cref="ClassInjector.RegisterTypeInIl2Cpp{T}()"/>
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public sealed class RegisterInIl2CppAttribute : Attribute
{
    private static readonly HashSet<Assembly> _registeredAssemblies = new();

    public Type[] Interfaces { get; }

    public RegisterInIl2CppAttribute()
    {
        Interfaces = Type.EmptyTypes;
    }

    public RegisterInIl2CppAttribute(params Type[] interfaces)
    {
        Interfaces = interfaces;
    }

    private static void RegisterType(Type type, Type[] interfaces)
    {
        var baseTypeAttribute = type.BaseType?.GetCustomAttribute<RegisterInIl2CppAttribute>();
        if (baseTypeAttribute != null)
        {
            RegisterType(type.BaseType!, baseTypeAttribute.Interfaces);
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
            Warning($"Failed to register {type.FullDescription()}: {e}");
        }
    }

    /// <summary>
    /// Registers in Il2Cpp all types marked with <see cref="RegisterInIl2CppAttribute"/> from the specified assembly.<br/>
    /// </summary>
    /// <param name="assembly"></param>
    public static void Register(Assembly assembly)
    {
        if (_registeredAssemblies.Contains(assembly)) return;
        _registeredAssemblies.Add(assembly);

        foreach (var type in assembly.GetTypes())
        {
            var attribute = type.GetCustomAttribute<RegisterInIl2CppAttribute>();
            if (attribute != null)
            {
                RegisterType(type, attribute.Interfaces);
            }
        }
    }

    internal static void Initialize()
    {
        IL2CPPChainloader.Instance.PluginLoad += (_, assembly, _) => Register(assembly);
    }
}
