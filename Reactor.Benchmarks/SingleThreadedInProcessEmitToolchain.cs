using BenchmarkDotNet.Engines;
using BenchmarkDotNet.Loggers;
using BenchmarkDotNet.Toolchains;
using BenchmarkDotNet.Toolchains.InProcess;
using BenchmarkDotNet.Toolchains.InProcess.Emit;
using BenchmarkDotNet.Toolchains.InProcess.Emit.Implementation;
using BenchmarkDotNet.Toolchains.Parameters;
using BenchmarkDotNet.Toolchains.Results;
using HarmonyLib;

namespace Reactor.Benchmarks;

public class SingleThreadedInProcessEmitToolchain : Toolchain
{
    public static readonly IToolchain Instance = new SingleThreadedInProcessEmitToolchain(true);

    public SingleThreadedInProcessEmitToolchain(bool logOutput)
        : base(nameof(SingleThreadedInProcessEmitToolchain), new InProcessEmitGenerator(), new InProcessEmitBuilder(), new SingleThreadedInProcessEmitExecutor(logOutput))
    {
    }
}

public class SingleThreadedInProcessEmitExecutor : IExecutor
{
    /// <summary>Initializes a new instance of the <see cref="SingleThreadedInProcessEmitExecutor" /> class.</summary>
    /// <param name="logOutput"><c>true</c> if the output should be logged.</param>
    public SingleThreadedInProcessEmitExecutor(bool logOutput)
    {
        LogOutput = logOutput;
    }

    /// <summary>Gets a value indicating whether the output should be logged.</summary>
    /// <value><c>true</c> if the output should be logged; otherwise, <c>false</c>.</value>
    public bool LogOutput { get; }

    /// <summary>Executes the specified benchmark.</summary>
    public ExecuteResult Execute(ExecuteParameters executeParameters)
    {
        var hostLogger = LogOutput ? executeParameters.Logger : NullLogger.Instance;
        var host = new InProcessHost(executeParameters.BenchmarkCase, hostLogger, executeParameters.Diagnoser);

        var exitCode = ExecuteCore(host, executeParameters);
        return ((ExecuteResult) AccessTools.Method(typeof(ExecuteResult), "FromRunResults").Invoke(null, new object[] { host.RunResults, exitCode })!)!;
    }

    private int ExecuteCore(IHost host, ExecuteParameters parameters)
    {
        var generatedAssembly = ((InProcessEmitArtifactsPath) parameters.BuildResult.ArtifactsPaths).GeneratedAssembly;

        return RunnableProgram.Run(parameters.BenchmarkId, generatedAssembly, parameters.BenchmarkCase, host);
    }
}
