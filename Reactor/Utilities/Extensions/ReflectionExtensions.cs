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

public static class ReflectionExtensions
{
    public static Il2CppMethodInfo ToIl2CppMethodInfo(this MethodInfo methodInfo)
    {
        var il2CppMethodField = Il2CppInteropUtils.GetIl2CppMethodInfoPointerFieldForGeneratedMethod(methodInfo);
        if (il2CppMethodField == null) throw new ArgumentException($"'{methodInfo.Name}' is not an il2cpp method", nameof(methodInfo));
        var il2CppMethod = (IntPtr) il2CppMethodField.GetValue(null)!;

        return new Il2CppSystem.Reflection.MethodInfo(IL2CPP.il2cpp_method_get_object(il2CppMethod, IntPtr.Zero));
    }

    public static Il2CppSystemType GetEnumeratorMoveNextType(this Il2CppMethodInfo methodInfo)
    {
        var customAttribute = Il2CppCustomAttributeExtensions.GetCustomAttribute(methodInfo, Il2CppType.Of<IteratorStateMachineAttribute>()).TryCast<IteratorStateMachineAttribute>();
        if (customAttribute == null) throw new ArgumentException($"'{methodInfo.Name}' is not an enumerator method", nameof(methodInfo));

        return customAttribute._StateMachineType_k__BackingField;
    }

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

        throw new Exception("Failed to find a system type for " + type.AssemblyQualifiedName);
    }

    public static MethodInfo EnumeratorMoveNext(Type type, string methodName)
    {
        return AccessTools.Method(AccessTools.Method(type, methodName).ToIl2CppMethodInfo().GetEnumeratorMoveNextType().ToSystemType(), "MoveNext");
    }

    public static IEnumerable<MethodBase> GetMethods(this Type type, BindingFlags bindingAttr, Type returnType, params Type[] parameterTypes)
    {
        return type.GetMethods(bindingAttr).Where(x => x.ReturnType == returnType && x.GetParameters().Select(x => x.ParameterType).SequenceEqual(parameterTypes));
    }

    public static IEnumerable<MethodBase> GetMethods(this Type type, Type returnType, params Type[] parameterTypes)
    {
        return type.GetMethods(AccessTools.all, returnType, parameterTypes);
    }
}
