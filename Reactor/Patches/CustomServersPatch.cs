using HarmonyLib;
using Hazel.Udp;
using Il2CppSystem.Net;
using InnerNet;

namespace Reactor.Patches
{
    internal static class CustomServersPatch
    {
        /// <summary>
        /// Send the account id only to Among Us official servers
        /// </summary>
        [HarmonyPatch(typeof(InnerNetClient), nameof(InnerNetClient.GetConnectionData))]
        public static class DontSendAccountIdPatch
        {
            public static void Prefix(ref bool useDtlsLayout)
            {
                var serverManager = ServerManager.Instance;
                if (!serverManager || serverManager.CurrentRegion?.TryCast<DnsRegionInfo>() is not { } regionInfo || !regionInfo.Fqdn.EndsWith("among.us"))
                {
                    useDtlsLayout = false;
                }
            }
        }

        /// <summary>
        /// Encrypt connection only to Among Us official servers
        /// </summary>
        [HarmonyPatch(typeof(AuthManager), nameof(AuthManager.CreateDtlsConnection))]
        public static class DontEncryptCustomServersPatch
        {
            public static bool Prefix(ref UnityUdpClientConnection __result, string targetIp, ushort targetPort)
            {
                var serverManager = ServerManager.Instance;
                if (serverManager.CurrentRegion.TryCast<DnsRegionInfo>() is not { } regionInfo || !regionInfo.Fqdn.EndsWith("among.us"))
                {
                    var remoteEndPoint = new IPEndPoint(IPAddress.Parse(targetIp), targetPort - 3);
                    __result = new UnityUdpClientConnection(remoteEndPoint);
                    return false;
                }

                return true;
            }
        }
    }
}
