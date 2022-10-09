using System;
using Hazel;

namespace Reactor.Networking.Serialization;

/// <summary>
/// Base type for MessageConverter's.
/// </summary>
public abstract class UnsafeMessageConverter
{
    /// <summary>
    /// Writes the <paramref name="value"/> to the <paramref name="writer"/>.
    /// </summary>
    /// <param name="writer">The <see cref="MessageWriter"/> to write to.</param>
    /// <param name="value">The value to write.</param>
    public abstract void UnsafeWrite(MessageWriter writer, object value);

    /// <summary>
    /// Reads an object of <paramref name="objectType"/> from the <paramref name="reader"/>.
    /// </summary>
    /// <param name="reader">The <see cref="MessageReader"/> to read from.</param>
    /// <param name="objectType">The type of the object being read.</param>
    /// <returns>The object read from the <paramref name="reader"/>.</returns>
    public abstract object UnsafeRead(MessageReader reader, Type objectType);

    /// <summary>
    /// Checks whether this MessageConverter can convert the specified type.
    /// </summary>
    /// <param name="objectType">The type to check.</param>
    /// <returns>A value indicating whether this MessageConverter can convert the specified type.</returns>
    public abstract bool CanConvert(Type objectType);
}

/// <summary>
/// Base type for MessageConverter's but typed with generics.
/// </summary>
/// <typeparam name="T">The type of the value being converted.</typeparam>
public abstract class MessageConverter<T> : UnsafeMessageConverter
{
    /// <inheritdoc cref="UnsafeMessageConverter.UnsafeWrite"/>
    public abstract void Write(MessageWriter writer, T value);

    /// <summary>
    /// Reads a <typeparamref name="T"/> from the <paramref name="reader"/>.
    /// </summary>
    /// <param name="reader">The <see cref="MessageReader"/> to read from.</param>
    /// <param name="objectType">The type of the object being read.</param>
    /// <returns>The object read from the <paramref name="reader"/>.</returns>
    public abstract T Read(MessageReader reader, Type objectType);

    /// <inheritdoc />
    public override bool CanConvert(Type objectType)
    {
        return typeof(T).IsAssignableFrom(objectType);
    }

    /// <inheritdoc />
    public override void UnsafeWrite(MessageWriter writer, object value) => Write(writer, (T) value);

    /// <inheritdoc />
    public override object UnsafeRead(MessageReader reader, Type objectType) => Read(reader, objectType)!;
}
