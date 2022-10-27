using System;
using AmongUs.Data;
using HarmonyLib;
using Hazel;
using Il2CppInterop.Runtime;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using InnerNet;
using Reactor.Networking.Extensions;
using Reactor.Networking.Messages;
using UnityEngine;

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

    [HarmonyPatch(typeof(InnerNetClient._HandleGameDataInner_d__39), nameof(InnerNetClient._HandleGameDataInner_d__39.MoveNext))]
    public static class HandleGameDataInnerPatch
    {
        public static bool Prefix(InnerNetClient._HandleGameDataInner_d__39 __instance, ref bool __result)
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

            if (reader.Tag == InnerNetClient.SceneChangeFlag && ReactorConnection.Instance?.Syncer == Syncer.Host)
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
                            if (reactorClientData == null && ModList.IsAnyModIsRequiredOnAllClients)
                            {
                                Warning("Kicking " + clientData.PlayerName + " for not having Reactor installed");

                                var chatText = clientData.PlayerName + " tried joining without Reactor installed";
                                if (DataManager.Settings.Multiplayer.ChatMode == QuickChatModes.FreeChatOrQuickChat)
                                    PlayerControl.LocalPlayer.RpcSendChat(chatText);
                                else
                                    HudManager.Instance.Chat.AddChat(PlayerControl.LocalPlayer, chatText);

                                innerNetClient.KickPlayer(clientData.Id, false);

                                __result = false;
                                return false;
                            }

                            if (!Mod.Validate(reactorClientData?.Mods ?? Array.Empty<Mod>(), ModList.Current, out var reason))
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

    [HarmonyPatch(typeof(InnerNetClient._CoSendSceneChange_d__30), nameof(InnerNetClient._CoSendSceneChange_d__30.MoveNext))]
    public static class CoSendSceneChangePatch
    {
        public static bool Prefix(InnerNetClient._CoSendSceneChange_d__30 __instance, ref bool __result)
        {
            if (ReactorConnection.Instance!.Syncer != Syncer.Host) return true;

            var innerNetClient = __instance.__4__this;

            if (__instance.__1__state == 2)
            {
                if (!innerNetClient.AmHost && innerNetClient.connection.State == ConnectionState.Connected)
                {
                    var writer = MessageWriter.Get(SendOption.Reliable);
                    writer.StartMessage(Tags.GameData);
                    writer.Write(innerNetClient.GameId);
                    writer.StartMessage(InnerNetClient.SceneChangeFlag);
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
                }

                __instance.__1__state = -1;
                __result = false;
                return false;
            }

            return true;
        }
    }

    [HarmonyPatch(typeof(InnerNetClient._CoHandleSpawn_d__40), nameof(InnerNetClient._CoHandleSpawn_d__40.MoveNext))]
    public static class CoHandleSpawnPatch
    {
        public static void Postfix(InnerNetClient._CoHandleSpawn_d__40 __instance, bool __result)
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

    [HarmonyPatch(typeof(InnerNetClient), nameof(InnerNetClient.WriteSpawnMessage))]
    public static class WriteSpawnMessagePatch
    {
        public static bool Prefix(InnerNetClient __instance, InnerNetObject netObjParent, int ownerId, SpawnFlags flags, MessageWriter msg)
        {
            if (ReactorConnection.Instance!.Syncer != Syncer.Host) return true;

            msg.StartMessage(4);
            msg.WritePacked(netObjParent.SpawnId);
            msg.WritePacked(ownerId);
            msg.Write((byte) flags);
            InnerNetObject[] componentsInChildren = netObjParent.GetComponentsInChildren<InnerNetObject>();
            msg.WritePacked(componentsInChildren.Length);
            foreach (var innerNetObject in componentsInChildren)
            {
                innerNetObject.OwnerId = ownerId;
                innerNetObject.SpawnFlags = flags;
                if (innerNetObject.NetId == 0)
                {
                    innerNetObject.NetId = __instance.NetIdCnt++;
                    __instance.allObjects.Add(innerNetObject);
                    __instance.allObjectsFast.Add(innerNetObject.NetId, innerNetObject);
                }

                msg.WritePacked(innerNetObject.NetId);
                msg.StartMessage(1);
                innerNetObject.Serialize(msg, initialState: true);
                msg.EndMessage();
            }

            // PATCH - Inject ReactorHandshakeS2C
            if (__instance.ClientId != ownerId && IL2CPP.il2cpp_object_get_class(netObjParent.Pointer) == Il2CppClassPointerStore<PlayerControl>.NativeClassPtr)
            {
                Debug("Injecting ReactorHandshakeS2C to WriteSpawnMessage");
                ReactorHeader.Write(msg);
                ModdedHandshakeS2C.Serialize(msg, "Among Us", Application.version, 0); // TODO
            }

            msg.EndMessage();

            return false;
        }
    }

    [HarmonyPatch(typeof(InnerNetClient), nameof(InnerNetClient.HandleMessage))]
    public static class HandleMessagePatch
    {
        public static bool Prefix(InnerNetClient __instance, [HarmonyArgument(0)] MessageReader reader)
        {
            if (__instance.GameMode == GameModes.FreePlay) return true;

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

    [HarmonyPatch(typeof(InnerNetClient), nameof(InnerNetClient.GetConnectionData))]
    public static class HandshakePatch
    {
        public static void Prefix(ref bool useDtlsLayout)
        {
            // Due to reasons currently unknown, the useDtlsLayout parameter sometimes doesn't reflect whether DTLS
            // is actually supposed to be enabled. This causes a bad handshake message and a quick disconnect.
            // The field on AmongUsClient appears to be more reliable, so override this parameter with what it is supposed to be.
            Info($"Currently using dtls: {useDtlsLayout}. Should use dtls: {AmongUsClient.Instance.useDtls}");
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
