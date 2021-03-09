using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using Hazel;
using Hazel.Udp;
using InnerNet;
using Reactor.Extensions;
using Reactor.Net;
using UnhollowerBaseLib;
using UnityEngine;

namespace Reactor.Patches
{
    internal static class ModdedHandshakePatches
    {
        // [HarmonyPatch(typeof(InnerNetServer), nameof(InnerNetServer.OnServerConnect))]
        [HarmonyPatch]
        public static class ServerPatch
        {
            public static IEnumerable<MethodBase> TargetMethods()
            {
                return typeof(InnerNetServer).GetMethods(typeof(void), typeof(NewConnectionEventArgs));
            }

            private static readonly string ServerBrand = $"Among Us {Application.version}";

            public static void Postfix([HarmonyArgument(0)] NewConnectionEventArgs evt)
            {
                var writer = MessageWriter.Get(SendOption.Reliable);
                ModdedHandshakeS2C.Serialize(writer, ServerBrand);
                evt.Connection.Send(writer);
                writer.Recycle();
            }
        }

        // [HarmonyPatch(typeof(InnerNetClient), nameof(InnerNetClient.HandleMessage))]
        [HarmonyPatch]
        public static class HandleMessagePatch
        {
            public static IEnumerable<MethodBase> TargetMethods()
            {
                return typeof(InnerNetClient).GetMethods(typeof(void), typeof(MessageReader), typeof(SendOption));
            }

            public static bool Prefix(InnerNetClient __instance, [HarmonyArgument(0)] MessageReader reader, [HarmonyArgument(1)] SendOption sendOption)
            {
                if (reader.Tag == byte.MaxValue)
                {
                    var serverBrand = reader.ReadString();

                    __instance.connection.State = ConnectionState.Connected;

                    PluginSingleton<ReactorPlugin>.Instance.Log.LogDebug($"Connected to modded server ({serverBrand})");

                    return false;
                }

                return true;
            }
        }

        [HarmonyPatch(typeof(UdpConnection), nameof(UdpConnection.HandleSend))]
        public static class SendHelloPatch
        {
            public static void Prefix(UdpConnection __instance, byte sendOption, ref Il2CppSystem.Action ackCallback)
            {
                if (sendOption != 8 || AmongUsClient.Instance == null || AmongUsClient.Instance.connection == null || !AmongUsClient.Instance.connection.Equals(__instance))
                {
                    return;
                }

                if (!PluginSingleton<ReactorPlugin>.Instance.ModdedHandshake.Value)
                {
                    return;
                }

                ackCallback = (Action) (() =>
                {
                    if (__instance.State == ConnectionState.Connected)
                        return;

                    PluginSingleton<ReactorPlugin>.Instance.Log.LogDebug("Hello was acked, waiting for modded handshake response");

                    Coroutines.Start(Coroutine());

                    __instance.add_Disconnected((Action<Il2CppSystem.Object, DisconnectedEventArgs>) ((_, _) =>
                    {
                        Coroutines.Stop(Coroutine());
                    }));
                });
            }
        }

        private static IEnumerator Coroutine()
        {
            yield return new WaitForSeconds(3);

            var client = AmongUsClient.Instance;

            if (client != null && client.connection != null && client.connection.State == ConnectionState.Connecting)
            {
                client.LastDisconnectReason = DisconnectReasons.Custom;
                client.LastCustomDisconnect = "Server didn't respond to modded handshake";
                client.HandleDisconnect(client.LastDisconnectReason, client.LastCustomDisconnect);
            }
        }

        [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.GetConnectionData))]
        public static class HandshakePatch
        {
            public static bool Prefix(out Il2CppStructArray<byte> __result)
            {
                var handshake = MessageWriter.Get(SendOption.Reliable);

                ModdedHandshakeC2S.Serialize(
                    handshake,
                    Constants.GetBroadcastVersionBytes(),
                    SaveManager.PlayerName,
                    ModList.GetCurrent()
                );

                __result = handshake.ToByteArray(false);
                handshake.Recycle();

                return false;
            }
        }
    }
}
