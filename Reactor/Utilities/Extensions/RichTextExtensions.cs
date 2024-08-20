namespace Reactor.Utilities.Extensions;

/// <summary>
/// Provides extension methods for TestMeshPro's Rich Text.
/// </summary>
internal static class RichTextExtensions
{
    private static string Wrap(this string text, string tag)
    {
        return $"<{tag}>{text}</{tag}>";
    }

    private static string Wrap(this string text, string tag, string value)
    {
        return $"<{tag}={value}>{text}</{tag}>";
    }

    public static string Align(this string text, string value)
    {
        return text.Wrap("align", value);
    }

    public static string Color(this string text, string value)
    {
        return text.Wrap("color", value);
    }

    public static string Size(this string text, string value)
    {
        return text.Wrap("size", value);
    }

    public static string EscapeRichText(this string text)
    {
        return text
            .Replace("<noparse>", string.Empty)
            .Replace("</noparse>", string.Empty)
            .Wrap("noparse");
    }
}
