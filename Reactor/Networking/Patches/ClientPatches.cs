using System;
using System.Collections;
using System.Linq;
using HarmonyLib;
using Hazel;
using Hazel.Udp;
using InnerNet;
using Reactor.Extensions;
using UnhollowerBaseLib;
using UnityEngine;

namespace Reactor.Networking.Patches
{
    internal static class ClientPatches
    {
        [HarmonyPatch(typeof(InnerNetClient), nameof(InnerNetClient.HandleMessage))]
        public static class HandleMessagePatch
        {
            public static bool Prefix(InnerNetClient __instance, [HarmonyArgument(0)] MessageReader reader)
            {
                if (reader.Tag == byte.MaxValue)
                {
                    var flag = (ReactorMessageFlags) reader.ReadByte();

                    switch (flag)
                    {
                        case ReactorMessageFlags.Handshake:
                        {
                            ModdedHandshakeS2C.Deserialize(reader, out var serverName, out var serverVersion, out var pluginCount);

                            Logger<ReactorPlugin>.Info($"Connected to a modded server ({serverName} {serverVersion}, {pluginCount} plugins), sending mod declarations");

                            var mods = ModList.Current;

                            var writer = MessageWriter.Get(SendOption.Reliable);

                            var expected = 0;
                            var got = 0;

                            void Send()
                            {
                                expected++;

                                __instance.connection.Send(writer, () =>
                                {
                                    got++;

                                    if (got >= expected)
                                    {
                                        Logger<ReactorPlugin>.Debug("Received all acks");
                                        __instance.connection.State = ConnectionState.Connected;
                                    }
                                });
                                writer.Recycle();
                            }

                            foreach (var mod in mods)
                            {
                                ModDeclaration.Serialize(writer, mod);

                                if (writer.Length >= 500)
                                {
                                    writer.CancelMessage();

                                    Send();

                                    writer = MessageWriter.Get(SendOption.Reliable);
                                    ModDeclaration.Serialize(writer, mod);
                                }

                                writer.EndMessage();
                            }

                            Send();

                            break;
                        }

                        case ReactorMessageFlags.PluginDeclaration:
                        {
                            // TODO
                            break;
                        }

                        default:
                            throw new ArgumentOutOfRangeException();
                    }

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

                ackCallback = (Action) (() =>
                {
                    if (__instance.State == ConnectionState.Connected)
                        return;

                    Logger<ReactorPlugin>.Debug("Hello was acked, waiting for modded handshake response");

                    var coroutine = Coroutines.Start(Coroutine(__instance));

                    __instance.Disconnected = Il2CppSystem.Delegate.Combine(
                        (Il2CppSystem.EventHandler<DisconnectedEventArgs>) (Action<Il2CppSystem.Object, DisconnectedEventArgs>) ((_, _) =>
                        {
                            Coroutines.Stop(coroutine);
                        }),
                        __instance.Disconnected
                    ).Cast<Il2CppSystem.EventHandler<DisconnectedEventArgs>>();
                });
            }

            private static IEnumerator Coroutine(UdpConnection connection)
            {
                yield return new WaitForSeconds(3);

                var client = AmongUsClient.Instance;
                if (client != null && client.connection != null && client.connection.Equals(connection) && client.connection.State == ConnectionState.Connecting)
                {
                    var reactorPlugin = PluginSingleton<ReactorPlugin>.Instance;

                    if (reactorPlugin.AllowVanillaServers.Value)
                    {
                        if (reactorPlugin.CustomRpcManager.List.Any())
                        {
                            Logger<ReactorPlugin>.Warning("Config option AllowVanillaServers was set to true, but there are custom rpcs registered!");
                        }
                        else
                        {
                            connection.State = ConnectionState.Connected;
                            yield break;
                        }
                    }

                    client.LastDisconnectReason = DisconnectReasons.Custom;
                    client.LastCustomDisconnect = "Server didn't respond to modded handshake";
                    client.HandleDisconnect(client.LastDisconnectReason, client.LastCustomDisconnect);
                }
            }
        }

        [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.GetConnectionData))]
        public static class HandshakePatch
        {
            public static void Postfix(ref Il2CppStructArray<byte> __result)
            {
                ModList.Update();

                var handshake = MessageWriter.Get(SendOption.Reliable);

                handshake.Write(__result);

                ModdedHandshakeC2S.Serialize(
                    handshake,
                    ModList.Current.Count
                );

                __result = handshake.ToByteArray(false);
                handshake.Recycle();
            }
        }
    }
}
