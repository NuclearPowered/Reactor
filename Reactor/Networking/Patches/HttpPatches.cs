using System;
using System.Linq;
using System.Text;
using HarmonyLib;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace Reactor.Networking.Patches;

internal static class HttpPatches
{
    public const int Version = 1;

    public static string BuildHeader()
    {
        var stringBuilder = new StringBuilder();

        stringBuilder.Append(Version);
        stringBuilder.Append(';');

        var mods = ModList.Current.Where(m => m.IsRequiredOnAllClients).ToArray();

        stringBuilder.Append(mods.Length);

        foreach (var mod in mods)
        {
            stringBuilder.Append(';');
            stringBuilder.Append(mod.Id);
            stringBuilder.Append('=');
            stringBuilder.Append(mod.Version);
        }

        return stringBuilder.ToString();
    }

    internal static bool IsCurrentRegionModded()
    {
        var currentRegion = ServerManager.Instance.CurrentRegion;

        if (currentRegion.TryCast<StaticHttpRegionInfo>() is { } httpRegionInfo)
        {
            var lastConnection = SendWebRequestPatch.LastConnection;
            return lastConnection.HasValue && lastConnection.Value.RegionInfo.Equals(httpRegionInfo) && lastConnection.Value.IsModded;
        }

        return ReactorConnection.Instance is { Syncer: Syncer.Server };
    }

    [HarmonyPatch(typeof(UnityWebRequest), nameof(UnityWebRequest.SendWebRequest))]
    private static class SendWebRequestPatch
    {
        internal static (StaticHttpRegionInfo RegionInfo, bool IsModded)? LastConnection { get; private set; }

        public static void Prefix(UnityWebRequest __instance)
        {
            var path = __instance.uri.AbsolutePath;
            if (path == "/api/games")
            {
                Debug($"{__instance.method} {path}");
                __instance.SetRequestHeader("Client-Mods", BuildHeader());
            }
        }

        public static void Postfix(UnityWebRequest __instance, UnityWebRequestAsyncOperation __result)
        {
            var path = __instance.uri.AbsolutePath;
            if (path == "/api/games")
            {
                __result.add_completed((Action<AsyncOperation>) (_ =>
                {
                    if (!HttpUtils.IsSuccess(__instance.responseCode)) return;

                    var responseHeader = __instance.GetResponseHeader("Client-Mods-Processed");

                    if (responseHeader != null)
                    {
                        Debug("Connected to a modded HTTP matchmaking server");
                    }
                    else
                    {
                        Warning("Connected to a vanilla HTTP matchmaking server");
                    }

                    if (__instance.GetMethod() == UnityWebRequest.UnityWebRequestMethod.Get)
                    {
                        if (responseHeader == null && ModList.IsAnyModRequiredOnAllClients && !ReactorConfig.IgnoreHandshakePopup.Value)
                        {
                            HandshakePopup.Show();
                        }
                    }

                    LastConnection = (ServerManager.Instance.CurrentRegion.Cast<StaticHttpRegionInfo>(), responseHeader != null);
                }));
            }
        }
    }

    [HarmonyPatch(typeof(GameStartManager), nameof(GameStartManager.Start))]
    private static class GameStartManagerPatch
    {
        public static void Postfix(GameStartManager __instance)
        {
            if (AmongUsClient.Instance.NetworkMode != NetworkModes.OnlineGame) return;
            if (ModList.IsAnyModRequiredOnAllClients && !IsCurrentRegionModded())
            {
                Warning("Vanilla region, locking public toggle");

                __instance.MakePublicButtonBehaviour.enabled = false;
                var actionMapGlyphDisplay = __instance.MakePublicButton.GetComponentInChildren<ActionMapGlyphDisplay>(includeInactive: true);
                if (actionMapGlyphDisplay)
                {
                    actionMapGlyphDisplay.gameObject.SetActive(value: false);
                }

                __instance.MakePublicButton.color = new Color(1, 1, 1, 0.5f);
                __instance.MakePublicButton.sprite = __instance.PrivateGameImage;

                var onClick = __instance.MakePublicButton.GetComponent<PassiveButton>().OnClick = new Button.ButtonClickedEvent();
                onClick.AddListener((Action) MakePublicDisallowedPopup.Show);

                if (AmongUsClient.Instance.AmHost && AmongUsClient.Instance.IsGamePublic)
                {
                    AmongUsClient.Instance.ChangeGamePublic(false);
                }
            }
        }
    }
}
