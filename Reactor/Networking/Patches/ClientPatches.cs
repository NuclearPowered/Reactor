using System;
using System.Linq;
using AmongUs.Data;
using AmongUs.InnerNet.GameDataMessages;
using BepInEx.Unity.IL2CPP.Utils;
using HarmonyLib;
using Hazel;
using Il2CppInterop.Runtime;
using Il2CppInterop.Runtime.InteropTypes;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using InnerNet;
using Reactor.Networking.Extensions;
using Reactor.Networking.Messages;
using UnityEngine;
using IEnumerator = System.Collections.IEnumerator;

namespace Reactor.Networking.Patches;

internal static class ClientPatches
{
    [HarmonyPatch(typeof(InnerNetClient), nameof(InnerNetClient.DisconnectInternal))]
    public static class DisconnectInternalPatch
    {
        public static void Prefix(InnerNetClient __instance, ref DisconnectReasons reason)
        {
            if (reason == DisconnectReasons.Kicked && ReactorConnection.Instance?.LastKickReason is { } lastKickReason)
            {
                reason = DisconnectReasons.Custom;
                __instance.LastCustomDisconnect = lastKickReason;
            }
        }
    }

    [HarmonyPatch(typeof(InnerNetClient._HandleGameDataInner_d__165), nameof(InnerNetClient._HandleGameDataInner_d__165.MoveNext))]
    public static class HandleGameDataInnerPatch
    {
        public static bool Prefix(InnerNetClient._HandleGameDataInner_d__165 __instance, ref bool __result)
        {
            var innerNetClient = __instance.__4__this;
            var reader = __instance.reader;

            if (__instance.__1__state != 0) return true;

            if (reader.Tag == byte.MaxValue)
            {
                var flag = (ReactorGameDataFlag) reader.ReadByte();
                switch (flag)
                {
                    case ReactorGameDataFlag.SetKickReason:
                    {
                        var reason = reader.ReadString();
                        Debug("Received SetKickReason: " + reason);
                        if (ReactorConnection.Instance != null)
                        {
                            ReactorConnection.Instance.LastKickReason = reason;
                        }

                        break;
                    }
                }

                reader.Recycle();

                __result = false;
                return false;
            }

            if (reader.Tag == (byte) GameDataTypes.SceneChangeFlag && ReactorConnection.Instance?.Syncer == Syncer.Host)
            {
                var clientId = reader.ReadPackedInt32();
                var clientData = innerNetClient.FindClientById(clientId);
                var sceneName = reader.ReadString();

                if (clientData != null && !string.IsNullOrWhiteSpace(sceneName))
                {
                    ReactorClientData? reactorClientData = null;

                    // PATCH - Read injected ReactorHandshakeC2S
                    {
                        if (reader.BytesRemaining >= ReactorHeader.Size && ReactorHeader.Read(reader))
                        {
                            ModdedHandshakeC2S.Deserialize(reader, out var mods);

                            reactorClientData = new ReactorClientData(clientData, mods);
                            ReactorClientData.Set(clientId, reactorClientData);

                            Debug("Received reactor handshake for " + clientData.PlayerName);
                        }

                        if (innerNetClient.AmHost)
                        {
                            if (reactorClientData == null && ModList.IsAnyModRequiredOnAllClients)
                            {
                                IEnumerator CoKick()
                                {
                                    var startTime = DateTimeOffset.UtcNow;
                                    string? playerName;

                                    do
                                    {
                                        yield return null;

                                        if (DateTimeOffset.UtcNow - startTime > TimeSpan.FromSeconds(2))
                                        {
                                            playerName = "(unknown)";
                                            break;
                                        }

                                        playerName = GameData.Instance.GetPlayerByClient(clientData)?.PlayerName;
                                    } while (string.IsNullOrEmpty(playerName));

                                    Warning("Kicking " + playerName + " for not having Reactor installed");

                                    const int ChatMessageLimit = 100;

                                    var chatText = $"{playerName} tried joining without the following mods:";
                                    foreach (var mod in ModList.Current.Where(m => m.IsRequiredOnAllClients))
                                    {
                                        chatText += $"\n- {mod}";
                                    }

                                    HudManager.Instance.Chat.AddChat(PlayerControl.LocalPlayer, chatText, false);

                                    if (DataManager.Settings.Multiplayer.ChatMode == QuickChatModes.FreeChatOrQuickChat)
                                    {
                                        var truncatedChatText = chatText.Length > ChatMessageLimit
                                            ? chatText[..(ChatMessageLimit - 3)] + "..."
                                            : chatText;

                                        // Write SendChat directly instead of using RpcSendChat so we can call AddChat ourselves
                                        var messageWriter = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte) RpcCalls.SendChat, SendOption.Reliable);
                                        messageWriter.Write(truncatedChatText);
                                        messageWriter.EndMessage();
                                    }

                                    innerNetClient.KickPlayer(clientData.Id, false);
                                }

                                innerNetClient.StartCoroutine(CoKick());
                            }
                            else if (!Mod.Validate(reactorClientData?.Mods ?? Array.Empty<Mod>(), ModList.Current, out var reason))
                            {
                                innerNetClient.KickWithReason(clientData.Id, reason);
                                __result = false;
                                return false;
                            }
                        }
                    }

                    innerNetClient.StartCoroutine(innerNetClient.CoOnPlayerChangedScene(clientData, sceneName));
                }
                else
                {
                    UnityEngine.Debug.Log($"Couldn't find client {clientId} to change scene to {sceneName}");
                    reader.Recycle();
                }

