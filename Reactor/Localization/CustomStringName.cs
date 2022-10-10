using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using HarmonyLib;
using Il2CppInterop.Runtime.InteropTypes.Arrays;

namespace Reactor.Localization;

/// <summary>
/// Provides a way to use custom static StringNames.
/// </summary>
public class CustomStringName
{
    private static int _lastId = -1;

    private static readonly List<CustomStringName> _list = new();

    /// <summary>
    /// Gets a list of all <see cref="CustomStringName"/>s.
    /// </summary>
    public static IReadOnlyList<CustomStringName> List => _list.AsReadOnly();

    /// <summary>
    /// Registers a <see cref="CustomStringName"/> with specified <paramref name="value"/>.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns>A <see cref="CustomStringName"/>.</returns>
    public static CustomStringName Register(string value)
    {
        var customStringName = new CustomStringName(_lastId--, value);
        _list.Add(customStringName);

        return customStringName;
    }

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
    /// Defines an implicit conversion of a <see cref="CustomStringName"/> to a <see cref="StringNames"/>.
    /// </summary>
    /// <param name="name">The <see cref="CustomStringName"/>.</param>
    /// <returns>A <see cref="StringNames"/>.</returns>
    public static implicit operator StringNames(CustomStringName name) => (StringNames) name.Id;

    /// <summary>
    /// Defines an explicit conversion of a <see cref="StringNames"/> to a <see cref="CustomStringName"/>.
    /// </summary>
    /// <param name="name">The <see cref="StringNames"/>.</param>
    /// <returns>A <see cref="CustomStringName"/>.</returns>
    public static explicit operator CustomStringName?(StringNames name) => List.SingleOrDefault(x => x.Id == (int) name);

    [HarmonyPatch(typeof(TranslationController), nameof(TranslationController.GetString), typeof(StringNames), typeof(Il2CppReferenceArray<Il2CppSystem.Object>))]
    private static class GetStringPatch
    {
        public static bool Prefix(StringNames id, Il2CppReferenceArray<Il2CppSystem.Object> parts, ref string __result)
        {
            var customStringName = (CustomStringName?) id;

            if (customStringName != null)
            {
                __result = string.Format(CultureInfo.InvariantCulture, customStringName.Value, parts);
                return false;
            }

            return true;
        }
    }
}
