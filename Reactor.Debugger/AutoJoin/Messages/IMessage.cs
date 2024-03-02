#pragma warning disable CA2252 // TODO Remove after BepInEx updates the .NET version
using System.IO;

namespace Reactor.Debugger.AutoJoin.Messages;

internal interface IMessage<TSelf> where TSelf : IMessage<TSelf>
{
    static abstract MessageType Type { get; }

    void Serialize(BinaryWriter writer);

    static abstract TSelf Deserialize(BinaryReader reader);

    string ToString();
}
