using Reactor.Utilities.UI;
using UnityEngine;

namespace Reactor.Networking;

internal static class HandshakePopup
{
    public const string Message =
        """
        This server doesn't support Reactor's modded handshake.
        The lobbies shown could be incompatible with your current mods.
        For more info see <link=https://reactor.gg/handshake>reactor.gg/handshake</link>
        """;

    private static ReactorPopup? _popup;

    public static void Show()
    {
        if (_popup == null)
        {
            _popup = ReactorPopup.Create(nameof(HandshakePopup));
            _popup.Background.transform.localPosition = new Vector3(0, 0.20f, 0);
            _popup.Background.size = new Vector2(6.5f, 1.7f);
            _popup.BackButton.transform.SetLocalY(-0.2f);
        }

        _popup.Show(Message);
    }
}
