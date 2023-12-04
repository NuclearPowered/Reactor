using UnityEngine;
using Object = UnityEngine.Object;

namespace Reactor.Networking;

internal static class MakePublicDisallowedPopup
{
    public const string Message =
        """
        You can't make public lobbies on servers that don't support modded handshake.
        For more info see https://reactor.gg/handshake
        """;

    public static void Show()
    {
        var popup = Object.Instantiate(DiscordManager.Instance.discordPopup, Camera.main!.transform);
        var background = popup.transform.Find("Background").GetComponent<SpriteRenderer>();
        var size = background.size;
        size.x *= 2.5f;
        background.size = size;
        popup.TextAreaTMP.fontSizeMin = 2;
        popup.Show(Message);
    }
}
