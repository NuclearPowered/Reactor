using Reactor.Localization.Providers;

namespace Reactor.Localization.Utilities;

/// <summary>
/// Provides a way to use custom static <see cref="StringNames"/>.
/// </summary>
public static class CustomStringName
{
    private static int _lastId = int.MinValue + 1;

    /// <summary>
    /// Creates an returns a unique <see cref="StringNames"/> value.
    /// </summary>
    /// <returns>A unique <see cref="StringNames"/>.</returns>
    public static StringNames Create()
    {
        var id = _lastId++;
        return (StringNames) id;
    }

    /// <summary>
    /// Creates, registeres and returns a unique <see cref="StringNames"/> value.
    /// </summary>
    /// <param name="text">The text.</param>
    /// <returns>A unique <see cref="StringNames"/>.</returns>
    public static StringNames CreateAndRegister(string text)
    {
        var stringName = Create();
        HardCodedLocalizationProvider.RegisterStringName(stringName, text);
        return stringName;
    }
}
