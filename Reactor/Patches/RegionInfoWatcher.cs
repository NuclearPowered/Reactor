using System;
using System.IO;
using System.Reflection;
using HarmonyLib;
using Reactor.Extensions;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Reactor.Patches;

internal class RegionInfoWatcher : IDisposable
{
    private FileSystemWatcher Watcher { get; }

    public bool IgnoreNext { get; set; }

    internal RegionInfoWatcher()
    {
        Watcher = new FileSystemWatcher(
            string.IsNullOrEmpty(Application.persistentDataPath) ? Directory.GetCurrentDirectory() : Application.persistentDataPath,
            "regionInfo.json"
        );

        Watcher.Changed += (_, e) =>
        {
            var fileInfo = new FileInfo(e.FullPath);
            if (fileInfo.Exists && fileInfo.Length > 0)
            {
                if (IgnoreNext)
                {
                    IgnoreNext = false;
                    return;
                }

                Dispatcher.Instance.Enqueue(() =>
                {
                    ServerManager.Instance.LoadServers();

                    Object.FindObjectOfType<RegionTextMonitor>()?.Start();
                    if (Object.FindObjectOfType<RegionMenu>() is { } regionMenu)
                    {
                        regionMenu.OnDisable();
                        regionMenu.OnEnable();
                    }

                    Logger<ReactorPlugin>.Info("Region file reloaded");
                });
            }
        };

        Watcher.EnableRaisingEvents = true;
    }

    public void Dispose()
    {
        Watcher.Dispose();
    }

    [HarmonyPatch]
    private static class WritePatch
    {
        public static MethodBase TargetMethod()
        {
            var type = Type.GetType("FileIO, Assembly-CSharp", false) ?? Type.GetType("Innersloth.IO.FileIO, Assembly-CSharp", true);
            return AccessTools.Method(type, "WriteAllText");
        }

        public static bool Prefix(string path, string contents)
        {
            // Among Us' region loading code unfortunately contains a call
            // to SaveServers, which will write out the region file. This
            // will lead to a positive feedback loop when detecting writes,
            // which is undesireable. So we check if the write makes a
            // change to the file on disk and if it would write the same
            // file again, stop AU from actually writing it.
            if (ServerManager.Instance && path == ServerManager.Instance.serverInfoFileJson)
            {
                var continueWrite = !File.Exists(path) || File.ReadAllText(path) != contents;
                Logger<ReactorPlugin>.Debug($"Continue serverInfoFile write? {continueWrite}");
                // If we will write, ignore the next change action from the observer.
                PluginSingleton<ReactorPlugin>.Instance.RegionInfoWatcher.IgnoreNext = continueWrite;
                return continueWrite;
            }

            return true;
        }
    }
}
