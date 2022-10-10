using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using Hazel;
using InnerNet;
using Reactor.Networking.Attributes;

namespace Reactor.Networking.Serialization;

/// <summary>
/// Provides a MessageConverter for <see cref="InnerNetObject"/>.
/// </summary>
[MessageConverter]
public class InnerNetObjectConverter : MessageConverter<InnerNetObject?>
{
    private static readonly MethodInfo _readNetObject = AccessTools.Method(typeof(MessageExtensions), nameof(MessageExtensions.ReadNetObject));
    private static readonly Dictionary<Type, Func<MessageReader, InnerNetObject?>> _readNetObjectMap = new();

    /// <summary>
    /// <see cref="MessageExtensions.ReadNetObject{T}"/> but without generics.
    /// </summary>
    /// <param name="reader">The <see cref="MessageReader"/> to read from.</param>
    /// <param name="innerNetObjectType">The type of the <see cref="InnerNetObject"/>.</param>
    /// <returns>The <see cref="InnerNetObject"/> read from the <paramref name="reader"/>.</returns>
    public static InnerNetObject? ReadNetObject(MessageReader reader, Type innerNetObjectType)
    {
        if (_readNetObjectMap.TryGetValue(innerNetObjectType, out var value))
        {
            return value(reader);
        }

        var @delegate = _readNetObject.MakeGenericMethod(innerNetObjectType).CreateDelegate<Func<MessageReader, InnerNetObject>>();
        _readNetObjectMap[innerNetObjectType] = @delegate;
        return @delegate(reader);
    }

    /// <inheritdoc />
    public override void Write(MessageWriter writer, InnerNetObject? value)
    {
        writer.WriteNetObject(value);
    }

    /// <inheritdoc />
    public override InnerNetObject? Read(MessageReader reader, Type objectType)
    {
        return ReadNetObject(reader, objectType);
    }
}
