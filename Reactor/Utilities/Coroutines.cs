using System;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using BepInEx.Unity.IL2CPP.Utils;
using Reactor.Utilities.Attributes;
using UnityEngine;

namespace Reactor.Utilities;

public static class Coroutines
{
    [RegisterInIl2Cpp]
    internal class Component : MonoBehaviour
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

    [return: NotNullIfNotNull("routine")]
    public static IEnumerator? Start(IEnumerator? routine)
    {
        if (routine != null)
        {
            _ourCoroutineStore.AddOrUpdate(routine, Component.Instance!.StartCoroutine(routine));
        }

        return routine;
    }

    public static void Stop(IEnumerator? enumerator)
    {
        if (enumerator != null && _ourCoroutineStore.TryGetValue(enumerator, out var routine))
        {
            Component.Instance!.StopCoroutine(routine);
        }
    }
}
