using System;
using System.Collections.Generic;
using System.Linq;
using Hazel;
using Reactor.Extensions;
using UnityEngine;

namespace Reactor.Networking.Serialization
{
    public static class MessageSerializer
    {
        private static List<UnsafeMessageConverter> MessageConverters { get; } = new List<UnsafeMessageConverter>();

        private static Dictionary<Type, UnsafeMessageConverter> MessageConverterMap { get; } = new Dictionary<Type, UnsafeMessageConverter>();

        public static void Register(UnsafeMessageConverter messageConverter)
        {
            MessageConverters.Add(messageConverter);
            MessageConverterMap.Clear();
        }

        public static UnsafeMessageConverter FindConverter(Type type)
        {
            if (MessageConverterMap.TryGetValue(type, out var value))
            {
                return value;
            }

            var converter = MessageConverters.SingleOrDefault(x => x.CanConvert(type));
            MessageConverterMap.Add(type, converter);

            return converter;
        }

        public static void Serialize(MessageWriter writer, object[] args)
        {
            foreach (var arg in args)
            {
                writer.Serialize(arg);
            }
        }

        public static void Serialize(this MessageWriter writer, object o)
        {
            switch (o)
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
                case Vector2 i:
                    writer.Write(i);
                    break;
                case string i:
                    writer.Write(i);
                    break;
                default:
                    var converter = FindConverter(o.GetType());
                    if (converter != null)
                    {
                        converter.UnsafeWrite(writer, o);
                        break;
                    }

                    throw new NotSupportedException("Couldn't serialize " + o.GetType());
            }
        }

        public static object[] Deserialize(MessageReader reader, MethodRpc.MethodRpc methodRpc)
        {
            var parameters = methodRpc.Method.GetParameters();
            var args = new object[parameters.Length - 1];

            for (var i = 1; i < parameters.Length; i++)
            {
                var parameter = parameters[i];
                args[i - 1] = reader.Deserialize(parameter.ParameterType);
            }

            return args;
        }

        public static object Deserialize(this MessageReader reader, Type t)
        {
            if (t == typeof(int))
            {
                return reader.ReadPackedInt32();
            }

            if (t == typeof(uint))
            {
                return reader.ReadPackedUInt32();
            }

            if (t == typeof(byte))
            {
                return reader.ReadByte();
            }

            if (t == typeof(float))
            {
                return reader.ReadSingle();
            }

            if (t == typeof(sbyte))
            {
                return reader.ReadSByte();
            }

            if (t == typeof(ushort))
            {
                return reader.ReadUInt16();
            }

            if (t == typeof(bool))
            {
                return reader.ReadBoolean();
            }

            if (t == typeof(Vector2))
            {
                return reader.ReadVector2();
            }

            if (t == typeof(string))
            {
                return reader.ReadString();
            }

            var converter = FindConverter(t);
            if (converter != null)
            {
                return converter.UnsafeRead(reader, t);
            }

            throw new NotSupportedException("Couldn't deserialize " + t);
        }
    }
}
