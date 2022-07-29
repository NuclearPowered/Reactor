using System;
using System.Linq;
using System.Text;
using HarmonyLib;
using UnhollowerBaseLib;
using UnityEngine.Networking;

namespace Reactor.Networking.Patches;

[HarmonyPatch]
internal static class HttpPatches
{
    public const int Version = 1;

    public static string BuildHeader()
    {
        var stringBuilder = new StringBuilder();

        stringBuilder.Append(Version);
        stringBuilder.Append(";");

        var mods = ModList.Update().Where(m => m.Side == PluginSide.Both).OrderBy(m => m.Id, StringComparer.Ordinal).ToArray();

        stringBuilder.Append(mods.Length);

        foreach (var mod in mods)
        {
            stringBuilder.Append(";");
            stringBuilder.Append(mod.Id);
            stringBuilder.Append("=");
            stringBuilder.Append(mod.Version);
        }

        return stringBuilder.ToString();
    }

    [HarmonyPatch(typeof(UnityWebRequest), nameof(UnityWebRequest.Get), typeof(string))]
    [HarmonyPatch(typeof(UnityWebRequest), nameof(UnityWebRequest.Post), typeof(string), typeof(string))]
    [HarmonyPatch(typeof(UnityWebRequest), nameof(UnityWebRequest.Put), typeof(string), typeof(Il2CppStructArray<byte>))]
    [HarmonyPostfix]
    public static void UnityWebRequestPatch(UnityWebRequest __result)
    {
        var path = __result.uri.AbsolutePath;

        if (path == "/api/games")
        {
            __result.SetRequestHeader("Client-Mods", BuildHeader());
        }
    }

    [HarmonyPatch(typeof(HttpMatchmakerManager._CoRequestGameList_d__7), nameof(HttpMatchmakerManager._CoRequestGameList_d__7.MoveNext))]
    public static class CoRequestGameListPatch
    {
        public static void Prefix(HttpMatchmakerManager._CoRequestGameList_d__7 __instance)
        {
            if (__instance.__1__state == 2)
            {
                if (HttpUtils.IsSuccess(__instance._request_5__3.responseCode))
                {
                    var responseHeader = __instance._request_5__3.GetResponseHeader("Client-Mods-Processed");
                    if (responseHeader == null)
                    {
                        AmongUsClient.Instance.LastDisconnectReason = DisconnectReasons.Custom;
                        AmongUsClient.Instance.LastCustomDisconnect = "This region doesn't support modded handshake.\nThe lobbies shown may not be compatible with your current mods.\nFor more info see https://reactor.gg/handshake";
                        DisconnectPopup.Instance.Show();
                    }
                }
            }
        }
    }
}
