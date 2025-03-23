using System.Linq;
using InnerNet;

namespace Reactor.Utilities;

/// <summary>
/// Provides a standard way of managing <see cref="InnerNetObject"/>s.
/// </summary>
public static class InnerNetObjectManager
{
    /// <summary>
    /// Retrieves the prefab for a custom <see cref="InnerNetObject"/> of the specified type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The type of the <see cref="InnerNetObject"/> prefab to retrieve. Must inherit from <see cref="InnerNetObject"/>.</typeparam>
    /// <returns>The prefab instance of type <typeparamref name="T"/>.</returns>
    /// <remarks>
    /// This method searches through the AmongUsClient.Instance.NonAddressableSpawnableObjects array
    /// to find a prefab of the specified type. If no matching prefab is found, an exception is thrown.
    /// </remarks>
    public static InnerNetObject? GetNetObjPrefab<T>() where T : InnerNetObject
    {
        var prefab = AmongUsClient.Instance.NonAddressableSpawnableObjects
            .FirstOrDefault(obj => obj.TryCast<T>() != null);

        return prefab;
    }

    /// <summary>
    /// Spawns a new <see cref="InnerNetObject"/> locally and on the network of type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The type of the <see cref="InnerNetObject"/> to spawn. Must inherit from <see cref="InnerNetObject"/>.</typeparam>
    /// <param name="ownerId">The owner ID for the spawned object. Defaults to -2, which typically means no specific owner.</param>
    /// <param name="spawnFlags">The spawn flags to use when spawning the object. Defaults to <see cref="SpawnFlags.None"/>.</param>
    /// <returns>The newly spawned <see cref="InnerNetObject"/>, if prefab is null then it will return null.</returns>
    public static InnerNetObject? SpawnNewNetObject<T>(int ownerId = -2, SpawnFlags spawnFlags = SpawnFlags.None) where T : InnerNetObject
    {
        var netObj = GetNetObjPrefab<T>();

        if (netObj == null)
        {
            return null;
        }

        var netObjSpawn = UnityEngine.Object.Instantiate(netObj);
        AmongUsClient.Instance.Spawn(netObjSpawn, ownerId, spawnFlags);
        return netObjSpawn;
    }

    /// <summary>
    /// Spawns an existing <see cref="InnerNetObject"/> instance on the network.
    /// </summary>
    /// <param name="netObj">The <see cref="InnerNetObject"/> instance to spawn.</param>
    /// <param name="ownerId">The owner ID for the spawned object. Defaults to -2, which typically means no specific owner.</param>
    /// <param name="spawnFlags">The spawn flags to use when spawning the object. Defaults to <see cref="SpawnFlags.None"/>.</param>
    public static void SpawnNetObject(this InnerNetObject netObj, int ownerId = -2, SpawnFlags spawnFlags = SpawnFlags.None)
    {
        AmongUsClient.Instance.Spawn(netObj, ownerId, spawnFlags);
    }
}
