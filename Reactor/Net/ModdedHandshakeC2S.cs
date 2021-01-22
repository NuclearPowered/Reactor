using System.Collections.Generic;
using Hazel;
using UnhollowerBaseLib;

namespace Reactor.Net
{
    public static class ModdedHandshakeC2S
    {
        public static void Serialize(MessageWriter writer, Il2CppStructArray<byte> clientVersion, string name, ISet<Mod> mods = null)
        {
            writer.Write(clientVersion);
            writer.Write(name);

            if (mods != null)
            {
                ModList.Serialize(writer, mods);
            }
        }
    }
}
