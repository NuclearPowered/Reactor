using UnityEngine;

namespace Reactor.Utilities.Extensions;

/// <summary>
/// GUI utilities
/// </summary>
public static class GUIExtensions
{
    /// <summary>
    /// Clamp Rect to screen size
    /// </summary>
    public static Rect ClampScreen(this Rect rect)
    {
        rect.x = Mathf.Clamp(rect.x, 0, Screen.width - rect.width);
        rect.y = Mathf.Clamp(rect.y, 0, Screen.height - rect.height);

        return rect;
    }

    /// <summary>
    /// Reset Rect size
    /// </summary>
    public static Rect ResetSize(this Rect rect)
    {
        rect.width = rect.height = 0;

        return rect;
    }

    /// <summary>
    /// Create <see cref="Sprite"/> from <paramref name="tex"/>
    /// </summary>
    public static Sprite CreateSprite(this Texture2D tex)
    {
        return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100f);
    }

    public static void SetSize(this RectTransform rectTransform, float width, float height)
    {
        rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
        rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
    }
}
