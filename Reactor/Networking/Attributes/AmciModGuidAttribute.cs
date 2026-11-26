using System;
using System.Linq;
using System.Reflection;

namespace Reactor.Networking.Attributes;

/// <summary>
/// Registers the annotated plugin with the official AMCI  protocol.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public sealed class AmciModGuidAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AmciModGuidAttribute"/> class.
    /// </summary>
    /// <param name="guid">The self-assigned v4 mod GUID of the mod.</param>
    public AmciModGuidAttribute(string guid)
    {
        Guid = Guid.Parse(guid);
    }

    public Guid Guid { get; }

    internal static Guid? GetGuid(Type type)
    {
        var attribute = type.GetCustomAttribute<AmciModGuidAttribute>();
        if (attribute != null)
        {
            return attribute.Guid;
        }

        var metadataAttribute = type.Assembly.GetCustomAttributes<AssemblyMetadataAttribute>().SingleOrDefault(x => x.Key == "Reactor.AmciModGuid");
        if (metadataAttribute is { Value: not null } && Guid.TryParse(metadataAttribute.Value, out var guid))
        {
            return guid;
        }

        return null;
    }
}
