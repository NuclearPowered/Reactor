using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AmongUs.GameOptions;
using AmongUs.Matchmaking;
using HarmonyLib;
using InnerNet;

namespace Reactor.Networking.Patches
{
    internal class ModRegistrationPatch
    {
        [HarmonyPatch(typeof(CurrentModRegistration), nameof(CurrentModRegistration.UpdateFilterSetWithModRegistrationSettings))]
        public static class CurrentModRegistrationPatch
        {
            public static void Prefix(GameFilterSet filterSet)
            {
                for (int i = filterSet.Filters.Count - 1; i >= 0; i--)
                {
                    if (filterSet.Filters[i].Key == "mod")
                    {
                        filterSet.Filters.RemoveAt(i);
                    }
                }
            }
        }

        [HarmonyPatch(typeof(InnerNetClient), nameof(InnerNetClient.HostGame), typeof(IGameOptions), typeof(GameFilterOptions))]
        public static class LocalGamePatch
        {
            private static string _savedGuid;

            public static void Prefix()
            {
                if (AmongUsClient.Instance != null
                    && AmongUsClient.Instance.NetworkMode != NetworkModes.OnlineGame)
                {
                    _savedGuid = CurrentModRegistration.ModRegistrationGuidString;
                    CurrentModRegistration.ModRegistrationGuidString = "";
                }
            }

            public static void Postfix()
            {
                if (_savedGuid != null)
                {
                    CurrentModRegistration.ModRegistrationGuidString = _savedGuid;
                    _savedGuid = null;
                }
            }
        }
    }
}
