using System.Collections.Generic;
using Il2CppInterop.Runtime.InteropTypes.Arrays;

namespace Reactor.Localization;

/// <summary>
/// Handles custom <see cref="StringNames"/> localization.
/// </summary>
public static class LocalizationManager
{
    private static readonly List<LocalizationProvider> _providers = new();

    /// <summary>
    /// Registers a new <see cref="LocalizationProvider"/> to be used for obtaining translations.
    /// </summary>
    /// <param name="provider">A <see cref="LocalizationProvider"/> instance.</param>
    public static void Register(LocalizationProvider provider)
    {
        if (!_providers.Contains(provider))
        {
            _providers.Add(provider);

            if (TranslationController.InstanceExists)
            {
                provider.SetLanguage(TranslationController.Instance.currentLanguage.languageID);
            }

            _providers.Sort((a, b) => b.Priority - a.Priority);
        }
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

    internal static bool TryGetTextFormatted(StringNames stringName, Il2CppReferenceArray<Il2CppSystem.Object> parts, out string text)
    {
        foreach (var provider in _providers)
        {
            if (provider.TryGetTextFormatted(stringName, parts, out text!))
            {
                return true;
            }
        }

        text = string.Empty;
        return false;
    }

    internal static bool TryGetText(StringNames stringName, out string text)
    {
        foreach (var provider in _providers)
        {
            if (provider.TryGetText(stringName, out text!))
            {
                return true;
            }
        }

        text = string.Empty;
        return false;
    }

    internal static bool TryGetStringName(SystemTypes systemType, out StringNames stringName)
    {
        foreach (var provider in _providers)
        {
            if (provider.TryGetStringName(systemType, out var stringNameNullable))
            {
                stringName = stringNameNullable!.Value;
                return true;
            }
        }

        stringName = default;
        return false;
    }

    internal static bool TryGetStringName(TaskTypes taskTypes, out StringNames stringName)
    {
        foreach (var provider in _providers)
        {
            if (provider.TryGetStringName(taskTypes, out var stringNameNullable))
            {
                stringName = stringNameNullable!.Value;
                return true;
            }
        }

        stringName = default;
        return false;
    }

    internal static void OnLanguageChanged(SupportedLangs newLanguage)
    {
        foreach (var provider in _providers)
        {
            provider.SetLanguage(newLanguage);
        }
    }
}
