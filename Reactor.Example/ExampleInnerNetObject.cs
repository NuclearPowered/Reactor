using System;
using System.Threading.Tasks;
using Hazel;
using InnerNet;
using Reactor.Networking.Attributes;

namespace Reactor.Example;

// The `IgnoreInnerNetObject` attribute is used to prevent this custom InnerNetObject
// from being automatically registered by Reactor.
[IgnoreInnerNetObject]
public class ExampleInnerNetObject : InnerNetObject
{
    // The `InnerNetObjectPrefab` attribute is used to define how the prefab for this
    // custom InnerNetObject is retrieved. The prefab is a template object that is used
    // when spawning instances of this object in the game.
    // There are three examples provided for retrieving the prefab:

    // Example 1: Directly assign a prefab
    // This is the simplest method, where the prefab is assigned directly to a static field or property.
    [InnerNetObjectPrefab]
    public static InnerNetObject? PrefabField;
    // or
    [InnerNetObjectPrefab]
    public static InnerNetObject? PrefabProperty { get; set; }

    // Example 2: Retrieve the prefab via a static method
    // This method allows for more complex logic to retrieve the prefab.
    // The method must be static and return an InnerNetObject (or a GameObject with an InnerNetObject component).
    [InnerNetObjectPrefab]
    public static InnerNetObject GetPrefab()
    {
        throw new NotImplementedException($"GetPrefab prefab retrieval not implemented!");
    }

    // Example 3: Retrieve the prefab asynchronously
    // This method is similar to Example 2 but allows for asynchronous operations,
    // such as loading assets from disk.
    [InnerNetObjectPrefab]
    public static async Task<InnerNetObject> GetPrefabAsync()
    {
        throw new NotImplementedException($"GetPrefab prefab retrieval not implemented!");
    }

    // The `HandleRpc` method is required abstract to handle Remote Procedure Calls (RPCs) for this object.
    // RPCs are used to communicate between clients and the server.
    // The `callId` parameter identifies the type of RPC, and the `reader` parameter provides the data.
    public override void HandleRpc(byte callId, MessageReader reader)
    {
        // Implement logic to handle specific RPCs based on the `callId`.
        // For example, you might switch on `callId` to handle different types of RPCs.
    }

    // The `Serialize` method is required abstract to serialize the state of this object into a `MessageWriter`.
    // This is used to synchronize the object's state across the network.
    // The `initialState` parameter indicates whether this is the first time the object is being serialized.
    public override bool Serialize(MessageWriter writer, bool initialState)
    {
        // Implement logic to write the object's state to the `writer`.
        // Return `true` if the state was serialized successfully, otherwise `false`.
        return false;
    }

    // The `Deserialize` method is required abstract to deserialize the state of this object from a `MessageReader`.
    // This is used to update the object's state based on data received from the network.
    // The `initialState` parameter indicates whether this is the first time the object is being deserialized.
    public override void Deserialize(MessageReader reader, bool initialState)
    {
        // Implement logic to read the object's state from the `reader`.
    }
}
