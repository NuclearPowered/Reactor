using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Hazel;
using Reactor.Networking.Extensions;
using UnityEngine;

namespace Reactor.Networking.Serialization;

/// <summary>
/// Provides de/serializing of objects from/into <see cref="MessageReader"/>/<see cref="MessageWriter"/>.
/// </summary>
public static class MessageSerializer
{
    private static Dictionary<Type, UnsafeMessageConverter?> MessageConverterMap { get; } = new();
    private static List<UnsafeMessageConverter> MessageConverters { get; } = new();
    private static List<Type> GenericConverters { get; } = new();

    private enum SearchMode
    {
        Basic,
        Generic,
        Combined,
    }

    internal static void ClearMaps()
    {
        MessageConverterMap.Clear();
    }

    /// <summary>
    /// Registers a MessageConverter.
    /// </summary>
    /// <param name="type">The Type of the MessageConverter to be registered.</param>
    public static void Register(Type type)
    {
        if (type.IsGenericTypeDefinition)
        {
            var baseType = type.BaseType;
            var isUnsafeConverter = baseType == typeof(UnsafeMessageConverter);
            var isMessageConverterT = baseType != null && baseType.IsGenericType && baseType.GetGenericTypeDefinition() == typeof(MessageConverter<>);

            if (!isMessageConverterT && !isUnsafeConverter)
                throw new InvalidOperationException($"{type.Name} must directly inherit from either MessageConverter<T> or UnsafeMessageConverter.");

            GenericConverters.Add(type);
        }
        else
        {
            var messageConverter = (UnsafeMessageConverter) CreateObject(type)!;
            MessageConverters.Add(messageConverter);
        }
    }

    /// <summary>
    /// Finds a MessageConverter for the specified <paramref name="type"/> by checking generic converters first, then basic converters.
    /// </summary>
    /// <param name="type">The type of the object.</param>
    /// <returns>A MessageConverter that can convert the specified <see cref="Type"/>.</returns>
    public static UnsafeMessageConverter? FindConverter(Type type)
        => ResolveAndCache(type, SearchMode.Combined);

    /// <summary>
    /// Finds a MessageConverter for the specified <paramref name="type"/> using basic converters.
    /// </summary>
    /// <param name="type">The type of the object.</param>
    /// <returns>A MessageConverter that can convert the specified <see cref="Type"/>.</returns>
    public static UnsafeMessageConverter? FindBasicConverter(Type type)
        => ResolveAndCache(type, SearchMode.Basic);

    /// <summary>
    /// Finds and builds a MessageConverter for the specified <paramref name="type"/> using a registered generic converter.
    /// </summary>
    /// <param name="type">The type of the object.</param>
    /// <returns>A MessageConverter that can convert the specified <see cref="Type"/>.</returns>
    public static UnsafeMessageConverter? FindGenericConverter(Type type)
        => ResolveAndCache(type, SearchMode.Generic);

    private static UnsafeMessageConverter? ResolveAndCache(Type type, SearchMode mode)
    {
        if (MessageConverterMap.TryGetValue(type, out var value))
            return value;

        value = mode switch
        {
            SearchMode.Basic => FindBasicConverterInternal(type),
            SearchMode.Generic => FindGenericConverterInternal(type),
            SearchMode.Combined => FindBasicConverterInternal(type) ?? FindGenericConverterInternal(type),
            _ => null,
        };

        MessageConverterMap[type] = value;

        return value;
    }

    private static UnsafeMessageConverter? FindBasicConverterInternal(Type type)
    {
        return MessageConverters.SingleOrDefault(x => x.CanConvert(type));
    }

    private static UnsafeMessageConverter? FindGenericConverterInternal(Type type)
    {
        foreach (var converterTypeDef in GenericConverters)
        {
            var mappedArgs = MapGenericArguments(converterTypeDef, type);

            if (mappedArgs == null)
                continue;

            var converterParams = converterTypeDef.GetGenericArguments();
            var allSatisfy = true;

            for (var i = 0; i < mappedArgs.Length; i++)
            {
                if (SatisfiesConstraints(mappedArgs[i], converterParams[i]))
                    continue;

                allSatisfy = false;
                break;
            }

            if (!allSatisfy)
                continue;

            var genericConverterType = converterTypeDef.MakeGenericType(mappedArgs);
            var converterInstance = (UnsafeMessageConverter) CreateObject(genericConverterType)!;

            MessageConverters.Add(converterInstance);

            return converterInstance;
        }

        return null;
    }

    private static Type[]? MapGenericArguments(Type converterTypeDef, Type targetType)
    {
        var baseType = converterTypeDef.BaseType;

        if (baseType == null)
            return null;

        if (baseType == typeof(UnsafeMessageConverter))
        {
            var converterParams = converterTypeDef.GetGenericArguments();

            if (converterParams.Length == 1)
                return new[] { targetType };

            if (targetType.IsGenericType && converterParams.Length == targetType.GetGenericArguments().Length)
                return targetType.GetGenericArguments();

            return null;
        }

        if (!baseType.IsGenericType)
            return null;

        var templateType = baseType.GetGenericArguments()[0];

        if (templateType.IsGenericParameter)
            return new[] { targetType };

        if (!templateType.IsGenericType || !targetType.IsGenericType)
        {
            return null;
        }

        var templateArgs = templateType.GetGenericArguments();
        var targetArgs = targetType.GetGenericArguments();

        if (templateArgs.Length != targetArgs.Length)
            return null;

        var mappedArgs = new Type[converterTypeDef.GetGenericArguments().Length];

        for (var i = 0; i < templateArgs.Length; i++)
        {
            var templateArg = templateArgs[i];
            var targetArg = targetArgs[i];

            if (templateArg.IsGenericParameter)
                mappedArgs[templateArg.GenericParameterPosition] = targetArg;
            else if (templateArg != targetArg)
                return null;
        }

        return mappedArgs.Any(t => t == null) ? null : mappedArgs;
    }

