using System;
using Hazel;

namespace Reactor.Networking.Serialization;

public abstract class UnsafeMessageConverter
{
    public abstract void UnsafeWrite(MessageWriter writer, object value);
    public abstract object UnsafeRead(MessageReader reader, Type objectType);

    public abstract bool CanConvert(Type objectType);
}

public abstract class MessageConverter<T> : UnsafeMessageConverter
{
    public abstract void Write(MessageWriter writer, T value);

    public abstract T Read(MessageReader reader, Type objectType);

    public override bool CanConvert(Type objectType)
    {
        return typeof(T).IsAssignableFrom(objectType);
    }

    public override void UnsafeWrite(MessageWriter writer, object value) => Write(writer, (T) value);

    public override object UnsafeRead(MessageReader reader, Type objectType) => Read(reader, objectType)!;
}
