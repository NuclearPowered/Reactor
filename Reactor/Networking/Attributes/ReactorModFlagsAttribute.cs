using System;
using System.Linq;
using System.Reflection;

namespace Reactor.Networking.Attributes;

/// <summary>
/// Describes the <see cref="ModFlags"/> of the annotated plugin class.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public sealed class ReactorModFlagsAttribute : Attribute
{
    /// <summary>
    /// Gets flags of the mod.
    /// </summary>
    public ModFlags Flags { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ReactorModFlagsAttribute"/> class.
    /// </summary>
    /// <param name="flags">Flags of the mod.</param>
    public ReactorModFlagsAttribute(ModFlags flags)
    {
        Flags = flags;
    }

    internal static ModFlags GetModFlags(Type type)
    {
        var attribute = type.GetCustomAttribute<ReactorModFlagsAttribute>();
        if (attribute != null)
        {
            return attribute.Flags;
        }

        var metadataAttribute = type.Assembly.GetCustomAttributes<AssemblyMetadataAttribute>().SingleOrDefault(x => x.Key == "Reactor.ModFlags");
        if (metadataAttribute is { Value: not null })
        {
            return Enum.Parse<ModFlags>(metadataAttribute.Value);
        }

        return ModFlags.None;
    }
}
