using UnityEngine;

namespace Reactor.Utilities.Extensions;

/// <summary>
/// Provides extension methods for unity.
/// </summary>
public static class UnityExtensions
{
    /// <summary>
    /// Returns the color as a hexadecimal string in the format "RRGGBBAA".
    /// </summary>
    /// <param name="color">The color to be converted.</param>
    /// <returns>Hexadecimal string representing the color.</returns>
    /// <remarks>https://docs.unity3d.com/ScriptReference/ColorUtility.ToHtmlStringRGBA.html.</remarks>
    public static string ToHtmlStringRGBA(this Color32 color)
    {
        return $"{color.r:X2}{color.g:X2}{color.b:X2}{color.a:X2}";
    }

    /// <inheritdoc cref="ToHtmlStringRGBA(UnityEngine.Color32)"/>
    public static string ToHtmlStringRGBA(this Color color)
    {
        return ((Color32) color).ToHtmlStringRGBA();
    }

    private static readonly int _outline = Shader.PropertyToID("_Outline");
    private static readonly int _outlineColor = Shader.PropertyToID("_OutlineColor");
    private static readonly int _addColor = Shader.PropertyToID("_AddColor");

    /// <summary>
    /// Sets the outline for renderer using the default Among Us shader.
    /// </summary>
    /// <param name="renderer">The renderer to set the color on.</param>
    /// <param name="color">The color or null to disable the outline.</param>
    public static void SetOutline(this Renderer renderer, Color? color)
    {
        renderer.material.SetFloat(_outline, color.HasValue ? 1 : 0);

        if (color.HasValue)
        {
            renderer.material.SetColor(_outlineColor, color.Value);
            renderer.material.SetColor(_addColor, color.Value);
        }
    }
}
