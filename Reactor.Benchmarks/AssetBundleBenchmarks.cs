using System;
using System.Collections.Generic;
using System.IO;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using BepInEx;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Reactor.Utilities;
using Reactor.Utilities.Extensions;
using UnityEngine;

namespace Reactor.Benchmarks;

[MemoryDiagnoser]
[StopOnFirstError, Config(typeof(Config))]
public class AssetBundleBenchmarks
{
    private class Config : ManualConfig
    {
        public Config()
        {
            AddJob(Job.ShortRun.WithLaunchCount(1).WithWarmupCount(1).WithIterationCount(10).WithToolchain(SingleThreadedInProcessEmitToolchain.Instance));
        }
    }

    [ParamsSource(nameof(ValuesForAssetBundleName))]
    public string AssetBundleFileName { get; set; } = null!;

    public static IEnumerable<string> ValuesForAssetBundleName()
    {
        yield return $"default-{AssetBundleManager.TargetName}{AssetBundleManager.BundleExtension}";
        // yield return "submerged";
    }

    [IterationSetup]
    public void IterationSetup()
    {
        AssetBundle.UnloadAllAssetBundles(true);
    }

    private AssetBundle LoadAll(AssetBundle assetBundle)
    {
        if (assetBundle == null) throw new ArgumentNullException(nameof(assetBundle));

        assetBundle.LoadAllAssets();
        return assetBundle;
    }

    private Stream GetResourceStream()
    {
        return typeof(ReactorPlugin).Assembly.GetManifestResourceStream("Reactor.Assets." + AssetBundleFileName)!;
    }

    [Benchmark]
    public AssetBundle LoadFromStream()
    {
        using var stream = GetResourceStream();
        return LoadAll(AssetBundle.LoadFromStream(stream.AsIl2Cpp()));
    }

    [Benchmark]
    public AssetBundle LoadFromMemory_ReadFully()
    {
        using var stream = GetResourceStream();
        return LoadAll(AssetBundle.LoadFromMemory(stream.ReadFully()));
    }

    [Benchmark]
    public unsafe AssetBundle LoadFromMemory_ReadFullyFastCopy()
    {
        using var stream = GetResourceStream();
        var array = stream.ReadFully();
        var il2CppArray = new Il2CppStructArray<byte>(array.Length);
        fixed (byte* arrayPtr = array) { Buffer.MemoryCopy(arrayPtr, IntPtr.Add(il2CppArray.Pointer, 4 * IntPtr.Size).ToPointer(), il2CppArray.Length, array.Length); }

        return LoadAll(AssetBundle.LoadFromMemory(il2CppArray));
    }

    [Benchmark]
    public AssetBundle LoadFromMemory_Span()
    {
        using var stream = GetResourceStream();
        var length = (int) stream.Length;

        var array = new Il2CppStructArray<byte>(length);
        if (stream.Read(array.ToSpan()) < length) throw new Exception("Failed to read in full");

        return LoadAll(AssetBundle.LoadFromMemory(array));
    }

    [Benchmark]
    public AssetBundle LoadFromFile()
    {
        return LoadAll(AssetBundle.LoadFromFile(Path.Combine(Paths.PluginPath, AssetBundleFileName)));
    }
}
