using System;
using System.IO;
using System.Threading.Tasks;
using DepotDownloader;
using Reactor.Greenhouse.Setup.Provider;

namespace Reactor.Greenhouse.Setup
{
    public class GameManager
    {
        public string WorkPath { get; }

        public Game PreObfuscation { get; }
        public Game Steam { get; }
        public Game Itch { get; }

        public GameManager()
        {
            WorkPath = Path.GetFullPath("work");
            PreObfuscation = new Game(new SteamProvider(true), "preobfuscation", Path.Combine(WorkPath, "preobfuscation"));
            Steam = new Game(new SteamProvider(false), "steam", Path.Combine(WorkPath, "steam"));
            Itch = new Game(new ItchProvider(), "itch", Path.Combine(WorkPath, "itch"));
        }

        public async Task SetupAsync(bool setupSteam, bool setupItch)
        {
            var preObfuscation = PreObfuscation.Provider.IsUpdateNeeded();
            var steam = setupSteam && Steam.Provider.IsUpdateNeeded();
            var itch = setupItch && Itch.Provider.IsUpdateNeeded();

            if (preObfuscation || steam || itch)
            {
                ContentDownloader.ShutdownSteam3();

                if (preObfuscation)
                {
                    await PreObfuscation.DownloadAsync();
                    Console.WriteLine($"Downloaded {nameof(PreObfuscation)} ({PreObfuscation.Version})");
                }

                if (steam)
                {
                    await Steam.DownloadAsync();
                    Console.WriteLine($"Downloaded {nameof(Steam)} ({Steam.Version})");
                }

                if (itch)
                {
                    await Itch.DownloadAsync();
                    Console.WriteLine($"Downloaded {nameof(Itch)} ({Itch.Version})");
                }
            }

            ContentDownloader.ShutdownSteam3();

            PreObfuscation.UpdateVersion();

            if (setupSteam)
            {
                Steam.UpdateVersion();
            }

            if (setupItch)
            {
                Itch.UpdateVersion();
            }

            if (PreObfuscation.Version != "2020.9.9")
            {
                throw new ArgumentException("Pre obfuscation version is invalid");
            }

            PreObfuscation.Dump();

            if (setupSteam)
            {
                Steam.Dump();
            }

            if (setupItch)
            {
                Itch.Dump();
            }
        }
    }
}
