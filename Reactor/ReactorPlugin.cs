using System;
using System.Linq;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.IL2CPP;
using HarmonyLib;
using Reactor.Extensions;
using Reactor.Networking;
using Reactor.Networking.MethodRpc;
using Reactor.Networking.Serialization;
using Reactor.Patches;
using Reactor.Unstrip;
using UnhollowerBaseLib.Attributes;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Reactor
{
    [BepInAutoPlugin("gg.reactor.api")]
    [BepInProcess("Among Us.exe")]
    public partial class ReactorPlugin : BasePlugin
    {
        public Harmony Harmony { get; } = new Harmony(Id);
        public CustomRpcManager CustomRpcManager { get; } = new CustomRpcManager();

        public ConfigEntry<bool>? AllowVanillaServers { get; private set; }

        private RegionInfoWatcher RegionInfoWatcher { get; } = new RegionInfoWatcher();

        public ReactorPlugin()
        {
            PluginSingleton<BasePlugin>.Initialize();
            RegisterInIl2CppAttribute.Initialize();
            RegisterCustomRpcAttribute.Initialize();
            MessageConverterAttribute.Initialize();
            MethodRpcAttribute.Initialize();

            ChainloaderHooks.OnPluginLoad(this);
        }

        public override void Load()
        {
            AllowVanillaServers = Config.Bind("Features", "Allow vanilla servers", false, "Whether reactor should ignore servers not responding to modded handshake. This config is ignored if any plugin uses custom rpcs!");

            Harmony.PatchAll();

            this.AddComponent<ReactorComponent>().Plugin = this;
            this.AddComponent<Coroutines.Component>();
            this.AddComponent<Dispatcher>();

            ReactorVersionShower.Initialize();
            FreeNamePatch.Initialize();
            SplashSkip.Initialize();
            DefaultBundle.Load();

            SceneManager.add_sceneLoaded((Action<Scene, LoadSceneMode>) ((scene, _) =>
            {
                if (scene.name == "MainMenu")
                {
                    ModManager.Instance.ShowModStamp();
                }
            }));
        }

        public override bool Unload()
        {
            Harmony.UnpatchSelf();
            RegionInfoWatcher?.Dispose();

            return base.Unload();
        }

        [RegisterInIl2Cpp]
        public class ReactorComponent : MonoBehaviour
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
                        if (!config.Any())
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
}
