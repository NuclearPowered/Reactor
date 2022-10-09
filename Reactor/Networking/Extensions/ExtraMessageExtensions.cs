using System;
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
