using Hazel;
using UnityEngine;

namespace Reactor.Extensions
{
    public static class MessageExtensions
    {
        private const float MIN = -40f;
        private const float MAX = 40f;

        private static float ReverseLerp(float t)
        {
            return Mathf.Clamp((t - MIN) / (MAX - MIN), 0f, 1f);
        }

        public static void Write(this MessageWriter writer, Vector2 value)
        {
            var x = (ushort) (ReverseLerp(value.x) * ushort.MaxValue);
            var y = (ushort) (ReverseLerp(value.y) * ushort.MaxValue);

            writer.Write(x);
            writer.Write(y);
        }

        public static Vector2 ReadVector2(this MessageReader reader)
        {
            var x = reader.ReadUInt16() / (float) ushort.MaxValue;
            var y = reader.ReadUInt16() / (float) ushort.MaxValue;

            return new Vector2(Mathf.Lerp(MIN, MAX, x), Mathf.Lerp(MIN, MAX, y));
        }

        // public static void Write(MessageWriter writer, InnerNetObject value)
        // {
        //     global::MessageExtensions.WriteNetObject(writer, value);
        // }

        // TODO OxygenFilter crashes, Mono.Cecil hates generic arguments, I have no idea why
        // public static T ReadNetObject<T>(MessageReader reader) where T : InnerNetObject
        // {
        //     return global::MessageExtensions.ReadNetObject<T>(reader);
        // }
    }
}
