using System;
using System.Linq;
using System.Reflection;

namespace Reactor.Networking.Attributes;

/// <summary>
/// Marks a plugin to be ignored during AMCI mod GUID registration and composite calculation.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public sealed class AmciIgnoreAttribute : Attribute
{
    internal static bool IsIgnored(Type type)
    {
        if (type.GetCustomAttribute<AmciIgnoreAttribute>() != null)
        {
            return true;
        }

        var metadata = type.Assembly.GetCustomAttributes<AssemblyMetadataAttribute>().SingleOrDefault(x => x.Key == "Reactor.AmciIgnore");
        if (metadata is { Value: not null } && bool.TryParse(metadata.Value, out var ignored))
        {
            return ignored;
        }

        return false;
    }
}
