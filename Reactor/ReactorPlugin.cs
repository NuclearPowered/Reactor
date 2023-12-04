global using static Reactor.Utilities.Logger<Reactor.ReactorPlugin>;
using System;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using Il2CppInterop.Runtime.Attributes;
using Reactor.Localization;
using Reactor.Localization.Providers;
using Reactor.Networking;
using Reactor.Networking.Attributes;
using Reactor.Networking.Rpc;
using Reactor.Patches;
using Reactor.Patches.Miscellaneous;
using Reactor.Utilities;
using Reactor.Utilities.Attributes;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Reactor;

/// <summary>
/// Reactor's main class.
/// </summary>
[BepInAutoPlugin("gg.reactor.api")]
[BepInProcess("Among Us.exe")]
public partial class ReactorPlugin : BasePlugin
{
    /// <summary>
    /// Gets harmony instance.
    /// </summary>
    public Harmony Harmony { get; } = new(Id);

    /// <summary>
    /// Gets custom rpc manager.
    /// </summary>
    public CustomRpcManager CustomRpcManager { get; } = new();

    internal ConfigEntry<bool>? AllowVanillaServers { get; private set; }

    internal RegionInfoWatcher RegionInfoWatcher { get; } = new();

    /// <inheritdoc />
    public ReactorPlugin()
    {
        Log.LogMessage($"Among Us {Application.version} {Constants.GetCurrentPlatformName()}");

        PluginSingleton<ReactorPlugin>.Instance = this;
        PluginSingleton<BasePlugin>.Initialize();

        RegisterInIl2CppAttribute.Initialize();
        ModList.Initialize();

        RegisterCustomRpcAttribute.Initialize();
        MessageConverterAttribute.Initialize();
        MethodRpcAttribute.Initialize();

        LocalizationManager.Register(new HardCodedLocalizationProvider());
    }

    /// <inheritdoc />
    public override void Load()
    {
        AllowVanillaServers = Config.Bind("Features", "Allow vanilla servers", false, "Whether reactor should ignore servers not responding to modded handshake. This config is ignored if any plugin uses custom rpcs!");

        Harmony.PatchAll();

        this.AddComponent<ReactorComponent>().Plugin = this;
        this.AddComponent<Coroutines.Component>();
        this.AddComponent<Dispatcher>();

        ReactorVersionShower.Initialize();
        FreeNamePatch.Initialize();
        DefaultBundle.Load();

        SceneManager.add_sceneLoaded((Action<Scene, LoadSceneMode>) ((scene, _) =>
        {
            if (scene.name == "MainMenu")
            {
                ModManager.Instance.ShowModStamp();
            }
        }));
    }

    /// <inheritdoc />
    public override bool Unload()
    {
        Harmony.UnpatchSelf();
        RegionInfoWatcher.Dispose();

        return base.Unload();
    }

    [RegisterInIl2Cpp]
    private sealed class ReactorComponent : MonoBehaviour
    {
        [HideFromIl2Cpp]
        public ReactorPlugin? Plugin { get; internal set; }

        public ReactorComponent(IntPtr ptr) : base(ptr)
        {
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.F5))
            {
                Plugin!.Log.LogInfo("Reloading all configs");

                foreach (var pluginInfo in IL2CPPChainloader.Instance.Plugins.Values)
                {
                    var config = ((BasePlugin) pluginInfo.Instance).Config;
                    if (config.Count == 0)
                    {
                        continue;
                    }

                    try
                    {
                        config.Reload();
                    }
                    catch (Exception e)
                    {
                        Plugin.Log.LogWarning($"Exception occured during reload of {pluginInfo.Metadata.Name}: {e}");
                    }
                }
            }
        }
    }
}
