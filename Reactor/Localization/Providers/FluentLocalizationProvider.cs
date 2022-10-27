using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using Il2CppInterop.Runtime;
using Il2CppInterop.Runtime.InteropTypes;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Linguini.Bundle;
using Linguini.Bundle.Builder;
using Linguini.Bundle.Errors;
using Linguini.Shared.Types.Bundle;
using Linguini.Syntax.Ast;
using Reactor.Localization.Extensions;
using Reactor.Localization.Utilities;
using Reactor.Utilities;
using Object = Il2CppSystem.Object;

namespace Reactor.Localization.Providers;

/// <summary>
/// Represents a localization provider that uses Fluent.
/// </summary>
public class FluentLocalizationProvider : LocalizationProvider
{
    private readonly Func<CultureInfo, IEnumerable<Resource>> _loadResources;
    private readonly List<FluentBundle> _bundles = new();
    private readonly Dictionary<StringNames, string> _ids = new();
    private readonly Dictionary<StringNames, string[]> _arguments = new();

    /// <summary>
    /// Gets or sets the fallback language.
    /// </summary>
    public SupportedLangs FallbackLanguage { get; set; } = SupportedLangs.English;

    /// <summary>
    /// Initializes a new instance of the <see cref="FluentLocalizationProvider"/> class.
    /// </summary>
    /// <param name="loadResources">The implementation of resource loading.</param>
    public FluentLocalizationProvider(Func<CultureInfo, IEnumerable<Resource>> loadResources)
    {
        _loadResources = loadResources;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FluentLocalizationProvider"/> class that loads resources from the calling assembly.
    /// </summary>
    /// <param name="prefix">The prefix of resource file names to load.</param>
    /// <returns>a <see cref="FluentLocalizationProvider"/>.</returns>
    public static FluentLocalizationProvider FromEmbeddedResources(string prefix) => FromEmbeddedResources(Assembly.GetCallingAssembly(), prefix);

    /// <summary>
    /// Initializes a new instance of the <see cref="FluentLocalizationProvider"/> class that loads resources from the specified <paramref name="assembly"/>.
    /// </summary>
    /// <param name="assembly">The assembly to load resources from.</param>
    /// <param name="prefix">The prefix of resource file names to load.</param>
    /// <returns>a <see cref="FluentLocalizationProvider"/>.</returns>
    public static FluentLocalizationProvider FromEmbeddedResources(Assembly assembly, string prefix)
    {
        return new FluentLocalizationProvider(cultureInfo =>
        {
            return assembly.GetManifestResourceNames()
                .Where(n => n.StartsWith($"{prefix}.{cultureInfo.Name}.", StringComparison.Ordinal) && n.EndsWith(".ftl", StringComparison.Ordinal))
                .Select(n => (Resource) new StreamReader(assembly.GetManifestResourceStream(n)!));
        });
    }

    /// <summary>
    /// Registers a <see cref="StringNames"/> for use with base game apis.
    /// </summary>
    /// <param name="id">The id of a Fluent message.</param>
    /// <returns>A <see cref="StringNames"/>.</returns>
    public StringNames Register(string id)
    {
        var stringNames = CustomStringName.Create();
        _ids[stringNames] = id;

        return stringNames;
    }

    /// <summary>
    /// Registers a <see cref="StringNames"/> for use with base game apis with overriden format argument names.
    /// </summary>
    /// <param name="id">The id of a Fluent message.</param>
    /// <param name="argumentNames">An array of Fluent variable names in the same order as format arguments.</param>
    /// <returns>A <see cref="StringNames"/>.</returns>
    public StringNames Register(string id, params string[] argumentNames)
    {
        var stringNames = Register(id);
        _arguments[stringNames] = argumentNames;

        return stringNames;
    }

    /// <inheritdoc/>
    public override int Priority => ReactorPriority.Normal;

    /// <inheritdoc />
    public override void OnLanguageChanged(SupportedLangs newLanguage)
    {
        if (CurrentLanguage == newLanguage) return;

        _bundles.Clear();
        _bundles.Add(Load(newLanguage.ToCultureInfo()));
        if (newLanguage != FallbackLanguage) _bundles.Add(Load(FallbackLanguage.ToCultureInfo()));
    }

    private FluentBundle Load(CultureInfo culture)
    {
        return LinguiniBuilder.Builder()
            .CultureInfo(culture)
            .AddResources(_loadResources(culture))
            .SetUseIsolating(false)
            .UncheckedBuild();
    }

    private string GetMsg(string id, IDictionary<string, IFluentType>? args)
    {
        foreach (var bundle in _bundles)
        {
            if (bundle.TryGetMsg(id, null, args, out var errors, out var message))
            {
                if (errors.Count > 0)
                {
                    throw new LinguiniException(errors);
                }

                return message;
            }
        }

        throw new KeyNotFoundException();
    }

    private static bool Is<T>(Il2CppObjectBase o)
    {
        return IL2CPP.il2cpp_class_is_assignable_from(Il2CppClassPointerStore<T>.NativeClassPtr, IL2CPP.il2cpp_object_get_class(o.Pointer));
    }

    private static bool TryUnbox<T>(Il2CppObjectBase o, [NotNullWhen(true)] out T? result) where T : unmanaged
    {
        if (!Is<T>(o))
        {
            result = null;
            return false;
        }

        result = o.Unbox<T>();
        return true;
    }

    private static IFluentType ToFluentType(Object o)
    {
        if (Is<string>(o)) return new FluentString(IL2CPP.Il2CppStringToManaged(o.Pointer));

        if (TryUnbox<byte>(o, out var @byte)) return (FluentNumber) @byte;
        if (TryUnbox<sbyte>(o, out var @sbyte)) return (FluentNumber) @sbyte;
        if (TryUnbox<short>(o, out var @short)) return (FluentNumber) @short;
        if (TryUnbox<int>(o, out var @int)) return (FluentNumber) @int;
        if (TryUnbox<long>(o, out var @long)) return (FluentNumber) @long;
        if (TryUnbox<ushort>(o, out var @ushort)) return (FluentNumber) @ushort;
        if (TryUnbox<uint>(o, out var @uint)) return (FluentNumber) @uint;
        if (TryUnbox<ulong>(o, out var @ulong)) return (FluentNumber) @ulong;
        if (TryUnbox<float>(o, out var @float)) return (FluentNumber) @float;
        if (TryUnbox<double>(o, out var @double)) return (FluentNumber) @double;

        return new FluentString(o.ToString());
    }

    private static IFluentType ToFluentType(object o)
    {
        return o switch
        {
            string str => (FluentString) str,
            byte num => (FluentNumber) num,
            sbyte num => (FluentNumber) num,
            short num => (FluentNumber) num,
            ushort num => (FluentNumber) num,
            int num => (FluentNumber) num,
            uint num => (FluentNumber) num,
            long num => (FluentNumber) num,
            ulong num => (FluentNumber) num,
            double num => (FluentNumber) num,
            float num => (FluentNumber) num,
            _ => (FluentString) o.ToString()!,
        };
    }

    /// <summary>
    /// Returns the localized text for the given fluent message <paramref name="id"/>.
    /// </summary>
    /// <param name="id">The id of the message.</param>
    /// <param name="args">The arguments used for formatting.</param>
    /// <returns>Text localized and formatted by fluent.</returns>
    public string GetText(string id, params (string, object)[] args)
    {
        var fluentArgs = new Dictionary<string, IFluentType>(args.Length);
        foreach (var (key, value) in args)
        {
            fluentArgs[key] = ToFluentType(value);
        }

        return GetMsg(id, fluentArgs);
    }

    /// <inheritdoc/>
    public override bool TryGetTextFormatted(StringNames stringName, Il2CppReferenceArray<Object> parts, [NotNullWhen(true)] out string? result)
    {
        if (_ids.TryGetValue(stringName, out var id))
        {
            var hasArgumentsOverride = _arguments.TryGetValue(stringName, out var arguments);
            if (hasArgumentsOverride)
            {
                if (arguments!.Length != parts.Length)
                    throw new InvalidOperationException($"Arguments override for {id} has an invalid count of arguments");
            }

            var args = new Dictionary<string, IFluentType>(parts.Length);
            for (var i = 0; i < parts.Count; i++)
            {
                var part = parts[i];

                if (hasArgumentsOverride)
                {
                    args[arguments![i]] = ToFluentType(part);
                }
                else
                {
                    // Fluent doesn't allow positional variables nor starting variable names with numbers so we have to use a,b,c,d... instead
                    if (i > 25) throw new InvalidOperationException("Too many format parts");
                    var c = (char) ('a' + i);
                    args[c.ToString()] = ToFluentType(part);
                }
            }

            result = GetMsg(id, args);
            return true;
        }

        result = null;
        return false;
    }

    /// <inheritdoc/>
    public override bool TryGetText(StringNames stringName, [NotNullWhen(true)] out string? result)
    {
        if (_ids.TryGetValue(stringName, out var id))
        {
            result = GetMsg(id, null);
            return true;
        }

        result = null;
        return false;
    }
}
