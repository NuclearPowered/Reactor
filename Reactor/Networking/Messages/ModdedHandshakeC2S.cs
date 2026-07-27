using System.Collections.Generic;
using System.Text;
using Hazel;

namespace Reactor.Networking.Messages;

internal static class ModdedHandshakeC2S
{
    private const int MaxPackedSize = 5;

    public static int GetMaxSerializedSize(IReadOnlyCollection<Mod> mods)
    {
        var size = MaxPackedSize;

        foreach (var mod in mods)
        {
            size += GetMaxStringSize(mod.Id) + GetMaxStringSize(mod.Version) + sizeof(ushort);
            if (mod.IsRequiredOnAllClients) size += GetMaxStringSize(mod.Name);
        }

        return size;
    }

    private static int GetMaxStringSize(string? value)
    {
        return MaxPackedSize + (value == null ? 0 : Encoding.UTF8.GetByteCount(value));
    }

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
