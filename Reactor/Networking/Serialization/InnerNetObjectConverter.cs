using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using Hazel;
using InnerNet;
using Reactor.Networking.Attributes;

namespace Reactor.Networking.Serialization;

[MessageConverter]
public class InnerNetObjectConverter : MessageConverter<InnerNetObject>
{
    private static readonly MethodInfo _readNetObject = AccessTools.Method(typeof(MessageExtensions), nameof(MessageExtensions.ReadNetObject));
    private static readonly Dictionary<Type, Func<MessageReader, InnerNetObject>> _readNetObjectMap = new();

    public static InnerNetObject ReadNetObject(MessageReader reader, Type t)
    {
        if (_readNetObjectMap.TryGetValue(t, out var value))
        {
            return value(reader);
        }

        var @delegate = _readNetObject.MakeGenericMethod(t).CreateDelegate<Func<MessageReader, InnerNetObject>>();
        _readNetObjectMap[t] = @delegate;
        return @delegate(reader);
    }

    public override void Write(MessageWriter writer, InnerNetObject value)
    {
        MessageExtensions.WriteNetObject(writer, value);
    }

    public override InnerNetObject Read(MessageReader reader, Type objectType)
    {
        return ReadNetObject(reader, objectType);
    }
}
