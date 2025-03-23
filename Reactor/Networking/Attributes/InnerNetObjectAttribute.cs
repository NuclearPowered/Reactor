using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using InnerNet;
using UnityEngine;

namespace Reactor.Networking.Attributes;

/// <summary>
/// Automatically registers a custom <see cref="InnerNetObject"/>.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public sealed class InnerNetObjectAttribute : Attribute
{
    private static readonly HashSet<Assembly> _registeredAssemblies = new();

    /// <summary>
    /// Registers all <see cref="InnerNetObject"/>s annotated with <see cref="InnerNetObjectAttribute"/> in the specified <paramref name="assembly"/>.
    /// </summary>
    /// <remarks>This is called automatically on plugin assemblies so you probably don't need to call this.</remarks>
    /// <param name="assembly">The assembly to search.</param>
    public static void Register(Assembly assembly)
    {
        if (_registeredAssemblies.Contains(assembly)) return;
        _registeredAssemblies.Add(assembly);

        foreach (var type in assembly.GetTypes())
        {
            var attribute = type.GetCustomAttribute<InnerNetObjectAttribute>();

            if (attribute != null)
            {
                if (!type.IsSubclassOf(typeof(InnerNetObject)))
                {
                    throw new InvalidOperationException($"Type {type.FullDescription()} has {nameof(InnerNetObjectAttribute)} but doesn't extend {nameof(InnerNetObject)}.");
                }

                try
                {
                    var methods = type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);

                    var prefabMethod = methods.FirstOrDefault(method =>
                        method.GetCustomAttribute<InnerNetObjectPrefabAttribute>() != null &&
                        (method.ReturnType == typeof(GameObject) || method.ReturnType == typeof(InnerNetObject)) &&
                        method.IsStatic);

                    if (prefabMethod != null)
                    {
                        var prefab = prefabMethod.Invoke(null, null);

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
                    else
                    {
                        Warning($"Failed to register InnerNetObject, static prefab return method not found.");
                    }
                }
                catch (Exception ex)
                {
                    Warning($"Failed to register {type.FullDescription()}: {ex}");
                }
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
        // Increase array length by one because of beginning if check in InnerNetClient.CoHandleSpawn()
        var innerNetClient = AmongUsClient.Instance;
        var list2 = innerNetClient.SpawnableObjects.ToList();
        list2.Add(new());
        innerNetClient.SpawnableObjects = list2.ToArray();

        IL2CPPChainloader.Instance.PluginLoad += (_, assembly, _) => Register(assembly);
    }
}
