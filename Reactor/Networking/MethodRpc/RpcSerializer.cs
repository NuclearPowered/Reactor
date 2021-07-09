using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Hazel;
using InnerNet;
using Reactor.Extensions;
using UnityEngine;

namespace Reactor.Networking.MethodRpc
{
    public static class RpcSerializer
    {
        public static void Serialize(MessageWriter writer,CustomMethodRpc methodRpc, params object[] list)
        {
            foreach (var o in list)
            {
                if (o.GetType().IsValueType  && !o.GetType().IsPrimitive)
                {
                    var structFeilds = methodRpc.StructBook[o.GetType()];
                    foreach (var feild in structFeilds)
                    {
                        writer.Write(feild.GetValue(o));
                    }
                }
                else
                {
                    writer.Write(o);
                }
                Logger<ReactorPlugin>.Debug($"Serialized {o.GetType()}");
            }
        }

        public static void Write(this MessageWriter writer, object o)
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
                    if (o.GetType().IsSubclassOf(typeof(InnerNetObject)))
                    {
                        writer.WriteNetObject((InnerNetObject) o);
                    }
                    else if (ReaderWriterManager.Instance.All.ContainsKey(o.GetType()))
                    {
                        ReaderWriterManager.Instance.All[o.GetType()].UnsafeWrite(writer, o);
                    }
                    else
                    {
                        Logger<ReactorPlugin>.Warning($"Tried serializing unsupported type {o.GetType()}");
                    }
                    break;
            }
        }


        public static object[] Deserialize(MessageReader reader,CustomMethodRpc methodRpc)
        {
            Logger<ReactorPlugin>.Debug($"Deserializing {methodRpc.Parameters.Length} object");

            object[] args = new object[methodRpc.Parameters.Length];

            for (int i = 0; i < args.Length; i++)
            {
                Type t = methodRpc.Parameters[i];
                if (t.IsValueType && !t.IsPrimitive)
                {
                    var fields = methodRpc.StructBook[t];
                    
                    object[] structArgs = new object[fields.Length];
                    for (int n = 0; n < structArgs.Length; n++)
                    {
                        structArgs[n] = reader.Read(fields[n].FieldType);
                        Logger<ReactorPlugin>.Debug($"Deserialized {fields[n].FieldType} from {t}");
                    }
                    t.GetType().GetConstructors().ToList().ForEach(x=>Logger<ReactorPlugin>.Info($"Gw {x.GetParameters().Length}"));

                    args[i] =  Activator.CreateInstance(t,structArgs);
                }
                else
                {
                    var value = reader.Read(t);
                    Logger<ReactorPlugin>.Debug($"Read {value} of {t.Name} for {methodRpc.Method.Name}");
                    args[i] = value;
                }
            }

            return args;
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
                return reader.ReadNetObject(t);
            }

            if (ReaderWriterManager.Instance.All.ContainsKey(t.BaseType!))
            {
                return ReaderWriterManager.Instance.All[t.BaseType].UnsafeRead(reader);
            }

            Logger<ReactorPlugin>.Warning($"Tried deserializing unsupported type {t.BaseType.Name}");
            return null;

        }

        #region InnerNetObject

        private static readonly MethodInfo _findObjectByNetId =
            typeof(RpcSerializer).GetMethod(nameof(FindObjectByNetId), BindingFlags.Static | BindingFlags.NonPublic);

        private static Dictionary<Type, Func<object[], object>> _findObjectByNetIdInvokers = new();
        static Func<object[], object> GetFindObjectByNetIdInvoker(Type type)
        {
            if (_findObjectByNetIdInvokers.ContainsKey(type)) return _findObjectByNetIdInvokers[type];

            var invoker = RpcPrefixHandle.GenerateCaller(_findObjectByNetId.MakeGenericMethod(type));
            _findObjectByNetIdInvokers.Add(type, invoker);
            return invoker;
        }

        private static InnerNetObject FindObjectByNetId<T>(uint id) where T : InnerNetObject
        {
            return AmongUsClient.Instance.FindObjectByNetId<T>(id);
        }

        public static void WriteNetObject(this MessageWriter writer, InnerNetObject o) => writer.WritePacked(o.NetId);

        public static InnerNetObject ReadNetObject(this MessageReader reader, Type t) =>
            (InnerNetObject) GetFindObjectByNetIdInvoker(t)(new object[] {reader.ReadPackedUInt32()});

        #endregion

    }
}
