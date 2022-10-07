using System.Diagnostics;
using BenchmarkDotNet.Running;
using BepInEx;
using BepInEx.Unity.IL2CPP;

namespace Reactor.Benchmarks;

[BepInAutoPlugin("gg.reactor.benchmarks")]
[BepInProcess("Among Us.exe")]
[BepInDependency(ReactorPlugin.Id)]
public partial class BenchmarksPlugin : BasePlugin
{
    public override void Load()
    {
        try
        {
            BenchmarkRunner.Run<AssetBundleBenchmarks>();
        }
        finally
        {
            Process.GetCurrentProcess().Kill();
        }
    }
}
