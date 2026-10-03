using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AmongUs.HTTP;
using HarmonyLib;
using Il2CppInterop.Runtime.InteropTypes;
using Reactor.Utilities;

namespace Reactor.Patches.Miscellaneous;

internal static class CustomServersPatch
{
    // A code lookup can point to an official game while a custom region is selected.
    // Keep only the target IP from an official response so later lookups replace stale state.
    private static string? _officialFindGameIp;

    [ThreadStatic]
    private static RetryableWebRequest? _findGameResponseRequest;

    private static bool IsCurrentServerOfficial()
    {
        return IsOfficialRegion(ServerManager.Instance.CurrentRegion);
    }

    private static bool IsOfficialRegion(IRegionInfo? region)
    {
        const string Domain = "among.us";

        return region?.TryCast<StaticHttpRegionInfo>() is { } regionInfo &&
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

        public static bool Prefix(Il2CppObjectBase __instance, ref bool __result)
        {
            var officialRegion = IsCurrentServerOfficial();
            var officialTarget = IsOfficialFindGameTarget();
            var allowAuth = officialRegion || officialTarget;
            var stateMachine = new Il2CppStateMachineWrapper<AuthManager>(__instance);
            if (stateMachine.State == 0)
            {
                Info($"Authentication {__instance.GetType().Name}: {(allowAuth ? "running" : "skipped")} for {AmongUsClient.Instance.networkAddress} (official region: {officialRegion}, code lookup target: {officialTarget})");
            }

            if (allowAuth)
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
            // The game puts the queried region's Name in UntranslatedRegion. Snapshot its
            // domain classification because region-file reloads can replace CurrentRegion
            // and AvailableRegions while the HTTP request is pending.
            var officialRegionNames = ServerManager.Instance.AvailableRegions
                .Where(IsOfficialRegion)
                .Select(region => region.Name)
                .ToHashSet(StringComparer.Ordinal);
            Info($"Code lookup started with {officialRegionNames.Count} official regions");

            var originalCallback = onGameInfo;
            onGameInfo = (Action<HttpMatchmakerManager.FindGameByCodeResponse, string>) ((response, matchmakerToken) =>
            {
                var officialRegion = response?.UntranslatedRegion is { } regionName && officialRegionNames.Contains(regionName);
                var targetIp = response?.Game?.IPString;
                _officialFindGameIp = officialRegion ? targetIp : null;
                Info($"Code lookup completed: official region: {officialRegion}, target: {targetIp ?? "<none>"}");

                // A 401 retry updates the request header, but the game's callback still
                // captures the token obtained before that retry. Forward the accepted token.
                if (_findGameResponseRequest?.requestHeaders.TryGetValue("Authorization", out var authorization) == true &&
                    authorization.StartsWith("Bearer ", StringComparison.Ordinal) && authorization.Length > 7)
                {
                    var acceptedToken = authorization[7..];
                    if (!string.Equals(matchmakerToken, acceptedToken, StringComparison.Ordinal))
                    {
                        Info("Using refreshed matchmaking token from the successful code lookup request");
                    }

                    matchmakerToken = acceptedToken;
                }

                // Invitation callbacks can start joining immediately, so cache first.
                originalCallback.Invoke(response!, matchmakerToken);
            });
        }
    }

    [HarmonyPatch]
    public static class CaptureFindGameResponsePatch
    {
        public static MethodBase TargetMethod()
        {
            return Il2CppStateMachineWrapper<HttpMatchmakerManager>.GetStateMachineMoveNext(nameof(HttpMatchmakerManager.CoSendRequest))!;
        }

        public static void Prefix(Il2CppObjectBase __instance)
        {
            var stateMachine = new Il2CppStateMachineWrapper<HttpMatchmakerManager>(__instance);
            if (stateMachine.State != 0) return;

            var request = stateMachine.GetParameter<RetryableWebRequest>("request");
            const string LookupPath = "/api/games/";
            if (request.Method != "GET" || !Uri.TryCreate(request.Url, UriKind.Absolute, out var uri) ||
                !uri.AbsolutePath.StartsWith(LookupPath, StringComparison.Ordinal) ||
                !int.TryParse(uri.AbsolutePath[LookupPath.Length..], out _)) return;

            var originalCallback = request.successCallback;
            if (originalCallback == null) return;

            request.SetOrReplaceSuccessCallback((Action<string>) (response =>
            {
                // Keep the successful request in scope only during its synchronous callback.
                // Saving/restoring the context also handles nested callbacks and exceptions.
                var previousRequest = _findGameResponseRequest;
                _findGameResponseRequest = request;
                try
                {
                    originalCallback.Invoke(response);
                }
                finally
                {
                    _findGameResponseRequest = previousRequest;
                }
            }));
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
