using System;
using System.Collections.Generic;
using System.Linq;
using Hazel;
using Reactor.Networking.Extensions;
using UnityEngine;

namespace Reactor.Networking.Serialization;

/// <summary>
/// Provides de/serializing of objects from/into <see cref="MessageReader"/>/<see cref="MessageWriter"/>.
/// </summary>
public static class MessageSerializer
{
    private static List<UnsafeMessageConverter> MessageConverters { get; } = new();

    private static Dictionary<Type, UnsafeMessageConverter?> MessageConverterMap { get; } = new();

    /// <summary>
    /// Registers a MessageConverter.
    /// </summary>
    /// <param name="messageConverter">The MessageConverter to be registered.</param>
    public static void Register(UnsafeMessageConverter messageConverter)
    {
        MessageConverters.Add(messageConverter);
        MessageConverterMap.Clear();
    }

    /// <summary>
    /// Finds a MessageConverter for the specified <paramref name="type"/>.
    /// </summary>
    /// <param name="type">The type of an object.</param>
    /// <returns>A MessageConverter that can convert the specified <see cref="Type"/>.</returns>
    public static UnsafeMessageConverter? FindConverter(Type type)
    {
        if (MessageConverterMap.TryGetValue(type, out var value))
        {
            return value;
        }

        var converter = MessageConverters.SingleOrDefault(x => x.CanConvert(type));
        MessageConverterMap.Add(type, converter);

        return converter;
    }

    /// <summary>
    /// Serializes <paramref name="args"/> to the <paramref name="writer"/>.
    /// </summary>
    /// <param name="writer">The <see cref="MessageWriter"/> to write to.</param>
    /// <param name="args">The args to be written.</param>
    public static void Serialize(MessageWriter writer, object[] args)
    {
        foreach (var arg in args)
        {
            writer.Serialize(arg);
        }
    }

    /// <summary>
    /// Serializes an <paramref name="object"/> to the <paramref name="writer"/>.
    /// </summary>
    /// <param name="writer">The <see cref="MessageWriter"/> to write to.</param>
    /// <param name="object">The <see cref="object"/> to be written.</param>
    public static void Serialize(this MessageWriter writer, object @object)
    {
        switch (@object)
        {
            case int i:
                writer.WritePacked(i);
                break;
            case uint i:
                writer.WritePacked(i);
                break;
            case byte i:
                writer.Write(i);
                break;
            case float i:
                writer.Write(i);
                break;
            case sbyte i:
                writer.Write(i);
                break;
            case ushort i:
                writer.Write(i);
                break;
            case bool i:
                writer.Write(i);
                break;
            case ulong i:
                writer.Write(i);
                break;
            case long i:
                ExtraMessageExtensions.Write(writer, i); // For some reason this insists on referring the to float write method, so this is me taking precautions
                break;
            case Vector2 i:
                writer.Write(i);
                break;
            case string i:
                writer.Write(i);
                break;
            case Enum i:
                writer.Write(i);
                break;
            default:
                var converter = FindConverter(@object.GetType());
                if (converter != null)
                {
                    converter.UnsafeWrite(writer, @object);
                    break;
                }

                throw new NotSupportedException("Couldn't serialize " + @object.GetType());
        }
    }

    /// <summary>
    /// Deserializes an <see cref="object"/> of <paramref name="objectType"/> from the <paramref name="reader"/>.
    /// </summary>
    /// <param name="reader">The <see cref="MessageReader"/> to read from.</param>
    /// <param name="objectType">The <see cref="Type"/> of the object.</param>
    /// <returns>An <see cref="object"/> from the <paramref name="reader"/>.</returns>
    public static object Deserialize(this MessageReader reader, Type objectType)
    {
        if (objectType == typeof(int))
        {
            return reader.ReadPackedInt32();
        }

        if (objectType == typeof(uint))
        {
            return reader.ReadPackedUInt32();
        }

        if (objectType == typeof(byte))
        {
            return reader.ReadByte();
        }

        if (objectType == typeof(float))
        {
            return reader.ReadSingle();
        }

        if (objectType == typeof(sbyte))
        {
            return reader.ReadSByte();
        }

        if (objectType == typeof(ushort))
        {
            return reader.ReadUInt16();
        }

        if (objectType == typeof(bool))
        {
            return reader.ReadBoolean();
        }

        if (objectType == typeof(Vector2))
        {
            return reader.ReadVector2();
        }

        if (objectType == typeof(string))
        {
            return reader.ReadString();
        }

        if (objectType == typeof(ulong))
        {
            return reader.ReadUInt64();
        }

        if (objectType == typeof(long))
        {
            return reader.ReadInt64();
        }

        if (objectType.IsEnum)
        {
            return reader.ReadEnum(objectType);
        }

        var converter = FindConverter(objectType);
        if (converter != null)
        {
            return converter.UnsafeRead(reader, objectType);
        }

        throw new NotSupportedException("Couldn't deserialize " + objectType);
    }
}
