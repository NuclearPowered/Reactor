using Il2CppInterop.Runtime;
using UnityEngine;

namespace Reactor.Utilities.Extensions;

/// <summary>
/// Provides extension methods for <see cref="AssetBundle"/>.
/// </summary>
public static class AssetBundleExtensions
{
    /// <summary>
    /// Loads an asset with <paramref name="name"/> from the <paramref name="bundle"/> with the specified <typeparamref name="T"/> type.
    /// </summary>
    /// <param name="bundle">The <see cref="AssetBundle"/> to load the asset from.</param>
    /// <param name="name">The name of the asset.</param>
    /// <typeparam name="T">The type of the asset.</typeparam>
    /// <returns>The loaded asset or null if it wasn't found.</returns>
    public static T? LoadAsset<T>(this AssetBundle bundle, string name) where T : Object
    {
        return bundle.LoadAsset(name, Il2CppType.Of<T>())?.Cast<T>();
    }
}
