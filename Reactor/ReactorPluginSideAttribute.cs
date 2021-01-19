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
            if (side == PluginSide.ServerOnly)
            {
                throw new ArgumentException("BepInEx plugin can't be server only!");
            }

            Side = side;
        }
    }

    /// <summary>
    /// Plugin side used in modded handshake
    /// </summary>
    public enum PluginSide : byte
    {
        /// <summary>
        /// Required by both sides, reject connection if missing on the other side
        /// </summary>
        Both,

        /// <summary>
        /// Required only by client
        /// </summary>
        ClientOnly,

        /// <summary>
        /// Required only by server
        /// </summary>
        ServerOnly
    }
}
