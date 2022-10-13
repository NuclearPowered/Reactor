using System;
using System.Globalization;

namespace Reactor.Localization.Extensions;

/// <summary>
/// Provides extension methods for <see cref="SupportedLangs"/>.
/// </summary>
public static class SupportedLangsExtensions
{
    /// <summary>
    /// Gets a <see cref="CultureInfo"/> from the specified <paramref name="language"/>.
    /// </summary>
    /// <param name="language">The <see cref="SupportedLangs"/>.</param>
    /// <returns>a <see cref="CultureInfo"/>.</returns>
    public static CultureInfo ToCultureInfo(this SupportedLangs language)
    {
        return language switch
        {
            SupportedLangs.English => CultureInfo.GetCultureInfo("en"),
            SupportedLangs.Latam => CultureInfo.GetCultureInfo("es"),
            SupportedLangs.Brazilian => CultureInfo.GetCultureInfo("pt-BR"),
            SupportedLangs.Portuguese => CultureInfo.GetCultureInfo("pt"),
            SupportedLangs.Korean => CultureInfo.GetCultureInfo("ko"),
            SupportedLangs.Russian => CultureInfo.GetCultureInfo("ru"),
            SupportedLangs.Dutch => CultureInfo.GetCultureInfo("nl"),
            SupportedLangs.Filipino => CultureInfo.GetCultureInfo("fil"),
            SupportedLangs.French => CultureInfo.GetCultureInfo("fr"),
            SupportedLangs.German => CultureInfo.GetCultureInfo("de"),
            SupportedLangs.Italian => CultureInfo.GetCultureInfo("it"),
            SupportedLangs.Japanese => CultureInfo.GetCultureInfo("ja"),
            SupportedLangs.Spanish => CultureInfo.GetCultureInfo("es"),
            SupportedLangs.SChinese => CultureInfo.GetCultureInfo("zh-Hans"),
            SupportedLangs.TChinese => CultureInfo.GetCultureInfo("zh-Hant"),
            SupportedLangs.Irish => CultureInfo.GetCultureInfo("ga"),
            _ => throw new ArgumentOutOfRangeException(nameof(language), language, null),
        };
    }
}
