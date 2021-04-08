using System;
using System.Collections;
using System.Collections.Generic;
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
        private static Dictionary<UdpConnection, bool> CustomConnections = new Dictionary<UdpConnection, bool>();

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

                            var key = CustomConnections.Keys.FirstOrDefault(k => k.Equals(__instance.connection));
                            if (key == null) key = __instance.connection;
                            CustomConnections[key] = true;

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

                    var key = CustomConnections.Keys.FirstOrDefault(k => k.Equals(__instance));
                    if (key == null) key = __instance;
                    CustomConnections[key] = false;

                    var keys = CustomConnections.Keys.ToArray();
                    foreach (var k in keys)
                    {
                        if (AmongUsClient.Instance == null || AmongUsClient.Instance.connection == null || !AmongUsClient.Instance.connection.Equals(k))
                            CustomConnections.Remove(k);
                    }

                    var coroutine = Coroutines.Start(Coroutine(__instance));

                    __instance.Disconnected = (Action<Il2CppSystem.Object, DisconnectedEventArgs>) ((_, _) =>
                    {
                        Coroutines.Stop(coroutine);
                    });
                });
            }

            private static IEnumerator Coroutine(UdpConnection connection)
            {
                yield return new WaitForSeconds(3);

                var client = AmongUsClient.Instance;
                if (client != null && client.connection != null && client.connection.Equals(connection) && connection.State == ConnectionState.Connecting)
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

        public static class InnerNetPatches
        {
            private static byte MaxCallId = (byte)Extensions.Extensions.GetHighestValue<RpcCalls>();
            private static MessageWriter DummyWriter = MessageWriter.Get(SendOption.None);

            private static bool AllowRpc(UdpConnection connection, byte callId)
            {
                if (callId <= MaxCallId)
                    return true;

                if (connection != null)
                {
                    var key = CustomConnections.Keys.FirstOrDefault(k => k.Equals(connection));
                    if (key != null && CustomConnections[key])
                        return true;
                }

                Logger<ReactorPlugin>.Warning($"A plugin is attempting to send custom rpcs on vanilla servers (callId: {callId})!");

                return false;
            }

            private static bool AllowRpc(UdpConnection connection, byte callId, ref MessageWriter writer, bool immediately = false)
            {
                if (!AllowRpc(connection, callId))
                {
                    DummyWriter.Clear(SendOption.None);

                    writer = DummyWriter;

                    return false;
                }

                return true;
            }

            [HarmonyPatch(typeof(MessageWriter), nameof(MessageWriter.StartMessage))]
            public static class StartMessagePatch
            {
                public static bool Prefix(MessageWriter __instance)
                {
                    return !__instance.Equals(DummyWriter);
                }
            }

            [HarmonyPatch(typeof(MessageWriter), nameof(MessageWriter.EndMessage))]
            public static class EndMessagePatch
            {
                public static bool Prefix(MessageWriter __instance)
                {
                    return !__instance.Equals(DummyWriter);
                }
            }

            [HarmonyPatch(typeof(MessageWriter), nameof(MessageWriter.CancelMessage))]
            public static class CancelMessagePatch
            {
                public static bool Prefix(MessageWriter __instance)
                {
                    return !__instance.Equals(DummyWriter);
                }
            }

            [HarmonyPatch(typeof(InnerNetClient), nameof(InnerNetClient.SendRpc))]
            public static class SendRpcPatch
            {
                public static bool Prefix(InnerNetClient __instance, [HarmonyArgument(1)] byte callId)
                {
                    return AllowRpc(__instance.connection, callId);
                }
            }

            [HarmonyPatch(typeof(InnerNetClient), nameof(InnerNetClient.StartRpc))]
            public static class StartRpcPatch
            {
                public static bool Prefix(InnerNetClient __instance, [HarmonyArgument(1)] byte callId, ref MessageWriter __result)
                {
                    return AllowRpc(__instance.connection, callId, ref __result);
                }
            }

            [HarmonyPatch(typeof(InnerNetClient), nameof(InnerNetClient.StartRpcImmediately))]
            public static class StartRpcImmediatelyPatch
            {
                public static bool Prefix(InnerNetClient __instance, [HarmonyArgument(1)] byte callId, ref MessageWriter __result)
                {
                    return AllowRpc(__instance.connection, callId, ref __result, true);
                }
            }

            [HarmonyPatch(typeof(InnerNetClient), nameof(InnerNetClient.FinishRpcImmediately))]
            public static class FinishRpcImmediatelyPatch
            {
                public static bool Prefix([HarmonyArgument(0)] MessageWriter writer)
                {
                    return !writer.Equals(DummyWriter);
                }
            }
        }
    }
}
