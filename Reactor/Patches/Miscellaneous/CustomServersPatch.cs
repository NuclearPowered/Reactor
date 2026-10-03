using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using Il2CppInterop.Runtime.InteropTypes;
using Reactor.Utilities;

namespace Reactor.Patches.Miscellaneous;

internal static class CustomServersPatch
{
    // A code lookup can point to an official game while a custom region is selected.
    // Keep only the target IP from an official response so later lookups replace stale state.
    private static string? _officialFindGameIp;

    private static bool IsCurrentServerOfficial()
    {
        const string Domain = "among.us";

        return ServerManager.Instance.CurrentRegion?.TryCast<StaticHttpRegionInfo>() is { } regionInfo &&
               regionInfo.PingServer.EndsWith(Domain, StringComparison.Ordinal) &&
               regionInfo.Servers.All(serverInfo => serverInfo.Ip.EndsWith(Domain, StringComparison.Ordinal));
    }

    private static bool IsOfficialFindGameTarget()
    {
        return _officialFindGameIp is { Length: > 0 } targetIp &&
               string.Equals(AmongUsClient.Instance.networkAddress, targetIp, StringComparison.Ordinal);
    }

    [HarmonyPatch]
    public static class DisableAuthServerPatch
    {
        public static IEnumerable<MethodBase> TargetMethods() =>
        [
            Il2CppStateMachineWrapper<AuthManager>.GetStateMachineMoveNext(nameof(AuthManager.CoConnect))!,
            Il2CppStateMachineWrapper<AuthManager>.GetStateMachineMoveNext(nameof(AuthManager.CoWaitForNonce))!
        ];

        public static bool Prefix(ref bool __result)
        {
            if (IsCurrentServerOfficial() || IsOfficialFindGameTarget())
            {
                return true;
            }

            __result = false;
            return false;
        }
    }

    [HarmonyPatch(typeof(HttpMatchmakerManager), nameof(HttpMatchmakerManager.CoFindGameInfo))]
    public static class CacheFindGameTargetPatch
    {
        public static void Prefix([HarmonyArgument(1)] ref Il2CppSystem.Action<HttpMatchmakerManager.FindGameByCodeResponse, string> onGameInfo)
        {
            // Failed lookups do not invoke the callback, so clear the previous target up front.
            _officialFindGameIp = null;
            var originalCallback = onGameInfo;
            onGameInfo = (Action<HttpMatchmakerManager.FindGameByCodeResponse, string>) ((response, matchmakerToken) =>
            {
                // The queried region is still selected here. Cache before invoking the callback,
                // because invitation callbacks can start joining the game immediately.
                _officialFindGameIp = response != null && IsCurrentServerOfficial()
                    ? response.Game?.IPString
                    : null;
                originalCallback.Invoke(response!, matchmakerToken);
            });
        }
    }

    [HarmonyPatch]
    public static class EnableUdpPatch
    {
        public static MethodBase TargetMethod()
        {
            return Il2CppStateMachineWrapper<AmongUsClient>.GetStateMachineMoveNext(nameof(AmongUsClient.CoJoinOnlinePublicGame))!;
        }

        public static void Prefix(Il2CppObjectBase __instance)
        {
            var stateMachine = new Il2CppStateMachineWrapper<AmongUsClient>(__instance);

            // Skip to state 1 which just calls CoJoinOnlineGameDirect
            if (stateMachine.State == 0 && !ServerManager.Instance.IsHttp)
            {
                stateMachine.State = 1;
                var lambdaType = stateMachine.GetParameter<Il2CppObjectBase>("__8__1").GetType();
                var newDisplayClassObject = Activator.CreateInstance(lambdaType);
                if (newDisplayClassObject == null)
                {
                    throw new InvalidOperationException($"Could not create display class of type '{lambdaType}'.");
                }

                var wrappedDisplayClassObject = new Il2CppCompilerGeneratedObjectWrapper(newDisplayClassObject);
                wrappedDisplayClassObject.SetField("matchmakerToken", string.Empty);

                stateMachine.SetParameter("__8__1", newDisplayClassObject);
            }
        }
    }
}
