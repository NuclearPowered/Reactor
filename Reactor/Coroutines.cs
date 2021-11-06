using System;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using BepInEx.IL2CPP.Utils;
using UnityEngine;

namespace Reactor
{
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
}
