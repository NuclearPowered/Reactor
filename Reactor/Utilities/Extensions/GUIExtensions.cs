using UnityEngine;

namespace Reactor.Utilities.Extensions;

/// <summary>
/// Provides extension methods for unity imgui.
/// </summary>
public static class GUIExtensions
{
    /// <summary>
    /// Clamps <paramref name="rect"/> to the screen size.
    /// </summary>
    /// <param name="rect">The <see cref="Rect"/> to clamp.</param>
    /// <returns>A clamped rect.</returns>
    public static Rect ClampScreen(this Rect rect)
    {
        rect.x = Mathf.Clamp(rect.x, 0, Screen.width - rect.width);
        rect.y = Mathf.Clamp(rect.y, 0, Screen.height - rect.height);

        return rect;
    }

    /// <summary>
    /// Set <paramref name="rect"/>'s <see cref="Rect.width"/> and <see cref="Rect.height"/> to 0.
    /// </summary>
    /// <param name="rect">The <see cref="Rect"/> to reset.</param>
    /// <returns>A rect with reset size.</returns>
    public static Rect ResetSize(this Rect rect)
    {
        rect.width = rect.height = 0;

        return rect;
    }

    /// <summary>
    /// Creates a <see cref="Sprite"/> from the <paramref name="texture"/>.
    /// </summary>
    /// <param name="texture">Texture from which to obtain the sprite graphic.</param>
    /// <param name="pivot">Sprite's pivot point relative to its graphic rectangle.</param>
    /// <param name="pixelsPerUnit">The number of pixels in the sprite that correspond to one unit in world space.</param>
    /// <returns>A sprite.</returns>
    public static Sprite CreateSprite(this Texture2D texture, Vector2? pivot = null, float pixelsPerUnit = 100f)
    {
        return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), pivot ?? new Vector2(0.5f, 0.5f), pixelsPerUnit);
    }

    /// <summary>
    /// Sets <paramref name="rectTransform"/>'s width and height.
    /// </summary>
    /// <param name="rectTransform">The <see cref="RectTransform"/> to set the size of.</param>
    /// <param name="width">The width.</param>
    /// <param name="height">The height.</param>
    public static void SetSize(this RectTransform rectTransform, float width, float height)
    {
        rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
        rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
    }
}
