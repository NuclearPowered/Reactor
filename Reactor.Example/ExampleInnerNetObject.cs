using System;
using Hazel;
using InnerNet;
using Reactor.Networking.Attributes;
using UnityEngine;

namespace Reactor.Example;

[InnerNetObject]
public class ExampleInnerNetObject : InnerNetObject
{
    // Method one of retrieving a prefab.
    // Be sure to only use one method!
    [InnerNetObjectPrefab]
    public static InnerNetObject GetPrefabByInnerNetObject()
    {
        throw new NotImplementedException($"GetPrefab prefab retrieval not implemented!");
    }

    // Method two of retrieving a prefab.
    // Be sure to only use one method!
    [InnerNetObjectPrefab]
    public static GameObject GetPrefabByGameObject()
    {
        throw new NotImplementedException($"GetPrefab prefab retrieval not implemented!");
    }

    public override void HandleRpc(byte callId, MessageReader reader)
    {
    }

    public override bool Serialize(MessageWriter writer, bool initialState)
    {
        return false;
    }

    public override void Deserialize(MessageReader reader, bool initialState)
    {
    }
}
