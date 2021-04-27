using HarmonyLib;
using Hazel;
using InnerNet;
using UnityEngine;

namespace Reactor.Networking.Patches
{
    internal static class ServerPatches
    {
        [HarmonyPatch(typeof(InnerNetServer), nameof(InnerNetServer.OnServerConnect))]
        public static class HandshakeResponsePatch
        {
            public static void Postfix([HarmonyArgument(0)] NewConnectionEventArgs evt)
            {
                var writer = MessageWriter.Get(SendOption.Reliable);
                ModdedHandshakeS2C.Serialize(writer, "Among Us", Application.version, 0);
                evt.Connection.Send(writer);
                writer.Recycle();
            }
        }
    }
}
