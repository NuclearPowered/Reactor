using System;
using System.Linq;
using BepInEx;
using BepInEx.IL2CPP;
using HarmonyLib;
using Reactor.Extensions;
using Reactor.Patches;
using Reactor.Unstrip;
using UnhollowerBaseLib.Attributes;
using UnityEngine;

namespace Reactor
{
    [BepInPlugin(Id)]
    [BepInProcess("Among Us.exe")]
    [ReactorPluginSide(PluginSide.ClientOnly)]
    public class ReactorPlugin : BasePlugin
    {
        public const string Id = "gg.reactor.api";

        public Harmony Harmony { get; } = new Harmony(Id);
        private RegionInfoWatcher RegionInfoWatcher { get; } = new RegionInfoWatcher();

        public override void Load()
        {
            RegisterInIl2CppAttribute.Register();

            var gameObject = new GameObject(nameof(ReactorPlugin)).DontDestroy();
            gameObject.AddComponent<ReactorComponent>().Plugin = this;

            Harmony.PatchAll();
            ReactorVersionShower.Initialize();
            DefaultBundle.Load();
        }

        public override bool Unload()
        {
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

            public void Start()
            {
                Camera.onPostRender = Camera.onPostRender == null
                    ? new Action<Camera>(OnPostRenderM)
                    : Il2CppSystem.Delegate.Combine(Camera.onPostRender, Il2CppSystem.Delegate.CreateDelegate(GetIl2CppType(), GetIl2CppType().GetMethod(nameof(OnPostRenderM), Il2CppSystem.Reflection.BindingFlags.Static | Il2CppSystem.Reflection.BindingFlags.Public))).Cast<Camera.CameraCallback>();
            }

            public static Camera OnPostRenderCam { get; private set; }

            private static void OnPostRenderM(Camera camera)
            {
                if (OnPostRenderCam == null)
                {
                    OnPostRenderCam = camera;
                }

                if (OnPostRenderCam == camera)
                {
                    Coroutines.ProcessWaitForEndOfFrame();
                }
            }

            private void Update()
            {
                Coroutines.Process();

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

            private void FixedUpdate()
            {
                Coroutines.ProcessWaitForFixedUpdate();
            }
        }
    }
}
