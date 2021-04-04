using System.IO;
using System.Threading.Tasks;
using AssemblyUnhollower;
using Cake.Common;
using Cake.Common.Diagnostics;
using Cake.Common.IO;
using Cake.Common.Net;
using Cake.Common.Tools.DotNetCore;
using Cake.Common.Tools.DotNetCore.Build;
using Cake.Core;
using Cake.Frosting;
using DepotDownloader;

public static class Program
{
    public static int Main(string[] args)
    {
        return new CakeHost()
            .UseContext<BuildContext>()
            .Run(args);
    }
}

public class BuildContext : FrostingContext
{
    public string TempPath { get; } = "bin/temp";
    public string AmongUsPath { get; } = "bin/Among Us";

    public BuildContext(ICakeContext context) : base(context)
    {
        context.CreateDirectory(TempPath);
        context.CreateDirectory(AmongUsPath);
    }
}

[TaskName("SetupAmongUs")]
public sealed class SetupAmongUsTask : AsyncFrostingTask<BuildContext>
{
    public const uint AppId = 945360;
    public const uint DepotId = 945361;
    public const ulong ManifestId = 3941730972865408291; // 2021.3.31.3s

    public override async Task RunAsync(BuildContext context)
    {
        AccountSettingsStore.LoadFromFile("account.config");
        var steam = context.EnvironmentVariable<string>("STEAM", null).Split(":");

        context.Information("Logging into steam");
        ContentDownloader.InitializeSteam3(steam[0], steam[1]);

        ContentDownloader.Config.InstallDirectory = context.AmongUsPath;

        context.Information("Downloading the game from steam");
        await ContentDownloader.DownloadAppAsync(AppId, DepotId, ManifestId);
        ContentDownloader.ShutdownSteam3();

        var bepinexZip = context.DownloadFile("https://github.com/NuclearPowered/BepInEx/releases/download/6.0.0-reactor.18%2Bstructfix/BepInEx-6.0.0-reactor.18+structfix.zip");
        context.Unzip(bepinexZip, Path.Combine(context.AmongUsPath));
    }
}

[TaskName("GenerateProxyAssembly")]
[IsDependentOn(typeof(SetupAmongUsTask))]
public sealed class GenerateProxyAssemblyTask : FrostingTask<BuildContext>
{
    public override void Run(BuildContext context)
    {
        var dumperConfig = new Il2CppDumper.Config
        {
            GenerateStruct = false,
            GenerateDummyDll = true
        };

        context.Information("Generating Il2CppDumper intermediate assemblies");

        var gameAssemblyPath = Path.Combine(context.AmongUsPath, "GameAssembly.dll");

        Il2CppDumper.Il2CppDumper.PerformDump(
            gameAssemblyPath,
            Path.Combine(context.AmongUsPath, "Among Us_Data", "il2cpp_data", "Metadata", "global-metadata.dat"),
            context.TempPath, dumperConfig, context.Debug
        );

        context.Information("Executing Il2CppUnhollower generator");

        UnhollowerBaseLib.LogSupport.InfoHandler += context.Information;
        UnhollowerBaseLib.LogSupport.WarningHandler += context.Warning;
        UnhollowerBaseLib.LogSupport.TraceHandler += context.Debug;
        UnhollowerBaseLib.LogSupport.ErrorHandler += context.Error;

        var unhollowerOptions = new UnhollowerOptions
        {
            GameAssemblyPath = gameAssemblyPath,
            MscorlibPath = Path.Combine(context.AmongUsPath, "mono", "Managed", "mscorlib.dll"),
            SourceDir = Path.Combine(context.TempPath, "DummyDll"),
            OutputDir = Path.Combine(context.AmongUsPath, "BepInEx", "unhollowed"),
            UnityBaseLibsDir = Path.Combine(context.AmongUsPath, "BepInEx", "unity-libs"),
            NoCopyUnhollowerLibs = true
        };

        AssemblyUnhollower.Program.Main(unhollowerOptions);
    }
}

[TaskName("Clean")]
public sealed class CleanTask : FrostingTask<BuildContext>
{
    public override void Run(BuildContext context)
    {
        context.CleanDirectory("bin");
    }
}

[TaskName("Build")]
[IsDependentOn(typeof(CleanTask))]
[IsDependentOn(typeof(GenerateProxyAssemblyTask))]
public sealed class BuildTask : FrostingTask<BuildContext>
{
    private void Build(BuildContext context, string project)
    {
        var buildId = context.EnvironmentVariable<string>("GITHUB_RUN_NUMBER", null);

        context.DotNetCoreBuild(project, new DotNetCoreBuildSettings
        {
            Configuration = "Release",
            VersionSuffix = buildId == null ? "dev" : "ci." + buildId,
            EnvironmentVariables =
            {
                ["GamePlatform"] = "Steam",
                ["AmongUs"] = Path.GetFullPath(context.AmongUsPath)
            }
        });
    }

    public override void Run(BuildContext context)
    {
        Build(context, "Reactor/Reactor.csproj");
        Build(context, "Reactor.Debugger/Reactor.Debugger.csproj");
    }
}

[IsDependentOn(typeof(BuildTask))]
public sealed class Default : FrostingTask<BuildContext>
{
}
