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
        [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.GetConnectionData))]
        public static class HandshakePatch
        {
            public static bool Prefix(out Il2CppStructArray<byte> __result)
            {
                var handshake = MessageWriter.Get(SendOption.Reliable);

                var plugins = Preloader.Chainloader.Plugins;

                if (plugins.Values
                    .Select(pluginInfo => pluginInfo.Instance.GetType().GetCustomAttribute<ReactorPluginSideAttribute>())
                    .Any(attribute => attribute == null || attribute.Side == PluginSide.Both))
                {
                    handshake.Write(-1);
                }

                handshake.Write(Constants.GetBroadcastVersion());
                handshake.Write(SaveManager.PlayerName);

                handshake.WritePacked(plugins.Count);

                foreach (var plugin in plugins)
                {
                    handshake.Write(plugin.Key);
                    handshake.Write(plugin.Value.Metadata.Version.ToString());
                }

                __result = handshake.ToByteArray(false);
                handshake.Recycle();

                return false;
            }
        }
    }
}
