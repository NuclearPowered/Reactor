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
    /// Returns the localized text for the given <see cref="StringNames"/>.
    /// </summary>
    /// <param name="stringName">The <see cref="StringNames"/> to localize.</param>
    /// <param name="language">The current language Among Us is set to.</param>
    /// <param name="result">The <see cref="string"/> representation of the given <see cref="StringNames"/>.</param>
    /// <returns>Whether or not this <see cref="LocalizationProvider"/> can handle this <see cref="StringNames"/>.</returns>
    public virtual bool TryGetText(StringNames stringName, SupportedLangs language, out string? result)
    {
        result = null;
        return false;
    }

    /// <summary>
    /// Returns the <see cref="StringNames"/> equivalent for the given <see cref="SystemTypes"/>.
    /// </summary>
    /// <param name="systemType">The <see cref="SystemTypes"/> value.</param>
    /// <param name="result">The <see cref="string"/> representation of the given <see cref="StringNames"/>.</param>
    /// <returns>Whether or not this <see cref="LocalizationProvider"/> can handle this <see cref="StringNames"/>.</returns>
    public virtual bool TryGetStringName(SystemTypes systemType, out StringNames? result)
    {
        result = null;
        return false;
    }

    /// <summary>
    /// Returns the <see cref="StringNames"/> equivalent for the given <see cref="TaskTypes"/>.
    /// </summary>
    /// <param name="taskType">The <see cref="TaskTypes"/> value.</param>
    /// <param name="result">The <see cref="string"/> representation of the given <see cref="StringNames"/>.</param>
    /// <returns>Whether or not this <see cref="LocalizationProvider"/> can handle this <see cref="StringNames"/>.</returns>
    public virtual bool TryGetStringName(TaskTypes taskType, out StringNames? result)
    {
        result = null;
        return false;
    }
}
