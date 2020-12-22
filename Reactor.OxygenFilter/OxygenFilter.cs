using System.IO;
using Mono.Cecil;

namespace Reactor.OxygenFilter
{
    public class OxygenFilter
    {
        public void Start(FileInfo mappingsFile, FileInfo dumpedDll, FileInfo outputDll)
        {
            using var inputStream = dumpedDll.Open(FileMode.Open, FileAccess.ReadWrite, FileShare.Read);
            using var outputStream = dumpedDll == outputDll ? inputStream : dumpedDll.Open(FileMode.Open, FileAccess.ReadWrite, FileShare.Read);

            var resolver = new DefaultAssemblyResolver();
            resolver.AddSearchDirectory(dumpedDll.DirectoryName);

            using var module = ModuleDefinition.ReadModule(inputStream, new ReaderParameters(ReadingMode.Immediate) { AssemblyResolver = resolver });
            var mapper = new ObfuscationMapper(module);
            mapper.LoadMappings(mappingsFile);
            mapper.Map();
            module.Dispose();
            module.Write(outputStream);
        }
    }
}
