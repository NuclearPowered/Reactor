using System;
using System.Linq;
using System.Reflection;

namespace Reactor;

/// <summary>
/// This attribute specifies plugin's side
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class ReactorPluginSideAttribute : Attribute
{
    public PluginSide Side { get; }

    public ReactorPluginSideAttribute(PluginSide side)
    {
        Side = side;
    }

    public static PluginSide GetPluginSide(Type type)
    {
        var attribute = type.GetCustomAttribute<ReactorPluginSideAttribute>();
        if (attribute != null)
        {
            return attribute.Side;
        }

        var metadataAttribute = type.Assembly.GetCustomAttributes<AssemblyMetadataAttribute>().SingleOrDefault(x => x.Key == "Reactor.PluginSide");
        if (metadataAttribute != null)
        {
            return Enum.Parse<PluginSide>(metadataAttribute.Value, true);
        }

        return PluginSide.Both;
    }
}
