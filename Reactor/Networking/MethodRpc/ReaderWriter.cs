using System;
using System.Collections.Generic;
using Hazel;

namespace Reactor.Networking.MethodRpc
{

    internal class ReaderWriterManager
    {
        public static ReaderWriterManager Instance { get; } = new();
        
        private readonly Dictionary<Type, UnsafeReaderWriter> _readerWriters = new();

        public IReadOnlyDictionary<Type, UnsafeReaderWriter> All => _readerWriters;

        public void Register(UnsafeReaderWriter readerWriter)
        {
            Logger<ReactorPlugin>.Instance.LogInfo($"Registered ReaderWriter for {readerWriter.Type}");
            if(_readerWriters.ContainsKey(readerWriter.Type)) return;

            _readerWriters.Add(readerWriter.Type, readerWriter);
        }
    }
    
    public abstract class UnsafeReaderWriter
    {
        public abstract Type Type { get; }
        public abstract void UnsafeWrite(MessageWriter writer, object dateTime);

        public abstract object UnsafeRead(MessageReader reader);
    }
    
    public abstract class ReaderWriter<T> : UnsafeReaderWriter
    {
        public abstract void Write(MessageWriter writer, T o);

        public abstract T Read(MessageReader reader);

        public override Type Type { get; } = typeof(T);

        public override void UnsafeWrite(MessageWriter writer, object o) => Write(writer, (T) o);

        public override object UnsafeRead(MessageReader reader) => Read(reader);
    }
}
