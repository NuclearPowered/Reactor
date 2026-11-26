using Hazel;

namespace Reactor.Networking.Serialization;

/// <summary>
/// Provides a read-write method pair so that data types can define how to read and write themselves.
/// </summary>
public interface INetSerializable
{
    /// <summary>
    /// Writes the instance to the provided writer.
    /// </summary>
    /// <param name="writer">The <see cref="MessageWriter"/> to write to.</param>
    void WriteTo(MessageWriter writer);

    /// <summary>
    /// Reads the instance's properties from the provided reader.
    /// </summary>
    /// <param name="reader">The <see cref="MessageReader"/> to read from.</param>
    void ReadFrom(MessageReader reader);
}
