using System;
using System.IO;
using Reactor.Extensions;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Reactor.Patches
{
    internal class RegionInfoWatcher : IDisposable
    {
        private FileSystemWatcher Watcher { get; }

        internal RegionInfoWatcher()
        {
            Watcher = new FileSystemWatcher(
                string.IsNullOrEmpty(Application.persistentDataPath) ? Directory.GetCurrentDirectory() : Application.persistentDataPath,
                "regionInfo.json"
            );

            Watcher.Changed += (s, e) =>
            {
                if (new FileInfo(e.Name).Length > 0)
                {
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
    }
}