                __result = false;
                return false;
            }

            return true;
        }
    }

    [HarmonyPatch(typeof(InnerNetClient._CoSendSceneChange_d__156), nameof(InnerNetClient._CoSendSceneChange_d__156.MoveNext))]
    public static class CoSendSceneChangePatch
    {
        public static bool Prefix(InnerNetClient._CoSendSceneChange_d__156 __instance, ref bool __result)
        {
            if (ReactorConnection.Instance!.Syncer != Syncer.Host) return true;

            var innerNetClient = __instance.__4__this;

            // Check for the conditions when the scene change message should be sent
            if (!innerNetClient.AmHost &&
                innerNetClient.connection.State == ConnectionState.Connected &&
                innerNetClient.ClientId >= 0)
            {
                var clientData = innerNetClient.FindClientById(innerNetClient.ClientId);
                if (clientData != null)
                {
                    var writer = MessageWriter.Get(SendOption.Reliable);
                    writer.StartMessage(Tags.GameData);
                    writer.Write(innerNetClient.GameId);
                    writer.StartMessage((byte) GameDataTypes.SceneChangeFlag);
                    writer.WritePacked(innerNetClient.ClientId);
                    writer.Write(__instance.sceneName);

                    // PATCH - Inject ReactorHandshakeC2S
                    Debug("Injecting ReactorHandshakeC2S to CoSendSceneChange");
                    ReactorHeader.Write(writer);
                    ModdedHandshakeC2S.Serialize(writer, ModList.Current);

                    writer.EndMessage();
                    writer.EndMessage();
                    innerNetClient.SendOrDisconnect(writer);
                    writer.Recycle();

                    // Create a new coroutine to let AmongUsClient handle scene changes too
                    innerNetClient.StartCoroutine(innerNetClient.CoOnPlayerChangedScene(clientData, __instance.sceneName));

                    // Cancel this coroutine
                    __instance.__1__state = -1;
                    __result = false;
                    return false;
                }
            }

            return true;
        }
    }

    [HarmonyPatch(typeof(InnerNetClient._CoHandleSpawn_d__166), nameof(InnerNetClient._CoHandleSpawn_d__166.MoveNext))]
    public static class CoHandleSpawnPatch
    {
        public static void Postfix(InnerNetClient._CoHandleSpawn_d__166 __instance, bool __result)
        {
            if (ReactorConnection.Instance!.Syncer != Syncer.Host) return;

            if (!__result && !AmongUsClient.Instance.AmHost && __instance._ownerId_5__2 == AmongUsClient.Instance.ClientId)
            {
                var reader = __instance.reader;
                if (reader.BytesRemaining >= ReactorHeader.Size && ReactorHeader.Read(reader))
                {
                    ModdedHandshakeS2C.Deserialize(reader, out var serverName, out var serverVersion, out _);
                    Debug($"Host is modded ({serverName} {serverVersion})");
                }
                else
                {
                    Debug("Host is not modded");
                    if (!Mod.Validate(ModList.Current, Array.Empty<Mod>(), out var reason))
                    {
                        AmongUsClient.Instance.DisconnectWithReason(reason);
                    }
                }
            }
        }
    }

    [HarmonyPatch(typeof(SpawnGameDataMessage), nameof(SpawnGameDataMessage.SerializeValues))]
    public static class SpawnGameDataMessagePatch
    {
        public static void Postfix(SpawnGameDataMessage __instance, MessageWriter msg)
        {
            if (ReactorConnection.Instance!.Syncer != Syncer.Host)
            {
                return;
            }

            // PATCH - Inject ReactorHandshakeS2C
            if (AmongUsClient.Instance.ClientId != __instance.ownerId && __instance.NetObjectType == Il2CppType.Of<PlayerControl>())
            {
                Debug("Injecting ReactorHandshakeS2C to WriteSpawnMessage");
                ReactorHeader.Write(msg);
                ModdedHandshakeS2C.Serialize(msg, "Among Us", Application.version, 0); // TODO
            }
        }
    }

    [HarmonyPatch(typeof(InnerNetClient), nameof(InnerNetClient.HandleMessage))]
    public static class HandleMessagePatch
    {
        public static bool Prefix(InnerNetClient __instance, [HarmonyArgument(0)] MessageReader reader)
        {
            if (__instance.NetworkMode == NetworkModes.FreePlay)
            {
                return true;
            }

            var isFirst = false;

            if (ReactorConnection.Instance!.Syncer == null)
            {
                var parentBuffer = reader.Parent.Buffer;
                var sendOption = (SendOption) parentBuffer[0];
                if (sendOption == SendOption.Reliable)
                {
                    var id = (ushort) ((parentBuffer[1] << 8) + parentBuffer[2]);
                    isFirst = id == 1;
                }
            }

            if (reader.Tag == byte.MaxValue)
            {
                var flag = (ReactorMessageFlags) reader.ReadByte();

                switch (flag)
                {
                    case ReactorMessageFlags.Handshake:
                    {
                        ModdedHandshakeS2C.Deserialize(reader, out var serverName, out var serverVersion, out var pluginCount);

                        if (isFirst)
                        {
                            Info($"Connected to a modded server ({serverName} {serverVersion}, {pluginCount} plugins)");
                            ReactorConnection.Instance.Syncer = Syncer.Server;
                        }
                        else
                        {
                            Warning("Modded handshake came in late");
                            __instance.DisconnectWithReason("Reactor handshake was sent out of order, please try connecting again.");
                        }

                        break;
                    }
                }

                return false;
            }

            if (isFirst)
            {
                ReactorConnection.Instance.Syncer = Syncer.Host;
                Warning("Server is not modded, falling back to HAAS");
            }

            return true;
        }
    }

    // [HarmonyPatch(typeof(InnerNetClient), nameof(InnerNetClient.GetConnectionData))]
    public static class HandshakePatch
    {
        public static void Prefix(Il2CppObjectBase __instance, ref bool useDtlsLayout)
        {
            // Due to reasons currently unknown, the useDtlsLayout parameter sometimes doesn't reflect whether DTLS
            // is actually supposed to be enabled. This causes a bad handshake message and a quick disconnect.
            // The field on AmongUsClient appears to be more reliable, so override this parameter with what it is supposed to be.
            Info($"Currently using dtls: {useDtlsLayout}. Should use dtls: {AmongUsClient.Instance.useDtls}");
            Info($"InnerNetClient null?: {__instance == null}");
            Info($"InnerNetClient pointer zero?: {__instance.Pointer == IntPtr.Zero}");
            useDtlsLayout = AmongUsClient.Instance.useDtls;
        }

        public static void Postfix(ref Il2CppStructArray<byte> __result)
        {
            var handshake = new MessageWriter(1000);

            handshake.Write(__result);

            ReactorHeader.Write(handshake);

            ModdedHandshakeC2S.Serialize(
                handshake,
                ModList.Current
            );

            __result = handshake.ToByteArray(true);
            handshake.Recycle();
        }
    }
}
