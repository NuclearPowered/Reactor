using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using InnerNet;
using UnityEngine;

namespace Reactor.Networking.Attributes;

/// <summary>
/// Ignores registering <see cref="InnerNetObject"/>.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public sealed class IgnoreInnerNetObjectAttribute : Attribute
{
    private static readonly HashSet<Assembly> _registeredAssemblies = new();
    private static readonly HashSet<MemberInfo> _registeredMembers = new();

    /// <summary>
    /// Registers all <see cref="InnerNetObject"/>s annotated with out <see cref="IgnoreInnerNetObjectAttribute"/> in the specified <paramref name="assembly"/>.
    /// </summary>
    /// <remarks>This is called automatically on plugin assemblies so you probably don't need to call this.</remarks>
    /// <param name="assembly">The assembly to search.</param>
    public static void Register(Assembly assembly)
    {
        if (_registeredAssemblies.Contains(assembly)) return;
        _registeredAssemblies.Add(assembly);

        foreach (var type in assembly.GetTypes())
        {
            if (!type.IsSubclassOf(typeof(InnerNetObject))) continue;

            var attribute = type.GetCustomAttribute<IgnoreInnerNetObjectAttribute>();

            if (attribute == null)
            {
                try
                {
                    var members = type.GetMembers(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);

                    var prefabMember = members.FirstOrDefault(member =>
                    {
                        if (member is MethodInfo method)
                        {
                            return method.IsStatic && (method.ReturnType == typeof(GameObject) || method.ReturnType == typeof(InnerNetObject) || method.ReturnType == typeof(Task<GameObject>) || method.ReturnType == typeof(Task<InnerNetObject>));
                        }
                        else if (member is FieldInfo field)
                        {
                            return field.IsStatic && (field.FieldType == typeof(GameObject) || field.FieldType == typeof(InnerNetObject));
                        }
                        else if (member is PropertyInfo property)
                        {
                            return property.GetMethod?.IsStatic == true && (property.PropertyType == typeof(GameObject) || property.PropertyType == typeof(InnerNetObject));
                        }

                        return false;
                    });

                    if (prefabMember != null)
                    {
                        _registeredMembers.Add(prefabMember);
                    }
                    else
                    {
                        Warning($"Failed to register InnerNetObject, static prefab return member not found.");
                    }
                }
                catch (Exception ex)
                {
                    Warning($"Failed to register {type.FullDescription()}: {ex}");
                }
            }
        }
    }

    internal static async Task LoadRegisteredAsync()
    {
        if (_registeredMembers.Count > 0) // Increase array length by one because of beginning if check in InnerNetClient.CoHandleSpawn()
        {
            var innerNetClient = AmongUsClient.Instance;
            var list2 = innerNetClient.SpawnableObjects.ToList();
            list2.Add(new());
            innerNetClient.SpawnableObjects = list2.ToArray();
        }

        foreach (var prefabMember in _registeredMembers)
        {
            object? prefab = null;

            if (prefabMember is MethodInfo method)
            {
                if (method.ReturnType == typeof(Task<GameObject>) || method.ReturnType == typeof(Task<InnerNetObject>))
                {
                    if (method.Invoke(null, null) is Task task)
                    {
                        await task.ConfigureAwait(false);

                        if (method.ReturnType == typeof(Task<GameObject>))
                        {
                            prefab = ((Task<GameObject>) task).Result;
                        }
                        else if (method.ReturnType == typeof(Task<InnerNetObject>))
                        {
                            prefab = ((Task<InnerNetObject>) task).Result;
                        }
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

            if (prefab == null)
            {
                Warning($"Failed to register InnerNetObject, prefab return null.");
            }
            else if (prefab is InnerNetObject netObj)
            {
                AddInnerNetObject(netObj);
            }
            else if (prefab is GameObject gameObj)
            {
                AddInnerNetObject(gameObj);
            }
        }
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
    }
}
