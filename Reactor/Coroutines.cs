using System;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using UnhollowerBaseLib.Attributes;
using UnhollowerRuntimeLib;
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

        /// <summary>
        /// Wraps a System Coroutine with the Coroutine Wrapper to return an Il2Cpp Coroutine
        /// </summary>
        private static Il2CppSystem.Collections.IEnumerator Wrap(this IEnumerator routine) => new CoroutineWrapper(routine);

        /// <summary>
        /// Starts a managed coroutine on a MonoBehaviour
        /// </summary>
        /// <param name="component">MonoBehaviour to start the coroutine on</param>
        /// <param name="coroutine">The coroutine to be started</param>
        /// <returns>The unity coroutine being started</returns>
        public static Coroutine StartCoroutine(this MonoBehaviour component, IEnumerator coroutine)
        {
            return component.StartCoroutine(coroutine.Wrap());
        }

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

    [RegisterInIl2Cpp(typeof(Il2CppSystem.Collections.IEnumerator))]
    public sealed class CoroutineWrapper : Il2CppSystem.Object
    {
#pragma warning disable 8618
        public CoroutineWrapper(IntPtr ptr) : base(ptr) { }
#pragma warning restore 8618

        /// <summary>
        /// Creates an Il2Cpp IEnumerator with a System IEnumerator.
        /// </summary>
        public CoroutineWrapper(IEnumerator enumerator) : base(ClassInjector.DerivedConstructorPointer<CoroutineWrapper>())
        {
            ClassInjector.DerivedConstructorBody(this);
            Enumerator = enumerator;
        }

        private IEnumerator Enumerator
        {
            [HideFromIl2Cpp]
            get;
            [HideFromIl2Cpp]
            set;
        }

        public Il2CppSystem.Object? Current
        {
            get
            {
                return Enumerator.Current switch
                {
                    IEnumerator current => new CoroutineWrapper(current),
                    Il2CppSystem.Collections.IEnumerator current => current.Cast<Il2CppSystem.Object>(),
                    Il2CppSystem.Object current => current,
                    null => null,
                    _ => throw new ArgumentException("Encountered unexpected yield value in coroutine, of type " + Enumerator.Current?.GetType().FullName, nameof(Enumerator.Current))
                };
            }
        }

        public bool MoveNext() => Enumerator.MoveNext();

        public void Reset() => Enumerator.Reset();

        public static implicit operator Il2CppSystem.Collections.IEnumerator(CoroutineWrapper wrapper) => wrapper.Cast<Il2CppSystem.Collections.IEnumerator>();
    }
}
