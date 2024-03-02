using Reactor.Utilities.UI;
using UnityEngine;

namespace Reactor.Networking;

internal static class MakePublicDisallowedPopup
{
    public const string Message =
        """
        You can't make public lobbies on servers that don't support modded handshake.
        For more info see <link=https://reactor.gg/handshake>reactor.gg/handshake</link>
        """;

    private static ReactorPopup? _popup;

    public static void Show()
    {
        if (_popup == null)
        {
            _popup = ReactorPopup.Create(nameof(MakePublicDisallowedPopup));
            _popup.Background.transform.localPosition = new Vector3(0, 0.25f, 0);
            _popup.Background.size = new Vector2(7.5f, 1.5f);
            _popup.BackButton.transform.SetLocalY(-0.1f);
        }

        var message = ReactorConfig.MakePublicDisallowedPopupMessage.Value;
        if (string.IsNullOrEmpty(message))
        {
            message = Message;
        }

        _popup.Show(message);
    }
}
