#if UNITY_EDITOR
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using UnityEditor;
using Debug = UnityEngine.Debug;

internal static class Build
{
    public static BuildTarget[] Targets { get; } =
    {
        BuildTarget.StandaloneWindows,
        BuildTarget.Android,
    };

    public static Compression Compression => Compression.LZMA;

    public static string OutputPath { get; } = Path.Combine("..", "Reactor", "Assets");

    public static string AssetBundlesPath { get; } = Path.Combine("Assets", "AssetBundles");
    public static string TempPath { get; } = Path.Combine("Temp", "AssetBundles");

    [MenuItem("File/Build Asset Bundles")]
    public static void BuildAssetBundles()
    {
        Directory.CreateDirectory(TempPath);

        var ignoredExtensions = new[] { ".meta", ".manifest", ".dll", ".cs", ".exe", ".tps" };

        var builds = Directory.GetDirectories(AssetBundlesPath)
            .Select(directoryPath => new AssetBundleBuild
            {
                assetBundleName = Path.GetFileName(directoryPath),
                assetNames = Directory.GetFiles(directoryPath, "*", SearchOption.AllDirectories).Where(p => !ignoredExtensions.Contains(Path.GetExtension(p))).ToArray(),
            })
            .ToArray();

        Debug.Log($"Building {builds.Length} asset bundle(s) ({string.Join(", ", builds.Select(b => b.assetBundleName))})");

        var options = BuildAssetBundleOptions.StrictMode;

        switch (Compression)
        {
            case Compression.None:
                options |= BuildAssetBundleOptions.UncompressedAssetBundle;
                break;

            case Compression.LZ4:
                options |= BuildAssetBundleOptions.ChunkBasedCompression;
                break;
        }

        foreach (var target in Targets)
        {
            var tempPath = Path.Combine(TempPath, target.ToString());
            Directory.CreateDirectory(tempPath);

            var stopwatch = Stopwatch.StartNew();
            var manifest = BuildPipeline.BuildAssetBundles(tempPath, builds, options, target);
            Debug.Log($"Built asset bundle(s) for {target} in {stopwatch.Elapsed}");

            foreach (var assetBundleName in manifest.GetAllAssetBundles())
            {
                var destinationPath = Path.Combine(OutputPath, $"{assetBundleName}-{GetTargetName(target)}.bundle");
                File.Delete(destinationPath);
                File.Copy(Path.Combine(tempPath, assetBundleName), destinationPath);
            }
        }
    }

    private static string GetTargetName(BuildTarget target, bool includeArchitecture = false)
    {
        return target switch
        {
            BuildTarget.StandaloneWindows => includeArchitecture ? "win-x86" : "win",
            BuildTarget.StandaloneWindows64 => includeArchitecture ? "win-x64" : "win",
            BuildTarget.Android => "android",
            BuildTarget.StandaloneLinux64 => includeArchitecture ? "linux-x64" : "linux",
            _ => throw new ArgumentOutOfRangeException(nameof(target), target, null),
        };
    }
}

internal enum Compression
{
    /// <summary>
    /// Worst size, best load speed
    /// </summary>
    None,

    /// <summary>
    /// Best size, worst load speed
    /// </summary>
    LZMA,

    /// <summary>
    /// Okay size, okay load speed
    /// </summary>
    LZ4,
}
#endif
