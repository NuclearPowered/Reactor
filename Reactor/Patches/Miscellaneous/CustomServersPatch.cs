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
    private static bool IsCurrentServerOfficial()
    {
        const string Domain = "among.us";

        return ServerManager.Instance.CurrentRegion?.TryCast<StaticHttpRegionInfo>() is { } regionInfo &&
               regionInfo.PingServer.EndsWith(Domain, StringComparison.Ordinal) &&
               regionInfo.Servers.All(serverInfo => serverInfo.Ip.EndsWith(Domain, StringComparison.Ordinal));
    }

    [HarmonyPatch]
    public static class DisableAuthServerPatch
    {
        public static IEnumerable<MethodBase> TargetMethods() =>
        [
            StateMachineWrapper<AuthManager>.GetStateMachineMoveNext(nameof(AuthManager.CoConnect))!,
            StateMachineWrapper<AuthManager>.GetStateMachineMoveNext(nameof(AuthManager.CoWaitForNonce))!
        ];

        public static bool Prefix(ref bool __result)
        {
            if (IsCurrentServerOfficial())
            {
                return true;
            }

            __result = false;
            return false;
        }
    }

    [HarmonyPatch]
    public static class EnableUdpPatch
    {
        public static MethodBase TargetMethod()
        {
            return StateMachineWrapper<AmongUsClient>.GetStateMachineMoveNext(nameof(AmongUsClient.CoJoinOnlinePublicGame))!;
        }

        public static void Prefix(Il2CppObjectBase __instance)
        {
            var stateMachine = new StateMachineWrapper<AmongUsClient>(__instance);

            // Skip to state 1 which just calls CoJoinOnlineGameDirect
            if (stateMachine.State == 0 && !ServerManager.Instance.IsHttp)
            {
                stateMachine.State = 1;
                var lambdaType = stateMachine.GetParameter<Il2CppObjectBase>("__8__1").GetType();
                var newDisplayClass = Activator.CreateInstance(lambdaType);
                if (newDisplayClass == null)
                {
                    throw new InvalidOperationException($"Could not create display class of type '{lambdaType}'.");
                }

                var displayClass = new CompilerGeneratedObjectWrapper(newDisplayClass);
                displayClass.SetField("matchmakerToken", string.Empty);

                stateMachine.SetParameter("__8__1", newDisplayClass);
            }
        }
    }
}
