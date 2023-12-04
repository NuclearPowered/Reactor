global using static Reactor.Utilities.Logger<Reactor.Debugger.DebuggerPlugin>;
using BepInEx;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using Reactor.Debugger.AutoJoin;
using Reactor.Debugger.Patches;
using Reactor.Debugger.Window;

namespace Reactor.Debugger;

[BepInAutoPlugin("gg.reactor.debugger")]
[BepInProcess("Among Us.exe")]
[BepInDependency(ReactorPlugin.Id)]
public partial class DebuggerPlugin : BasePlugin
{
    public Harmony Harmony { get; } = new(Id);

    public override void Load()
    {
        DebuggerConfig.Bind(Config);

        this.AddComponent<DebuggerWindow>();

        GameOptionsPatches.Initialize();

        Harmony.PatchAll();

        AutoJoinConnectionManager.StartOrConnect();
    }
}
