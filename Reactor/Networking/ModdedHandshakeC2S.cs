using System.Collections.Generic;
using Hazel;

namespace Reactor.Networking;

public static class ModdedHandshakeC2S
{
    public static void Serialize(MessageWriter writer, IReadOnlyCollection<Mod> mods)
    {
        writer.WritePacked(mods.Count);
        foreach (var mod in mods)
        {
            writer.Write(mod.Id);
            writer.Write(mod.Version);
            writer.Write((ushort) mod.Flags);
            if (mod.IsRequiredOnAllClients) writer.Write(mod.Name);
        }
    }

    public static void Deserialize(MessageReader reader, out Mod[] mods)
    {
        var modCount = reader.ReadPackedInt32();
        mods = new Mod[modCount];

        for (var i = 0; i < modCount; i++)
        {
            var id = reader.ReadString();
            var version = reader.ReadString();
            var flags = (ModFlags) reader.ReadUInt16();
            var name = (flags & ModFlags.RequireOnAllClients) != 0 ? reader.ReadString() : null;

            mods[i] = new Mod(id, version, flags, name);
        }
    }
}
