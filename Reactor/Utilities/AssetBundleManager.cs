using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using MonoMod.Utils;
using Reactor.Utilities.Extensions;
using UnityEngine;

namespace Reactor.Utilities;

/// <summary>
/// Provides a standard way of loading asset bundles from a file or embedded resource.
/// </summary>
public static class AssetBundleManager
{
    /// <summary>
    /// Extension of a asset bundle.
    /// </summary>
    public const string BundleExtension = ".bundle";

    /// <summary>
    /// Gets the target name of the current system.
    /// </summary>
    public static string TargetName { get; }

    static AssetBundleManager()
    {
        if (PlatformHelper.Is(Platform.Android))
        {
            TargetName = "android";
        }
        else
        {
            if (PlatformHelper.Is(Platform.Windows))
            {
                TargetName = "win";
            }
            else if (PlatformHelper.Is(Platform.Linux))
            {
                TargetName = "linux";
            }
            else
            {
                throw new PlatformNotSupportedException();
            }

            if (PlatformHelper.Is(Platform.ARM))
            {
                TargetName += Environment.Is64BitProcess ? "-arm64" : "-arm";
            }
            else
            {
                TargetName += Environment.Is64BitProcess ? "-x64" : "-x86";
            }
        }
    }

    /// <summary>
    /// Gets the file name for an asset bundle named <paramref name="assetBundleName"/>.
    /// </summary>
    /// <param name="assetBundleName">The name of the asset bundle.</param>
    /// <returns>File name of the asset bundle.</returns>
    public static string GetFileName(string assetBundleName) => $"{assetBundleName}-{TargetName}{BundleExtension}";

    private static bool TryFindFile(Assembly assembly, string fileName, [NotNullWhen(true)] out string? path)
    {
        var pluginDirectoryPath = Path.GetDirectoryName(assembly.Location);
        if (pluginDirectoryPath != null)
        {
            var filePath = Path.Combine(pluginDirectoryPath, fileName);
            if (File.Exists(filePath))
            {
                path = filePath;
                return true;
            }
        }

        path = null;
        return false;
    }

    private static bool TryLoadResource(Assembly assembly, string fileName, [NotNullWhen(true)] out Il2CppStructArray<byte>? data)
    {
        var resourceName = assembly.GetManifestResourceNames().SingleOrDefault(n => n.EndsWith(fileName, StringComparison.Ordinal));
        if (resourceName != null)
        {
            using var stream = assembly.GetManifestResourceStream(resourceName) ?? throw new InvalidOperationException("Resource stream was null");

            var length = (int) stream.Length;
            data = new Il2CppStructArray<byte>(length);
            if (stream.Read(data.ToSpan()) < length) throw new IOException("Failed to read in full");

            return true;
        }

        data = null;
        return false;
    }

    /// <summary>
    /// Synchronously loads an <see cref="AssetBundle"/> named <paramref name="name"/> from the calling assembly.
    /// </summary>
    /// <param name="name">The name of the asset bundle.</param>
    /// <returns>The loaded assetbundle.</returns>
    /// <exception cref="AssetBundleNotFoundException">Couldn't find an assetbundle named <paramref name="name"/>.</exception>
    public static AssetBundle Load(string name)
    {
        return Load(Assembly.GetCallingAssembly(), name);
    }

    /// <summary>
    /// Synchronously loads an <see cref="AssetBundle"/> named <paramref name="name"/> from the specified <paramref name="assembly"/>.
    /// </summary>
    /// <param name="assembly">The specified assembly.</param>
    /// <param name="name">The name of the asset bundle.</param>
    /// <returns>The loaded assetbundle.</returns>
    /// <exception cref="AssetBundleNotFoundException">Couldn't find an assetbundle named <paramref name="name"/>.</exception>
    public static AssetBundle Load(Assembly assembly, string name)
    {
        var fileName = GetFileName(name);

        if (TryFindFile(assembly, fileName, out var filePath))
        {
            return AssetBundle.LoadFromFile(filePath);
        }

        if (TryLoadResource(assembly, fileName, out var data))
        {
            return AssetBundle.LoadFromMemory(data);
        }

        throw new AssetBundleNotFoundException(name);
    }

    /// <summary>
    /// Asynchronously loads an <see cref="AssetBundle"/> named <paramref name="name"/> from the calling assembly.
    /// </summary>
    /// <param name="name">The name of the asset bundle.</param>
    /// <returns>The loaded assetbundle.</returns>
    /// <exception cref="AssetBundleNotFoundException">Couldn't find an assetbundle named <paramref name="name"/>.</exception>
    public static AssetBundleCreateRequest LoadAsync(string name)
    {
        return LoadAsync(Assembly.GetCallingAssembly(), name);
    }

    /// <summary>
    /// Asynchronously loads an <see cref="AssetBundle"/> named <paramref name="name"/> from the specified <paramref name="assembly"/>.
    /// </summary>
    /// <param name="assembly">The specified assembly.</param>
    /// <param name="name">The name of the asset bundle.</param>
    /// <returns>The loaded assetbundle.</returns>
    /// <exception cref="AssetBundleNotFoundException">Couldn't find an assetbundle named <paramref name="name"/>.</exception>
    public static AssetBundleCreateRequest LoadAsync(Assembly assembly, string name)
    {
        var fileName = GetFileName(name);

        if (TryFindFile(assembly, fileName, out var filePath))
        {
            return AssetBundle.LoadFromFileAsync(filePath);
        }

        if (TryLoadResource(assembly, fileName, out var data))
        {
            return AssetBundle.LoadFromMemoryAsync(data);
        }

        throw new AssetBundleNotFoundException(name);
    }
}

/// <summary>
/// The exception that is thrown when an assetbundle is not found.
/// </summary>
public class AssetBundleNotFoundException : IOException
{
    internal AssetBundleNotFoundException(string name) : base("Couldn't find an assetbundle named " + name)
    {
    }
}
