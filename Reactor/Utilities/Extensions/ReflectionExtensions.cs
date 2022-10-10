using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using Il2CppInterop.Common;
using Il2CppInterop.Common.Attributes;
using Il2CppInterop.Runtime;
using Il2CppSystem.Runtime.CompilerServices;
using Il2CppCustomAttributeExtensions = Il2CppSystem.Reflection.CustomAttributeExtensions;
using Il2CppMethodInfo = Il2CppSystem.Reflection.MethodInfo;
using Il2CppSystemType = Il2CppSystem.Type;
using MethodInfo = System.Reflection.MethodInfo;

namespace Reactor.Utilities.Extensions;

/// <summary>
/// Provides extension methods for reflection.
/// </summary>
public static class ReflectionExtensions
{
    /// <summary>
    /// Gets a <see cref="Il2CppMethodInfo"/> for the specified <see cref="MethodInfo"/>.
    /// </summary>
    /// <param name="methodInfo">The <see cref="MethodInfo"/>.</param>
    /// <returns>A <see cref="Il2CppMethodInfo"/>.</returns>
    public static Il2CppMethodInfo ToIl2CppMethodInfo(this MethodInfo methodInfo)
    {
        var il2CppMethodField = Il2CppInteropUtils.GetIl2CppMethodInfoPointerFieldForGeneratedMethod(methodInfo);
        if (il2CppMethodField == null) throw new ArgumentException($"'{methodInfo.Name}' is not an il2cpp method", nameof(methodInfo));
        var il2CppMethod = (IntPtr) il2CppMethodField.GetValue(null)!;

        return new Il2CppMethodInfo(IL2CPP.il2cpp_method_get_object(il2CppMethod, IntPtr.Zero));
    }

    /// <summary>
    /// Gets enumerator's MoveNext type for the specified <see cref="Il2CppMethodInfo"/>.
    /// </summary>
    /// <param name="methodInfo">The enumerator <see cref="Il2CppMethodInfo"/>.</param>
    /// <returns>A <see cref="Il2CppSystemType"/> for enumerator's MoveNext type.</returns>
    public static Il2CppSystemType GetEnumeratorMoveNextType(this Il2CppMethodInfo methodInfo)
    {
        var customAttribute = Il2CppCustomAttributeExtensions.GetCustomAttribute(methodInfo, Il2CppType.Of<IteratorStateMachineAttribute>()).TryCast<IteratorStateMachineAttribute>();
        if (customAttribute == null) throw new ArgumentException($"'{methodInfo.Name}' is not an enumerator method", nameof(methodInfo));

        return customAttribute._StateMachineType_k__BackingField;
    }

    /// <summary>
    /// Gets a <see cref="Type"/> for the specified <see cref="Il2CppSystemType"/>.
    /// </summary>
    /// <param name="type">The <see cref="Il2CppSystemType"/>.</param>
    /// <returns>A <see cref="Type"/>.</returns>
    public static Type ToSystemType(this Il2CppSystemType type)
    {
        var result = Type.GetType(type.AssemblyQualifiedName);
        if (result != null) return result;

        foreach (var t in AccessTools.GetTypesFromAssembly(Assembly.Load(type.Assembly.FullName)))
        {
            if (t.Namespace != type.Namespace) continue;

            var name = t.GetCustomAttribute<ObfuscatedNameAttribute>()?.ObfuscatedName?.Replace('/', '+') ?? t.FullName;
            if (name != type.FullName) continue;

            return t;
        }

        throw new TypeLoadException("Failed to find a system type for " + type.AssemblyQualifiedName);
    }

    /// <summary>
    /// Gets MoveNext <see cref="MethodInfo"/> for specified enumerator method.
    /// </summary>
    /// <param name="type">The enclosing type of the method.</param>
    /// <param name="methodName">The name of the method.</param>
    /// <returns>A <see cref="MethodInfo"/>.</returns>
    public static MethodInfo EnumeratorMoveNext(Type type, string methodName)
    {
        return AccessTools.Method(AccessTools.Method(type, methodName).ToIl2CppMethodInfo().GetEnumeratorMoveNextType().ToSystemType(), "MoveNext");
    }

    /// <summary>
    /// Gets all method from the <paramref name="type"/> with the specified <paramref name="bindingFlags"/>, <paramref name="returnType"/> and <paramref name="parameterTypes"/>.
    /// </summary>
    /// <param name="type">The type to search the methods in.</param>
    /// <param name="bindingFlags">The <see cref="BindingFlags"/>.</param>
    /// <param name="returnType">The return type.</param>
    /// <param name="parameterTypes">The parameter types.</param>
    /// <returns>An array of <see cref="MethodInfo"/> objects representing all methods defined for the current <see cref="Type"/> that match the specified binding constraints.</returns>
    public static IEnumerable<MethodBase> GetMethods(this Type type, BindingFlags bindingFlags, Type returnType, params Type[] parameterTypes)
    {
        return type.GetMethods(bindingFlags).Where(x => x.ReturnType == returnType && x.GetParameters().Select(x => x.ParameterType).SequenceEqual(parameterTypes));
    }

    /// <summary>
    /// Gets all method from the <paramref name="type"/> with the specified <paramref name="returnType"/> and <paramref name="parameterTypes"/>.
    /// </summary>
    /// <param name="type">The type to search the methods in.</param>
    /// <param name="returnType">The return type.</param>
    /// <param name="parameterTypes">The parameter types.</param>
    /// <returns>An array of <see cref="MethodInfo"/> objects representing all methods defined for the current <see cref="Type"/> that match the specified binding constraints.</returns>
    public static IEnumerable<MethodBase> GetMethods(this Type type, Type returnType, params Type[] parameterTypes)
    {
        return type.GetMethods(AccessTools.all, returnType, parameterTypes);
    }
}
