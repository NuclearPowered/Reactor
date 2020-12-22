// https://github.com/sinai-dev/UnityExplorer/blob/master/src/Unstrip/AssetBundleUnstrip.cs

using System;
using UnhollowerBaseLib;
using UnhollowerRuntimeLib;

namespace Reactor.Unstrip
{
    public class AssetBundle
    {
        private delegate IntPtr d_LoadFromFile(IntPtr path, uint crc, ulong offset);

        private static readonly d_LoadFromFile i_LoadFromFile = IL2CPP.ResolveICall<d_LoadFromFile>("UnityEngine.AssetBundle::LoadFromFile_Internal");

        public static AssetBundle LoadFromFile(string path, uint crc = 0, ulong offset = 0)
        {
            return new AssetBundle(i_LoadFromFile.Invoke(IL2CPP.ManagedStringToIl2Cpp(path), crc, offset));
        }

        private delegate IntPtr d_LoadFromMemory(IntPtr binary, uint crc);

        private static readonly d_LoadFromMemory i_LoadFromMemory = IL2CPP.ResolveICall<d_LoadFromMemory>("UnityEngine.AssetBundle::LoadFromMemory_Internal");

        public static AssetBundle LoadFromMemory(byte[] binary, uint crc = 0)
        {
            return new AssetBundle(i_LoadFromMemory(((Il2CppStructArray<byte>) binary).Pointer, crc));
        }

        public IntPtr Pointer { get; }

        public AssetBundle(IntPtr ptr)
        {
            Pointer = ptr;
        }

        private delegate IntPtr d_LoadAssetWithSubAssets_Internal(IntPtr __instance, IntPtr name, IntPtr type);

        private static readonly d_LoadAssetWithSubAssets_Internal i_LoadAssetWithSubAssets_Internal = IL2CPP.ResolveICall<d_LoadAssetWithSubAssets_Internal>("UnityEngine.AssetBundle::LoadAssetWithSubAssets_Internal");

        public UnityEngine.Object[] LoadAllAssets()
        {
            var ptr = i_LoadAssetWithSubAssets_Internal.Invoke(Pointer, IL2CPP.ManagedStringToIl2Cpp(string.Empty), Il2CppType.Of<UnityEngine.Object>().Pointer);

            if (ptr == IntPtr.Zero)
            {
                return new UnityEngine.Object[0];
            }

            return new Il2CppReferenceArray<UnityEngine.Object>(ptr);
        }

        private delegate IntPtr d_LoadAsset_Internal(IntPtr __instance, IntPtr name, IntPtr type);

        private static readonly d_LoadAsset_Internal i_LoadAsset_Internal = IL2CPP.ResolveICall<d_LoadAsset_Internal>("UnityEngine.AssetBundle::LoadAsset_Internal");

        public T LoadAsset<T>(string name) where T : UnityEngine.Object
        {
            var ptr = i_LoadAsset_Internal.Invoke(Pointer, IL2CPP.ManagedStringToIl2Cpp(name), Il2CppType.Of<T>().Pointer);

            return ptr == IntPtr.Zero ? null : new UnityEngine.Object(ptr).TryCast<T>();
        }
    }
}
