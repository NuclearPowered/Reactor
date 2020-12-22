using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Il2CppDumper;
using Reactor.Greenhouse.Setup.Provider;
using IOPath = System.IO.Path;

namespace Reactor.Greenhouse.Setup
{
    public class Game
    {
        public BaseProvider Provider { get; }
        public string Name { get; }
        public char Postfix { get; }
        public string Path { get; }
        public string Dll { get; }
        public string Version { get; private set; }

        public Game(BaseProvider provider, string name, string path)
        {
            Provider = provider;
            provider.Game = this;
            Name = name;
            Postfix = char.ToLowerInvariant(name[0]);
            Path = path;
            Dll = IOPath.Combine(Path, "DummyDll", "Assembly-CSharp.dll");
        }

        public async Task DownloadAsync()
        {
            Provider.Setup();
            await Provider.DownloadAsync();
            UpdateVersion();
        }

        public void UpdateVersion()
        {
            if (Version != null)
                return;

            Version = GameVersionParser.Parse(System.IO.Path.Combine(Path, "Among Us_Data", "globalgamemanagers"));
        }

        public void Dump()
        {
            Console.WriteLine($"Dumping {Name}");

            var hash = ComputeHash(IOPath.Combine(Path, "GameAssembly.dll"));
            var hashFile = IOPath.Combine(Path, "Assembly-CSharp.dll.md5");

            if (File.Exists(hashFile) && File.ReadAllText(hashFile) == hash)
            {
                return;
            }

            if (!Il2CppDumper.Il2CppDumper.PerformDump(
                IOPath.Combine(Path, "GameAssembly.dll"),
                IOPath.Combine(Path, "Among Us_Data", "il2cpp_data", "Metadata", "global-metadata.dat"),
                Path,
                new Config
                {
                    RequireAnyKey = false,
                    GenerateScript = false,
                    DumpProperty = true,
                    DumpAttribute = true
                },
                Console.WriteLine
            ))
            {
                throw new Exception("Il2CppDumper failed");
            }

            File.WriteAllText(hashFile, hash);
        }

        private static string ComputeHash(string file)
        {
            using var md5 = MD5.Create();
            using var assemblyStream = File.OpenRead(file);

            var hash = md5.ComputeHash(assemblyStream);

            return Encoding.UTF8.GetString(hash);
        }
    }
}
