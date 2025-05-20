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
            // new Uri() is necessary because uri property got stripped.
            var path = new Uri(__instance.url).AbsolutePath;
            // Innersloth changed /api/games to /api/games/filtered
            // We may want to keep support for /api/games though.
            if (path.Contains("/api/games"))
            {
                Debug($"{__instance.method} {path}");
                __instance.SetRequestHeader("Client-Mods", BuildHeader());
            }
        }

        public static void Postfix(UnityWebRequest __instance, UnityWebRequestAsyncOperation __result)
        {
            // new Uri() is necessary because uri property got stripped.
            var path = new Uri(__instance.url).AbsolutePath;
            // Innersloth changed /api/games to /api/games/filtered
            // We may want to keep support for /api/games though.
            if (path.Contains("/api/games"))
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
                        if (responseHeader == null && ModList.IsAnyModRequiredOnAllClients)
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

                __instance.HostPublicButton.enabled = false;
                __instance.HostPrivateButton.transform.FindChild("Inactive").GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 0.5f);

                var onClick = __instance.HostPrivateButton.OnClick = new Button.ButtonClickedEvent();
                onClick.AddListener((Action) MakePublicDisallowedPopup.Show);

                if (AmongUsClient.Instance.AmHost && AmongUsClient.Instance.IsGamePublic)
                {
                    AmongUsClient.Instance.ChangeGamePublic(false);
                }
            }
        }
    }
}
