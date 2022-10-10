using System;
using System.Collections.Generic;
using System.Reflection;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using Il2CppInterop.Runtime.Injection;

namespace Reactor.Utilities.Attributes;

/// <summary>
/// Automatically registers an il2cpp type using <see cref="ClassInjector.RegisterTypeInIl2Cpp{T}()"/>.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public sealed class RegisterInIl2CppAttribute : Attribute
{
    private static readonly HashSet<Assembly> _registeredAssemblies = new();

    /// <summary>
    /// Gets il2cpp interfaces to be injected with this type.
    /// </summary>
    public Type[] Interfaces { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="RegisterInIl2CppAttribute"/> class without any interfaces.
    /// </summary>
    public RegisterInIl2CppAttribute()
    {
        Interfaces = Type.EmptyTypes;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RegisterInIl2CppAttribute"/> class with interfaces.
    /// </summary>
    /// <param name="interfaces">Il2Cpp interfaces to be injected with this type.</param>
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
    /// Registers all Il2Cpp types annotated with <see cref="RegisterInIl2CppAttribute"/> in the specified <paramref name="assembly"/>.
    /// </summary>
    /// <remarks>This is called automatically on plugin assemblies so you probably don't need to call this.</remarks>
    /// <param name="assembly">The assembly to search.</param>
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
