using System;
using System.Collections;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Mono.Cecil;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Reactor.Greenhouse.Setup;
using Reactor.OxygenFilter;

namespace Reactor.Greenhouse
{
    internal static class Program
    {
        private static Task<int> Main(string[] args)
        {
            var rootCommand = new RootCommand();

            rootCommand.Handler = CommandHandler.Create(SearchAsync);

            return rootCommand.InvokeAsync(args);
        }

        private static async Task SearchAsync()
        {
            var gameManager = new GameManager();
            await gameManager.SetupAsync();

            JsonConvert.DefaultSettings = () => new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                DefaultValueHandling = DefaultValueHandling.Ignore,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ContractResolver = ShouldSerializeContractResolver.Instance,
            };

            Console.WriteLine($"Generating mappings from {gameManager.PreObfuscation.Name} ({gameManager.PreObfuscation.Version})");
            using var old = ModuleDefinition.ReadModule(File.OpenRead(gameManager.PreObfuscation.Dll));

            await GenerateAsync(gameManager.Steam, old);
            await GenerateAsync(gameManager.Itch, old);
        }

        private static async Task GenerateAsync(Game game, ModuleDefinition old)
        {
            Console.WriteLine($"Compiling mappings for {game.Name} ({game.Version})");

            using var moduleDef = ModuleDefinition.ReadModule(File.OpenRead(game.Dll));
            var version = game.Version;
            var postfix = game.Postfix;

            var generated = Generator.Generate(old, moduleDef);

            await File.WriteAllTextAsync(Path.Combine("work", version + postfix + ".generated.json"), JsonConvert.SerializeObject(generated, Formatting.Indented));

            Apply(generated, Path.Combine("mappings", version + ".json"));
            Apply(generated, Path.Combine("mappings", version + postfix + ".json"));

            generated.Compile(moduleDef);

            await File.WriteAllTextAsync(Path.Combine("work", version + postfix + ".json"), JsonConvert.SerializeObject(generated));
        }

        private static void Apply(Mappings generated, string file)
        {
            if (File.Exists(file))
            {
                var mappings = JsonConvert.DeserializeObject<Mappings>(File.ReadAllText(file));
                generated.Apply(mappings);
            }
        }

        public class ShouldSerializeContractResolver : CamelCasePropertyNamesContractResolver
        {
            public static ShouldSerializeContractResolver Instance { get; } = new ShouldSerializeContractResolver();

            protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
            {
                var property = base.CreateProperty(member, memberSerialization);

                if (property.PropertyType != null && property.PropertyType != typeof(string))
                {
                    if (property.PropertyType.GetInterface(nameof(IEnumerable)) != null)
                    {
                        property.ShouldSerialize = instance => (instance?.GetType().GetProperty(property.UnderlyingName!)!.GetValue(instance) as IEnumerable<object>)?.Count() > 0;
                    }
                }

                return property;
            }
        }
    }
}
