using System;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using BepInEx.Unity.IL2CPP.Utils;
using Reactor.Utilities.Attributes;
using UnityEngine;

namespace Reactor.Utilities;

/// <summary>
/// Provides utilities for starting coroutines.
/// </summary>
public static class Coroutines
{
    [RegisterInIl2Cpp]
    internal sealed class Component : MonoBehaviour
    {
        internal static Component? Instance { get; set; }

        public Component(IntPtr ptr) : base(ptr)
        {
        }

        private void Awake()
        {
            Instance = this;
        }

        [SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "OnDestroy is an unity event and can't be static")]
        private void OnDestroy()
        {
            Instance = null;
        }
    }

    private static readonly ConditionalWeakTable<IEnumerator, Coroutine> _ourCoroutineStore = new();

    /// <summary>
    /// Starts a coroutine.
    /// </summary>
    /// <param name="coroutine">The coroutine to start.</param>
    /// <returns>Specified <paramref name="coroutine"/>.</returns>
    [return: NotNullIfNotNull("coroutine")]
    public static IEnumerator? Start(IEnumerator? coroutine)
    {
        if (coroutine != null)
        {
            _ourCoroutineStore.AddOrUpdate(coroutine, Component.Instance!.StartCoroutine(coroutine));
        }

        return coroutine;
    }

    /// <summary>
    /// Stops a coroutine.
    /// </summary>
    /// <param name="coroutine">The coroutine to stop.</param>
    public static void Stop(IEnumerator? coroutine)
    {
        if (coroutine != null && _ourCoroutineStore.TryGetValue(coroutine, out var routine))
        {
            Component.Instance!.StopCoroutine(routine);
        }
    }
}
