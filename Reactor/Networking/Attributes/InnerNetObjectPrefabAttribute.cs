using System;
using InnerNet;
using UnityEngine;

namespace Reactor.Networking.Attributes;

/// <summary>
/// Attribute for Load prefab method for custom <see cref="InnerNetObject"/>.
/// </summary>
/// <remarks>Must be static and return either <see cref="GameObject"/> with a <see cref="InnerNetObject"/> component, or a <see cref="InnerNetObject"/> prefab.</remarks>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Field | AttributeTargets.Property)]
public sealed class InnerNetObjectPrefabAttribute : Attribute
{
}
