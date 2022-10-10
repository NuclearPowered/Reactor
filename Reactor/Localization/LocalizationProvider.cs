namespace Reactor.Localization;

/// <summary>
/// The required implementation of a localization provider class.
/// </summary>
public abstract class LocalizationProvider
{
    /// <summary>
    /// Gets the priority of this <see cref="LocalizationProvider"/>.
    /// The higher the priority is, the earlier it will be invoked in relation to other providers.
    /// <br/>
    /// You can use the <see cref="HarmonyLib.Priority"/> class for this value if you want to make this easier.
    /// </summary>
    public virtual int Priority => 0;

    /// <summary>
    /// Whether or not this <see cref="LocalizationProvider"/> can handle this <see cref="StringNames"/>
    /// <br/>
    /// Returning true here will subsequently call <see cref="GetText"/> with the same <see cref="StringNames"/>.
    /// </summary>
    /// <param name="stringName">The <see cref="StringNames"/> to check for.</param>
    /// <returns>A value indicating whether or not this <see cref="LocalizationProvider"/> can handle this <see cref="StringNames"/>.</returns>
    public abstract bool CanHandle(StringNames stringName);

    /// <summary>
    /// Returns the localized text for the given <see cref="StringNames"/>.
    /// </summary>
    /// <param name="stringName">The <see cref="StringNames"/> to localize.</param>
    /// <param name="language">The current language Among Us is set to.</param>
    /// <returns>The <see cref="string"/> representation of the given <see cref="StringNames"/>.</returns>
    public abstract string GetText(StringNames stringName, SupportedLangs language);
}