    private static bool SatisfiesConstraints(Type targetType, Type genericParam)
    {
        var attributes = genericParam.GenericParameterAttributes;
        var constraints = genericParam.GetGenericParameterConstraints();

        // struct constraint
        if (attributes.HasFlag(GenericParameterAttributes.NotNullableValueTypeConstraint) && !targetType.IsValueType)
            return false;

        // class constraint
        if (attributes.HasFlag(GenericParameterAttributes.ReferenceTypeConstraint) && targetType.IsValueType)
            return false;

        // new() constraint
        if (attributes.HasFlag(GenericParameterAttributes.DefaultConstructorConstraint) && !targetType.IsValueType && targetType.GetConstructor(Type.EmptyTypes) == null)
            return false;

        // notnull constraint
        if (genericParam.CustomAttributes.Any(a => a.AttributeType.Name == "NullableAttribute"))
        {
            if (Nullable.GetUnderlyingType(targetType) != null)
                return false;

            // Nullable reference types cannot be distinguished, so we let them through here
        }

        // Explicit type constraints
        foreach (var constraint in constraints)
        {
            var check = constraint == typeof(Enum)
                ? !targetType.IsEnum
                : !constraint.IsAssignableFrom(targetType);

            if (check)
                return false;
        }

        return true;
    }

    /// <summary>
    /// Serializes <paramref name="args"/> to the <paramref name="writer"/>.
    /// </summary>
    /// <param name="writer">The <see cref="MessageWriter"/> to write to.</param>
    /// <param name="args">The args to be written.</param>
    public static void Serialize(this MessageWriter writer, params object[] args)
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
                writer.WritePacked(i);
                break;
            case long i:
                writer.WritePacked(i);
                break;
            case Vector2 i:
                writer.Write(i);
                break;
            case string i:
                writer.Write(i);
                break;
            case Color i:
                writer.Write(i);
                break;
            case Color32 i:
                writer.Write(i);
                break;
            case Enum i:
                writer.Write(i);
                break;
            case INetSerializable i:
                i.WriteTo(writer);
                break;
            default:
                var type = @object.GetType();
                var converter = FindConverter(type);

                if (converter != null)
                    converter.UnsafeWrite(writer, @object);
                else
                    throw new NotSupportedException("Couldn't serialize " + type.Name);

                break;
        }
    }

    /// <summary>
    /// Deserializes a generic <typeparamref name="T"/> value from the <paramref name="reader"/>.
    /// </summary>
    /// <param name="reader">The <see cref="MessageReader"/> to read from.</param>
    /// <typeparam name="T">The type to be read.</typeparam>
    /// <returns>A generic <typeparamref name="T"/> value from the <paramref name="reader"/>.</returns>
    public static T Deserialize<T>(this MessageReader reader) => (T) reader.Deserialize(typeof(T));

    private static readonly Dictionary<Type, Func<MessageReader, object>> _basicReaders = new()
    {
        [typeof(int)] = reader => reader.ReadPackedInt32(),
        [typeof(uint)] = reader => reader.ReadPackedUInt32(),
        [typeof(byte)] = reader => reader.ReadByte(),
        [typeof(float)] = reader => reader.ReadSingle(),
        [typeof(sbyte)] = reader => reader.ReadSByte(),
        [typeof(ushort)] = reader => reader.ReadUInt16(),
        [typeof(short)] = reader => reader.ReadInt16(),
        [typeof(bool)] = reader => reader.ReadBoolean(),
        [typeof(Vector2)] = reader => reader.ReadVector2(),
        [typeof(string)] = reader => reader.ReadString(),
        [typeof(ulong)] = reader => reader.ReadPackedUInt64(),
        [typeof(long)] = reader => reader.ReadPackedInt64(),
        [typeof(Color)] = reader => reader.ReadColor(),
        [typeof(Color32)] = reader => reader.ReadColor32(),
    };

    /// <summary>
    /// Deserializes an <see cref="object"/> of <paramref name="objectType"/> from the <paramref name="reader"/>.
    /// </summary>
    /// <param name="reader">The <see cref="MessageReader"/> to read from.</param>
    /// <param name="objectType">The <see cref="Type"/> of the object.</param>
    /// <returns>An <see cref="object"/> from the <paramref name="reader"/>.</returns>
    public static object Deserialize(this MessageReader reader, Type objectType)
    {
        if (_basicReaders.TryGetValue(objectType, out var readDel))
        {
            return readDel(reader);
        }

        if (objectType.IsEnum)
        {
            return reader.ReadEnum(objectType);
        }

        if (objectType.IsAssignableTo(typeof(INetSerializable)))
        {
            var instance = (INetSerializable) CreateObject(objectType);
            instance.ReadFrom(reader);
            return instance;
        }

        var converter = FindConverter(objectType);

        if (converter != null)
        {
            return converter.UnsafeRead(reader, objectType);
        }

        throw new NotSupportedException("Couldn't deserialize " + objectType);
    }

    private static readonly ConcurrentDictionary<Type, ConstructorInfo?> _constructors = new();

    private static object CreateObject(Type objectType)
    {
        var construct = _constructors.GetOrAdd(objectType, GetConstruct);

        if (construct != null)
            return construct.Invoke(null);

        Warning($"{objectType.Name} does not have a parameterless constructor. It is highly recommended that one is added so that field initialisers are not skipped.");
        return RuntimeHelpers.GetUninitializedObject(objectType);
    }

    private static ConstructorInfo? GetConstruct(Type type) => type.GetConstructor(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance, Type.EmptyTypes);
}
