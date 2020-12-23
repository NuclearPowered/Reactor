using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DepotDownloader;
using SteamKit2;

namespace Reactor.Greenhouse.Setup.Provider
{
    public class SteamProvider : BaseProvider
    {
        private const uint AppId = 945360;
        private const uint DepotId = 945361;
        private const ulong PreObfuscationManifest = 3596575937380717449;

        public bool IsPreObfuscation { get; }

        public SteamProvider(bool isPreObfuscation)
        {
            IsPreObfuscation = isPreObfuscation;
        }

        public override bool IsUpdateNeeded()
        {
            DepotConfigStore.LoadFromFile(Path.Combine(Game.Path, ".DepotDownloader", "depot.config"));
            if (DepotConfigStore.Instance.InstalledManifestIDs.TryGetValue(DepotId, out var installedManifest))
            {
                if (IsPreObfuscation)
                {
                    if (installedManifest == PreObfuscationManifest)
                    {
                        return false;
                    }
                }
                else
                {
                    if (ContentDownloader.steam3 == null)
                    {
                        ContentDownloader.InitializeSteam3();
                    }

                    ContentDownloader.steam3!.RequestAppInfo(AppId);

                    var depots = ContentDownloader.GetSteam3AppSection(AppId, EAppInfoSection.Depots);
                    if (installedManifest == depots[DepotId.ToString()]["manifests"][ContentDownloader.DEFAULT_BRANCH].AsUnsignedLong())
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        public override void Setup()
        {
            if (ContentDownloader.steam3 != null && ContentDownloader.steam3.bConnected)
            {
                return;
            }

            AccountSettingsStore.LoadFromFile("account.config");

            var environmentVariable = Environment.GetEnvironmentVariable("STEAM");

            if (environmentVariable != null)
            {
                var split = environmentVariable.Split(":");
                ContentDownloader.InitializeSteam3(split[0], split[1]);
            }
            else
            {
                ContentDownloader.Config.RememberPassword = true;

                Console.Write("Steam username: ");
                var username = Console.ReadLine();

                string password = null;

                if (!AccountSettingsStore.Instance.LoginKeys.ContainsKey(username))
                {
                    Console.Write("Steam password: ");
                    password = ContentDownloader.Config.SuppliedPassword = Util.ReadPassword();
                    Console.WriteLine();
                }

                ContentDownloader.InitializeSteam3(username, password);
            }

            ContentDownloader.Config.UsingFileList = true;
            ContentDownloader.Config.FilesToDownload = new List<string>
            {
                "GameAssembly.dll"
            };
            ContentDownloader.Config.FilesToDownloadRegex = new List<Regex>
            {
                new Regex("^Among Us_Data/il2cpp_data/Metadata/global-metadata.dat$".Replace("/", "[\\\\|/]"), RegexOptions.Compiled | RegexOptions.IgnoreCase),
                new Regex("^Among Us_Data/globalgamemanagers$".Replace("/", "[\\\\|/]"), RegexOptions.Compiled | RegexOptions.IgnoreCase)
            };
        }

        public override Task DownloadAsync()
        {
            ContentDownloader.Config.InstallDirectory = Game.Path;
            return ContentDownloader.DownloadAppAsync(AppId, DepotId, IsPreObfuscation ? PreObfuscationManifest : ContentDownloader.INVALID_MANIFEST_ID);
        }
    }
}
