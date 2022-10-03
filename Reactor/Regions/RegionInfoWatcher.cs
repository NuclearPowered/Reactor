using System;
using System.IO;
using Reactor.Utilities;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Reactor.Regions;

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

                    Info("Region file reloaded");
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
