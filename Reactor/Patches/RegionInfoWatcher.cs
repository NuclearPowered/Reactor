using System;
using System.IO;
using UnityEngine;

namespace Reactor.Patches
{
    internal class RegionInfoWatcher : IDisposable
    {
        private FileSystemWatcher Watcher { get; }
        public bool Reload { get; set; }

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
                    Reload = true;
                }
            };

            Watcher.EnableRaisingEvents = true;
        }

        public void Dispose()
        {
            Watcher?.Dispose();
        }
    }
}
