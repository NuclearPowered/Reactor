using HarmonyLib;
using Il2CppSystem.Security.Cryptography.X509Certificates;

namespace Reactor.Networking.Patches
{
    internal static class AuthPatches
    {
        [HarmonyPatch(typeof(X509Certificate2Collection), nameof(X509Certificate2Collection.Contains))]
        public static class AcceptCustomCertificatesPatch
        {
            public static bool Prefix(X509Certificate2Collection __instance, ref bool __result)
            {
                if (AuthManager.Instance && AuthManager.Instance.connection != null && AuthManager.Instance.connection.serverCertificates.Equals(__instance))
                {
                    __result = true;
                    return false;
                }

                return true;
            }
        }
    }
}
