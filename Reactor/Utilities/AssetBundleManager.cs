using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
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
    [Obsolete("Use AssetBundleManager#GetTargetName instead")]
    public static string TargetName { get; } = GetTargetName(true);

    /// <summary>
    /// Gets the target name of the current system.
    /// </summary>
    /// <param name="includeArchitecture">A value indicating whether to include the process architecture.</param>
    /// <returns>Target name of the current system.</returns>
    public static string GetTargetName(bool includeArchitecture)
    {
        var operatingSystem = Application.platform switch
        {
            RuntimePlatform.WindowsPlayer or RuntimePlatform.WSAPlayerX86 => "win",
            RuntimePlatform.LinuxPlayer or RuntimePlatform.OSXPlayer => "linux",
            RuntimePlatform.Android => "android",
            _ => throw new PlatformNotSupportedException(),
        };

        if (!includeArchitecture)
        {
            return operatingSystem;
        }

        return $"{operatingSystem}-{RuntimeInformation.ProcessArchitecture switch
        {
            Architecture.X86 => "x86",
            Architecture.X64 => "x64",
            Architecture.Arm => "arm",
            Architecture.Arm64 => "arm64",
            _ => throw new PlatformNotSupportedException(),
        }}";
    }

    /// <summary>
    /// Gets the file name for an asset bundle named <paramref name="assetBundleName"/>.
    /// </summary>
    /// <param name="assetBundleName">The name of the asset bundle.</param>
    /// <returns>File name of the asset bundle.</returns>
    [Obsolete("Use the overload with includeArchitecture instead")]
    public static string GetFileName(string assetBundleName) => GetFileName(assetBundleName, true);

    /// <summary>
    /// Gets the file name for an asset bundle named <paramref name="assetBundleName"/>.
    /// </summary>
    /// <param name="assetBundleName">The name of the asset bundle.</param>
    /// <param name="includeArchitecture">A value indicating whether to include the process architecture.</param>
    /// <returns>File name of the asset bundle.</returns>
    public static string GetFileName(string assetBundleName, bool includeArchitecture) => $"{assetBundleName}-{GetTargetName(includeArchitecture)}{BundleExtension}";

    private static bool TryFindFile(Assembly assembly, string fileName, [NotNullWhen(true)] out string? path)
    {
        var pluginDirectoryPath = Path.GetDirectoryName(assembly.Location);
        if (pluginDirectoryPath != null)
        {
            var filePath = Path.Combine(pluginDirectoryPath, fileName);
            if (File.Exists(filePath))
            {
                Debug($"Loading an asset bundle from {filePath}");
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
            Debug($"Loading an asset bundle from {resourceName}");

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
        return TryLoad(assembly, GetFileName(name, includeArchitecture: true))
               ?? TryLoad(assembly, GetFileName(name, includeArchitecture: false))
               ?? throw new AssetBundleNotFoundException(name);
    }

    private static AssetBundle? TryLoad(Assembly assembly, string fileName)
    {
        if (TryFindFile(assembly, fileName, out var filePath))
        {
            return AssetBundle.LoadFromFile(filePath);
        }

        if (TryLoadResource(assembly, fileName, out var data))
        {
            return AssetBundle.LoadFromMemory(data);
        }

        return null;
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
        return TryLoadAsync(assembly, GetFileName(name, includeArchitecture: true))
               ?? TryLoadAsync(assembly, GetFileName(name, includeArchitecture: false))
               ?? throw new AssetBundleNotFoundException(name);
    }

    private static AssetBundleCreateRequest? TryLoadAsync(Assembly assembly, string fileName)
    {
        if (TryFindFile(assembly, fileName, out var filePath))
        {
            return AssetBundle.LoadFromFileAsync(filePath);
        }

        if (TryLoadResource(assembly, fileName, out var data))
        {
            return AssetBundle.LoadFromMemoryAsync(data);
        }

        return null;
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
