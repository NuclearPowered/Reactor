namespace Reactor.Networking;

internal static class HandshakePopup
{
    public const string Message =
        """
        This server doesn't support Reactor's modded handshake.
        The lobbies shown may be incompatible with your current mods.
        For more info see <link=https://reactor.gg/handshake>reactor.gg/handshake</link>
        """;

    public static void Show()
    {
        DisconnectPopup.Instance.ShowCustom(Message);
    }
}
