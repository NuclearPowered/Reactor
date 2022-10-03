using Il2CppInterop.Runtime;
using UnityEngine;

namespace Reactor.Resources.Extensions;

public static class AssetBundleExtensions
{
    public static T? LoadAsset<T>(this AssetBundle assetBundle, string name) where T : Object
    {
        return assetBundle.LoadAsset(name, Il2CppType.Of<T>())?.Cast<T>();
    }
}
