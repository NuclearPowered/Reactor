using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using BepInEx.Unity.IL2CPP;
using InnerNet;
using UnityEngine;

namespace Reactor.Networking.Attributes;

/// <summary>
/// Ignores registering <see cref="InnerNetObject"/>.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public sealed class IgnoreInnerNetObjectAttribute : Attribute
{
    private static readonly HashSet<string> _registeredPrefabs = new();
    private static readonly HashSet<Assembly> _registeredAssemblies = new();
    private static readonly List<(string AssemblyName, MemberInfo Member)> _registeredMembers = new();

    /// <summary>
    /// Registers all <see cref="InnerNetObject"/>s annotated without <see cref="IgnoreInnerNetObjectAttribute"/> in the specified <paramref name="assembly"/>.
    /// </summary>
    /// <remarks>This is called automatically on plugin assemblies so you probably don't need to call this.</remarks>
    /// <param name="assembly">The assembly to search.</param>
    public static void Register(Assembly assembly)
    {
        if (_registeredAssemblies.Contains(assembly)) return;
        _registeredAssemblies.Add(assembly);

        var assemblyName = assembly.GetName().Name;

        foreach (var type in assembly.GetTypes())
        {
            if (!type.IsSubclassOf(typeof(InnerNetObject))) continue;
            if (type.GetCustomAttribute<IgnoreInnerNetObjectAttribute>() != null) continue;

            try
            {
                var members = type.GetMembers(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
                var prefabMember = members.FirstOrDefault(IsValidPrefabMember);

                if (prefabMember != null && assemblyName != null)
                {
                    _registeredMembers.Add((assemblyName, prefabMember));
                }
                else
                {
                    Warning($"No valid prefab member found for {type.FullName} in {assemblyName}.");
                }
            }
            catch (Exception ex)
            {
                Warning($"Failed to register {type.FullName}: {ex}");
            }
        }
    }

    private static bool IsValidPrefabMember(MemberInfo member)
    {
        return member switch
        {
            MethodInfo method => method.IsStatic && (
                method.ReturnType == typeof(GameObject) ||
                method.ReturnType == typeof(InnerNetObject) ||
                method.ReturnType == typeof(Task<GameObject>) ||
                method.ReturnType == typeof(Task<InnerNetObject>)
            ),
            FieldInfo field => field.IsStatic && (
                field.FieldType == typeof(GameObject) ||
                field.FieldType == typeof(InnerNetObject)
            ),
            PropertyInfo property => property.GetMethod?.IsStatic == true && (
                property.PropertyType == typeof(GameObject) ||
                property.PropertyType == typeof(InnerNetObject)
            ),
            _ => false,
        };
    }

    internal static async Task LoadRegisteredAsync()
    {
        while (AmongUsClient.Instance == null)
            await Task.Delay(1000);

        var orderedMembers = _registeredMembers
            .OrderBy(x => x.AssemblyName)
            .ThenBy(x => x.Member.DeclaringType?.FullName)
            .ThenBy(x => x.Member.Name)
            .Select(x => x.Member);

        foreach (var prefabMember in orderedMembers)
        {
            var prefabFullName = $"{prefabMember.DeclaringType?.FullName}.{prefabMember.Name}";
            var prefab = await GetPrefabAsync(prefabMember);
            if (prefab == null)
            {
                Warning($"Prefab for {prefabFullName} is null.");
                continue;
            }

            if (!_registeredPrefabs.Contains(prefabFullName))
            {
                _registeredPrefabs.Add(prefabFullName);

                if (prefab is InnerNetObject netObj)
                {
                    AddInnerNetObject(netObj);
                }
                else if (prefab is GameObject gameObj)
                {
                    AddInnerNetObject(gameObj);
                }
            }
            else
            {
                Warning($"Prefab {prefabFullName} has already been registered, which shouldn't be possible but indicates there is a duplicate...");
            }
        }
    }

    private static async Task<object?> GetPrefabAsync(MemberInfo prefabMember)
    {
        object? prefab = null;

        if (prefabMember is MethodInfo method)
        {
            if (method.ReturnType == typeof(Task<GameObject>) || method.ReturnType == typeof(Task<InnerNetObject>))
            {
                if (method.Invoke(null, null) is Task task)
                {
                    await task.ConfigureAwait(false);
                    prefab = method.ReturnType == typeof(Task<GameObject>)
                        ? ((Task<GameObject>) task).Result
                        : ((Task<InnerNetObject>) task).Result;
                }
            }
            else
            {
                prefab = method.Invoke(null, null);
            }
        }
        else if (prefabMember is FieldInfo field)
        {
            prefab = field.GetValue(null);
        }
        else if (prefabMember is PropertyInfo property)
        {
            prefab = property.GetValue(null);
        }

        return prefab;
    }

    private static void AddInnerNetObject(InnerNetObject prefab)
    {
        var innerNetClient = AmongUsClient.Instance;

        // Setup InnerNetObject.
        prefab.SpawnId = (uint) innerNetClient.SpawnableObjects.Length;
        UnityEngine.Object.DontDestroyOnLoad(prefab);

        // Add InnerNetObject to NonAddressableSpawnableObjects.
        var list = innerNetClient.NonAddressableSpawnableObjects.ToList();
        list.Add(prefab);
        innerNetClient.NonAddressableSpawnableObjects = list.ToArray();

        // Increase array length by one because of beginning if check in InnerNetClient.CoHandleSpawn()
        var list2 = innerNetClient.SpawnableObjects.ToList();
        list2.Add(new());
        innerNetClient.SpawnableObjects = list2.ToArray();
    }

    private static void AddInnerNetObject(GameObject prefab)
    {
        if (prefab != null)
        {
            var netObj = prefab.GetComponent<InnerNetObject>();
            if (netObj != null)
            {
                AddInnerNetObject(netObj);
            }
        }
    }

    internal static void Initialize()
    {
        IL2CPPChainloader.Instance.PluginLoad += (_, assembly, _) => Register(assembly);
        IL2CPPChainloader.Instance.Finished += () => _ = LoadRegisteredAsync();
    }
}
