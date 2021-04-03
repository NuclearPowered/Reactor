using System;

namespace Reactor
{
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
    }
}
