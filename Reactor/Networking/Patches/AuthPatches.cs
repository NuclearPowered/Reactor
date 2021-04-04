using System.Linq;
using HarmonyLib;
using Hazel.Dtls;
using Il2CppSystem.Security.Cryptography.X509Certificates;

namespace Reactor.Networking.Patches
{
    internal static class AuthPatches
    {
        [HarmonyPatch(typeof(X509Certificate2Collection), nameof(X509Certificate2Collection.Contains))]
        public static class AcceptCustomCertificatesPatch
        {
            public static bool Prefix(ref bool __result)
            {
                var caller = new Il2CppSystem.Diagnostics.StackTrace().GetFrames().First().GetMethod();

                if (caller.DeclaringType.FullName == typeof(DtlsUnityConnection).FullName && caller.Name == nameof(DtlsUnityConnection.ProcessHandshake))
                {
                    __result = true;
                    return false;
                }

                return true;
            }
        }
    }
}
