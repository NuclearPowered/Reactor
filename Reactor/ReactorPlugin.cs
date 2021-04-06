using System;
using System.Linq;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.IL2CPP;
using HarmonyLib;
using Reactor.Extensions;
using Reactor.Networking;
using Reactor.Patches;
using Reactor.Unstrip;
using UnhollowerBaseLib.Attributes;
using UnityEngine;

namespace Reactor
{
    [BepInPlugin(Id)]
    [BepInProcess("Among Us.exe")]
    public class ReactorPlugin : BasePlugin
    {
        public const string Id = "gg.reactor.api";

        public Harmony Harmony { get; } = new Harmony(Id);
        public CustomRpcManager CustomRpcManager { get; } = new CustomRpcManager();

        public ConfigEntry<bool> AllowVanillaServers;

        private GameObject _gameObject;
        private RegionInfoWatcher RegionInfoWatcher { get; } = new RegionInfoWatcher();

        public override void Load()
        {
            RegisterInIl2CppAttribute.Register();

            AllowVanillaServers = Config.Bind("Features", "Allow vanilla servers", false, "Whether reactor should ignore servers not responding to modded handshake. This config is ignored if any plugin uses custom rpcs!");

            _gameObject = new GameObject(nameof(ReactorPlugin)).DontDestroy();
            _gameObject.AddComponent<ReactorComponent>().Plugin = this;
            _gameObject.AddComponent<Coroutines.Component>();

            Harmony.PatchAll();
            ReactorVersionShower.Initialize();
            SplashSkip.Initialize();
            DefaultBundle.Load();
        }

        public override bool Unload()
        {
            Harmony.UnpatchSelf();
            _gameObject.Destroy();
            RegionInfoWatcher?.Dispose();

            return base.Unload();
        }

        [RegisterInIl2Cpp]
        public class ReactorComponent : MonoBehaviour
        {
            [HideFromIl2Cpp]
            public ReactorPlugin Plugin { get; internal set; }

            public ReactorComponent(IntPtr ptr) : base(ptr)
            {
            }

            private void Update()
            {
                if (Plugin.RegionInfoWatcher.Reload)
                {
                    Plugin.RegionInfoWatcher.Reload = false;
                    ServerManager.Instance.LoadServers();
                    Plugin.Log.LogInfo("Region file reloaded");
                }

                if (Input.GetKeyDown(KeyCode.F5))
                {
                    Plugin.Log.LogInfo("Reloading all configs");

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
