using System;
using System.Globalization;
using Hazel;
using Hazel.Udp;
using Reactor.Networking.Serialization;
using UnityEngine;

namespace Reactor.Networking.Extensions;

/// <summary>
/// Provides extension methods for <see cref="MessageWriter"/> and <see cref="MessageReader"/>.
/// </summary>
public static class ExtraMessageExtensions
{
    private const float MIN = -50f;
    private const float MAX = 50f;
    private const float DIFF = MAX - MIN;

    private static float ReverseLerp(float t)
    {
        return Mathf.Clamp01((t - MIN) / DIFF);
    }

    /// <summary>
    /// Writes a <see cref="Vector2"/> to the <paramref name="writer"/>.
    /// </summary>
    /// <param name="writer">The <see cref="MessageWriter"/> to write to.</param>
    /// <param name="value">The <see cref="Vector2"/> to write.</param>
    public static void Write(this MessageWriter writer, Vector2 value)
    {
        var x = (ushort) (ReverseLerp(value.x) * ushort.MaxValue);
        var y = (ushort) (ReverseLerp(value.y) * ushort.MaxValue);

        writer.Write(x);
        writer.Write(y);
    }

    /// <summary>
    /// Writes an Enum value to the <paramref name="writer"/>.
    /// </summary>
    /// <param name="writer">The <see cref="MessageWriter"/> to write to.</param>
    /// <param name="value">The <see cref="Enum"/> to write.</param>
    public static void Write(this MessageWriter writer, Enum value) => writer.Serialize(Convert.ChangeType(value, Enum.GetUnderlyingType(value.GetType()), CultureInfo.InvariantCulture));

    /// <summary>
    /// Writes a long value to the <paramref name="writer"/>.
    /// </summary>
    /// <param name="writer">The <see cref="MessageWriter"/> to write to.</param>
    /// <param name="value">The <see cref="long"/> to write.</param>
    public static void Write(this MessageWriter writer, long value) => writer.Write(BitConverter.GetBytes(value));

    /// <summary>
    /// Writes a color value to the <paramref name="writer"/>.
    /// </summary>
    /// <param name="writer">The <see cref="MessageWriter"/> to write to.</param>
    /// <param name="value">The <see cref="Color32"/> to write.</param>
    public static void Write(this MessageWriter writer, Color32 value)
    {
        writer.Write(value.r);
        writer.Write(value.g);
        writer.Write(value.b);
        writer.Write(value.a);
    }

    /// <summary>
    /// Reads a <see cref="Vector2"/> from the <paramref name="reader"/>.
    /// </summary>
    /// <param name="reader">The <see cref="MessageReader"/> to read from.</param>
    /// <returns>A <see cref="Vector2"/> from the <paramref name="reader"/>.</returns>
    public static Vector2 ReadVector2(this MessageReader reader)
    {
        var x = reader.ReadUInt16() / (float) ushort.MaxValue;
        var y = reader.ReadUInt16() / (float) ushort.MaxValue;

        return new Vector2(Mathf.Lerp(MIN, MAX, x), Mathf.Lerp(MIN, MAX, y));
    }

    /// <summary>
    /// Reads an enum value and casts it to the specified type from a network message.
    /// </summary>
    /// <param name="reader">The <see cref="MessageReader"/> to read from.</param>
    /// <typeparam name="T">The <see cref="Enum"/> type to convert to.</typeparam>
    /// <returns>An <see cref="Enum"/> value from the <paramref name="reader"/>.</returns>
    public static T ReadEnum<T>(this MessageReader reader) where T : struct, Enum => (T) reader.ReadEnum(typeof(T));

    /// <summary>
    /// Reads an enum value from a network message.
    /// </summary>
    /// <param name="reader">The <see cref="MessageReader"/> to read from.</param>
    /// <param name="enumType">The type of the enum.</param>
    /// <returns>The resulting enum value from the <paramref name="reader"/>.</returns>
    public static object ReadEnum(this MessageReader reader, Type enumType) => Enum.ToObject(enumType, reader.Deserialize(Enum.GetUnderlyingType(enumType)));

    /// <summary>
    /// Reads a long value from a network message.
    /// </summary>
    /// <param name="reader">The <see cref="MessageReader"/> to read from.</param>
    /// <returns>The resulting long value from the <paramref name="reader"/>.</returns>
    public static long ReadInt64(this MessageReader reader) => BitConverter.ToInt64(reader.ReadBytes(8), 0);

    /// <summary>
    /// Reads a color value from a network message.
    /// </summary>
    /// <param name="reader">The <see cref="MessageReader"/> to read from.</param>
    /// <returns>The resulting Color32 value from the <paramref name="reader"/>.</returns>
    public static Color32 ReadColor32(this MessageReader reader) => new(reader.ReadByte(), reader.ReadByte(), reader.ReadByte(), reader.ReadByte());

    /// <summary>
    /// Reads a color value from a network message.
    /// </summary>
    /// <param name="reader">The <see cref="MessageReader"/> to read from.</param>
    /// <returns>The resulting Color value from the <paramref name="reader"/>.</returns>
    public static Color ReadColor(this MessageReader reader) => new(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());

    /// <summary>
    /// Sends a message on the <paramref name="connection"/> with an <paramref name="ackCallback"/>.
    /// </summary>
    /// <param name="connection">The connection to send the message on.</param>
    /// <param name="msg">The message to send.</param>
    /// <param name="ackCallback">The callback to invoke when this packet is acknowledged.</param>
    public static void Send(this UdpConnection connection, MessageWriter msg, Action ackCallback)
    {
        if (msg.SendOption != SendOption.Reliable)
            throw new InvalidOperationException("Message SendOption has to be Reliable.");

        var buffer = new byte[msg.Length];
        Buffer.BlockCopy(msg.Buffer, 0, buffer, 0, msg.Length);

        connection.ResetKeepAliveTimer();

        connection.AttachReliableID(buffer, 1, ackCallback);
        connection.WriteBytesToConnection(buffer, buffer.Length);
    }
}
