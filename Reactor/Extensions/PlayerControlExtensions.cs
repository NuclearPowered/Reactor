using UnityEngine;

namespace Reactor.Extensions
{
    public static class PlayerControlExtensions
    {
        public static Color32 ToPlayerColor(this PlayerColor color)
        {
            return Palette.PlayerColors[(byte) color];
        }

        public static Color32 ToShadowColor(this PlayerColor color)
        {
            return Palette.ShadowColors[(byte) color];
        }
    }
}
