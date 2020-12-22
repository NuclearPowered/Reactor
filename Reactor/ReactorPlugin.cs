using System;
using BepInEx;
using BepInEx.IL2CPP;
using HarmonyLib;
using Reactor.Extensions;
using Reactor.Patches;
using Reactor.Unstrip;
using UnhollowerRuntimeLib;
using UnityEngine;

namespace Reactor
{
    [BepInPlugin(Id)]
    [BepInProcess("Among Us.exe")]
    [ReactorPluginSide(PluginSide.Client)]
    public class ReactorPlugin : BasePlugin
    {
        public const string Id = "gg.reactor.api";

        public Harmony Harmony { get; } = new Harmony(Id);
        private RegionInfoWatcher RegionInfoWatcher { get; } = new RegionInfoWatcher();

        public override void Load()
        {
            ClassInjector.RegisterTypeInIl2Cpp<ReactorComponent>();
            ClassInjector.RegisterTypeInIl2Cpp<Unstrip.Patches.GUIWordWrapSizer>();

            var gameObject = new GameObject(nameof(ReactorPlugin)).DontDestroy();
            gameObject.AddComponent<ReactorComponent>();

            Harmony.PatchAll();
            ReactorVersionShower.Initialize();
            DefaultBundle.Load();
        }

        public override bool Unload()
        {
            RegionInfoWatcher?.Dispose();

            return base.Unload();
        }

        public class ReactorComponent : MonoBehaviour
        {
            public ReactorComponent(IntPtr ptr) : base(ptr)
            {
            }

            private void Update()
            {
                var watcher = PluginSingleton<ReactorPlugin>.Instance.RegionInfoWatcher;
                if (watcher.Reload)
                {
                    watcher.Reload = false;
                    ServerManager.Instance.LoadServers();
                    PluginSingleton<ReactorPlugin>.Instance.Log.LogInfo("Region file reloaded");
                }
            }
        }
    }
}
