using System.Linq;
using System.Reflection;
using BepInEx.IL2CPP;
using HarmonyLib;
using Hazel;
using UnhollowerBaseLib;

namespace Reactor.Patches
{
    internal static class ModdedHandshake
    {
        // TODO add back when vent issue is fixed
        // [HarmonyPatch(typeof(InnerNetServer), nameof(InnerNetServer.OnServerConnect))]
        // public static class FreeplayPatch
        // {
        //     public static void Prefix([HarmonyArgument(0)] NewConnectionEventArgs evt)
        //     {
        //         var handshakeData = evt.HandshakeData;
        //
        //         var protocolVersion = handshakeData.ReadInt32();
        //         if (protocolVersion != -1)
        //         {
        //             // reset position if vanilla
        //             handshakeData._position -= sizeof(int);
        //             handshakeData.readHead -= sizeof(int);
        //         }
        //     }
        // }

        // workaround for ^
        [HarmonyPatch(typeof(InnerNetServer), nameof(InnerNetServer.IsCompatibleVersion))]
        public static class FreeplayPatch
        {
            public static bool Prefix([HarmonyArgument(0)] int version, ref bool __result)
            {
                if (version == -1)
                {
                    __result = true;
                    return false;
                }

                return true;
            }
        }

        [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.GetConnectionData))]
        public static class HandshakePatch
        {
            public static bool Prefix(out Il2CppStructArray<byte> __result)
            {
                var handshake = MessageWriter.Get(SendOption.Reliable);

                var plugins = IL2CPPChainloader.Instance.Plugins;

                if (plugins.Values
                    .Select(pluginInfo => pluginInfo.Instance.GetType().GetCustomAttribute<ReactorPluginSideAttribute>())
                    .Any(attribute => attribute == null || attribute.Side == PluginSide.Both))
                {
                    handshake.Write(-1);
                }

                handshake.Write(Constants.GetBroadcastVersion());
                handshake.Write(SaveManager.PlayerName);

                handshake.WritePacked(plugins.Count);

                foreach (var (pluginId, plugin) in plugins)
                {
                    handshake.Write(pluginId);
                    handshake.Write(plugin.Metadata.Version.ToString());
                }

                __result = handshake.ToByteArray(false);
                handshake.Recycle();

                return false;
            }
        }
    }
}
