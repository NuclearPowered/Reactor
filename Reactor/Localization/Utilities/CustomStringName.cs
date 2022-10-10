using Reactor.Localization.Providers;

namespace Reactor.Localization.Utilities;

/// <summary>
/// Provides a way to use custom static StringNames.
/// </summary>
public class CustomStringName
{
    private static int _lastId = int.MinValue + 1;

    /// <summary>
    /// Gets the id.
    /// </summary>
    public int Id { get; }

    /// <summary>
    /// Gets the value.
    /// </summary>
    public string Value { get; }

    private CustomStringName(int id, string value)
    {
        Id = id;
        Value = value;
    }

    /// <summary>
    /// Registers a <see cref="CustomStringName"/> with specified <paramref name="value"/>.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns>A <see cref="CustomStringName"/>.</returns>
    public static CustomStringName Register(string value)
    {
        var id = _lastId++;
        var customStringName = new CustomStringName(id, value);

        HardCodedLocalizationProvider.Strings[(StringNames) id] = customStringName;

        return customStringName;
    }

    /// <summary>
    /// Defines an implicit conversion of a <see cref="CustomStringName"/> to a <see cref="StringNames"/>.
    /// </summary>
    /// <param name="name">The <see cref="CustomStringName"/>.</param>
    /// <returns>A <see cref="StringNames"/>.</returns>
    public static implicit operator StringNames(CustomStringName name) => (StringNames) name.Id;

    /// <summary>
    /// Defines an implicit conversion of a <see cref="CustomStringName"/> to a <see cref="string"/>.
    /// </summary>
    /// <param name="name">The <see cref="CustomStringName"/>.</param>
    /// <returns>A <see cref="string"/>.</returns>
    public static implicit operator string(CustomStringName name) => name.Value;
}
