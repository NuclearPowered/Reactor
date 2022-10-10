using System.Collections.Generic;
using System.Linq;

namespace Reactor.Localization;

/// <summary>
/// Handles custom <see cref="StringNames"/> localization.
/// </summary>
public static class LocalizationManager
{
    private static readonly List<LocalizationProvider> _providers = new();

    internal static readonly Dictionary<SystemTypes, StringNames> SystemTypes = new();
    internal static readonly Dictionary<TaskTypes, StringNames> TaskTypes = new();

    /// <summary>
    /// Registers a new <see cref="LocalizationProvider"/> to be used for obtaining translations.
    /// </summary>
    /// <param name="provider">A <see cref="LocalizationProvider"/> instance.</param>
    public static void Register(LocalizationProvider provider)
    {
        if (!_providers.Contains(provider)) _providers.Add(provider);
    }

    /// <summary>
    /// Unregisters a <see cref="LocalizationProvider"/>.
    /// </summary>
    /// <param name="provider">The <see cref="LocalizationProvider"/> to unregister.</param>
    public static void Unregister(LocalizationProvider provider)
    {
        _providers.Remove(provider);
    }

    /// <summary>
    /// Unregisters all <see cref="LocalizationProvider"/>s of the given type.
    /// </summary>
    /// <typeparam name="T">The type of the <see cref="LocalizationProvider"/>s to be unregistered.</typeparam>
    public static void UnregisterAllByType<T>() where T : LocalizationProvider
    {
        _providers.RemoveAll(x => x is T);
    }

    /// <summary>
    /// Assigns a <see cref="global::SystemTypes"/> to a <see cref="global::StringNames"/>.
    /// </summary>
    /// <param name="systemType">The <see cref="global::SystemTypes"/>.</param>
    /// <param name="stringName">The <see cref="global::StringNames"/>.</param>
    public static void AssignSystemType(SystemTypes systemType, StringNames stringName)
    {
        if (SystemTypes.ContainsKey(systemType))
        {
            Warning($"Assigning SystemType {stringName} that is already assigned");
        }

        SystemTypes[systemType] = stringName;
    }

    /// <summary>
    /// Assigns a <see cref="global::TaskTypes"/> to a <see cref="global::StringNames"/>.
    /// </summary>
    /// <param name="taskTypes">The <see cref="global::TaskTypes"/>.</param>
    /// <param name="stringName">The <see cref="global::StringNames"/>.</param>
    public static void AssignTaskType(TaskTypes taskTypes, StringNames stringName)
    {
        if (TaskTypes.ContainsKey(taskTypes))
        {
            Warning($"Assigning SystemType {stringName} that is already assigned");
        }

        TaskTypes[taskTypes] = stringName;
    }

    internal static bool TryGetText(StringNames stringName, SupportedLangs language, out string text)
    {
        foreach (var provider in _providers.OrderByDescending(p => p.Priority))
        {
            if (provider.CanHandle(stringName))
            {
                text = provider.GetText(stringName, language);
                return true;
            }
        }

        text = string.Empty;
        return false;
    }
}
