using System;
using System.Collections.Generic;
using System.Reflection;
using Hazel;
using InnerNet;
using Reactor.Extensions;
using UnityEngine;

namespace Reactor.Networking
{
    public static class RpcSerializer
    {
        public static void SendMassage(MessageWriter writer, params object[] list)
        {
            Logger<ReactorPlugin>.Debug($"Serializing {list.Length} object");

            foreach (var o in list)
            {
                if (o.GetType().IsValueType  && !o.GetType().IsPrimitive)
                {
                    var structFeilds = o.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance);
                    foreach (var feild in structFeilds)
                    {
                        writer.Write(feild.GetValue(o));
                    }
                }
                else
                {
                    writer.Write(o);
                }
            }
        }

        public static void Write(this MessageWriter writer, object o)
        {
            switch (o)
            {
                case int i:
                    writer.WritePacked(i);
                    Logger<ReactorPlugin>.Debug($"Serialized int");
                    break;
                case uint i:
                    writer.WritePacked(i);
                    Logger<ReactorPlugin>.Debug($"Serialized uint");
                    break;
                case byte i:
                    writer.Write(i);
                    Logger<ReactorPlugin>.Debug($"Serialized byte");
                    break;
                case float i:
                    writer.Write(i);
                    Logger<ReactorPlugin>.Debug($"Serialized float");
                    break;
                case sbyte i:
                    writer.Write(i);
                    Logger<ReactorPlugin>.Debug($"Serialized sbyte");
                    break;
                case ushort i:
                    writer.Write(i);
                    Logger<ReactorPlugin>.Debug($"Serialized ushort");
                    break;
                case bool i:
                    writer.Write(i);
                    Logger<ReactorPlugin>.Debug($"Serialized bool");
                    break;
                case Vector2 i:
                    writer.Write(i);
                    Logger<ReactorPlugin>.Debug($"Serialized Vector2");
                    break;
                case string i:
                    writer.Write(i);
                    Logger<ReactorPlugin>.Debug($"Serialized string");
                    break;
                default:
                    if (o.GetType().IsSubclassOf(typeof(InnerNetObject)))
                    {
                        Logger<ReactorPlugin>.Debug($"Serialized {nameof(InnerNetObject)}");
                        writer.WritePacked(((InnerNetObject)o).NetId);
                    }
                    else
                    {
                        Logger<ReactorPlugin>.Warning($"Tried serializing unsupported type {nameof(o)}");
                    }

                    break;
            }
        }


        public static object[] Deserialize(MessageReader reader, Type[] list)
        {
            Logger<ReactorPlugin>.Debug($"Deserializing {list.Length} object");

            List<object> args = new();

            foreach (var t in list)
            {
                if (t.IsValueType && !t.IsPrimitive)
                {
                    List<object> structArgs = new();
                    var feilds = t.GetFields(BindingFlags.Public | BindingFlags.Instance);
                    foreach (var feild in feilds)
                    {
                        structArgs.Add(reader.Read(feild.GetType()));
                    }

                    args.Add(Activator.CreateInstance(t, structArgs));
                }
                else
                {
                    args.Add(reader.Read(t));
                }
            }

            return args.ToArray();
        }


        public static object Read(this MessageReader reader, Type t)
        {
            if (t == typeof(int) )
            {
                return reader.ReadPackedInt32();
            }

            if (t == typeof(uint))
            {
                return (reader.ReadPackedUInt32());
            }

            if (t == typeof(byte))
            {
                return (reader.ReadByte());
            }

            if (t == typeof(float))
            {
                return (reader.ReadSingle());
            }

            if (t == typeof(sbyte))
            {
                return (reader.ReadSByte());
            }

            if (t == typeof(ushort))
            {
                return (reader.ReadUInt16());
            }

            if (t == typeof(bool))
            {
                return (reader.ReadBoolean());
            }

            if (t == typeof(Vector2))
            {
                return (reader.ReadVector2());
            }

            if (t == typeof(string))
            {
                return (reader.ReadString());
            }

            if (t.IsSubclassOf(typeof(InnerNetObject)))
            {
                return typeof(AmongUsClient).GetMethod(nameof(AmongUsClient.FindObjectByNetId)).MakeGenericMethod(t)
                    .Invoke(AmongUsClient.Instance, new object[]{reader.ReadPackedUInt32()});
            }

            Logger<ReactorPlugin>.Warning($"Tried deserializing unsupported type {t.BaseType.Name}");
            return null;

        }
    }
}
