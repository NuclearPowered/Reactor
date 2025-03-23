using System;
using System.Globalization;
using Hazel;
using Hazel.Udp;
using UnityEngine;

namespace Reactor.Networking.Extensions;

/// <summary>
/// Provides extension methods for <see cref="MessageWriter"/> and <see cref="MessageReader"/>.
/// </summary>
public static class ExtraMessageExtensions
{
    private const float MIN = -50f;
    private const float MAX = 50f;

    private static float ReverseLerp(float t)
    {
        return Mathf.Clamp((t - MIN) / (MAX - MIN), 0f, 1f);
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
    public static void Write(this MessageWriter writer, Enum value)
    {
        var enumType = value.GetType();
        var underlyingType = enumType.GetEnumUnderlyingType();

        if (underlyingType == typeof(byte))
            writer.Write(Convert.ToByte(value, NumberFormatInfo.InvariantInfo));
        else if (underlyingType == typeof(sbyte))
            writer.Write(Convert.ToSByte(value, NumberFormatInfo.InvariantInfo));
        else if (underlyingType == typeof(short))
            writer.Write(Convert.ToInt16(value, NumberFormatInfo.InvariantInfo));
        else if (underlyingType == typeof(ushort))
            writer.Write(Convert.ToUInt16(value, NumberFormatInfo.InvariantInfo));
        else if (underlyingType == typeof(ulong))
            writer.Write(Convert.ToUInt64(value, NumberFormatInfo.InvariantInfo));
        else if (underlyingType == typeof(uint))
            writer.WritePacked(Convert.ToUInt32(value, NumberFormatInfo.InvariantInfo));
        else if (underlyingType == typeof(int))
            writer.WritePacked(Convert.ToInt32(value, NumberFormatInfo.InvariantInfo));
        else if (underlyingType == typeof(long))
            throw new NotSupportedException("long enum types are not supported at the moment.");
        else
            throw new ArgumentException("Unknown underlying type for " + enumType.Name);
    }

    /// <summary>
    /// Writes a generic Enum value to the <paramref name="writer"/>.
    /// </summary>
    /// <param name="writer">The <see cref="MessageWriter"/> to write to.</param>
    /// <param name="value">The <see cref="Enum"/> to write.</param>
    /// <typeparam name="T">Enum type to write.</typeparam>
    public static void Write<T>(this MessageWriter writer, T value) where T : struct, Enum => writer.Write((Enum) value);

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
    /// Reads and converts an enum value from a network message.
    /// </summary>
    /// <param name="reader">The <see cref="MessageReader"/> to read from.</param>
    /// <typeparam name="T">The <see cref="Enum"/> type to convert to.</typeparam>
    /// <returns>An <see cref="Enum"/> value from the <paramref name="reader"/>.</returns>
    public static T ReadEnum<T>(this MessageReader reader) where T : struct, Enum
    {
        var enumType = typeof(T);
        var underlyingType = enumType.GetEnumUnderlyingType();

        if (underlyingType == typeof(byte))
            return (T) (object) reader.ReadByte();

        if (underlyingType == typeof(sbyte))
            return (T) (object) reader.ReadSByte();

        if (underlyingType == typeof(short))
            return (T) (object) reader.ReadInt16();

        if (underlyingType == typeof(ushort))
            return (T) (object) reader.ReadUInt16();

        if (underlyingType == typeof(ulong))
            return (T) (object) reader.ReadUInt64();

        if (underlyingType == typeof(uint))
            return (T) (object) reader.ReadPackedUInt32();

        if (underlyingType == typeof(int))
            return (T) (object) reader.ReadPackedInt32();

        if (underlyingType == typeof(long))
            throw new NotSupportedException("long enum types are not supported at the moment.");

        throw new ArgumentException("Unknown underlying type for " + enumType.Name);
    }

    /// <summary>
    /// Reads an enum value from a network message.
    /// </summary>
    /// <param name="reader">The <see cref="MessageReader"/> to read from.</param>
    /// <param name="enumType">The type of the enum.</param>
    /// <returns>The resulting enum value from the <paramref name="reader"/>.</returns>
    public static object ReadEnum(this MessageReader reader, Type enumType)
    {
        var underlyingType = enumType.GetEnumUnderlyingType();

        if (underlyingType == typeof(byte))
            return Enum.Parse(enumType, $"{reader.ReadByte()}");

        if (underlyingType == typeof(sbyte))
            return Enum.Parse(enumType, $"{reader.ReadSByte()}");

        if (underlyingType == typeof(short))
            return Enum.Parse(enumType, $"{reader.ReadInt16()}");

        if (underlyingType == typeof(ushort))
            return Enum.Parse(enumType, $"{reader.ReadUInt16()}");

        if (underlyingType == typeof(ulong))
            return Enum.Parse(enumType, $"{reader.ReadUInt64()}");

        if (underlyingType == typeof(uint))
            return Enum.Parse(enumType, $"{reader.ReadPackedUInt32()}");

        if (underlyingType == typeof(int))
            return Enum.Parse(enumType, $"{reader.ReadPackedInt32()}");

        if (underlyingType == typeof(long))
            throw new NotSupportedException("long enum types are not supported at the moment.");

        throw new ArgumentException("Unknown underlying type for " + enumType.Name);
    }

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
