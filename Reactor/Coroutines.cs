using System;
using System.Collections;
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
            internal static Component Instance { get; set; }

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

        private static ConditionalWeakTable<IEnumerator, Coroutine> _ourCoroutineStore = new();
        
        /// <summary>
        /// Wraps a System Coroutine with the Coroutine Wrapper to return an Il2Cpp Coroutine
        /// </summary>
        private static Il2CppSystem.Collections.IEnumerator GetRoutine(this IEnumerator routine) => new CoroutineWrapper(routine).TryCast<Il2CppSystem.Collections.IEnumerator>();
        
        /// <summary>
        /// Starts a managed coroutine on a MonoBehaviour
        /// </summary>
        /// <param name="component">MonoBehaviour to start the coroutine on</param>
        /// <param name="coroutine">The coroutine to be started</param>
        /// <returns>The unity coroutine being started</returns>
        public static Coroutine StartCoroutine(this MonoBehaviour component, IEnumerator coroutine)
        {
            return component.StartCoroutine(coroutine.GetRoutine());
        }
        
        public static IEnumerator Start(IEnumerator routine)
        {
            if (routine != null) _ourCoroutineStore.AddOrUpdate(routine, Component.Instance.StartCoroutine(routine));
            return routine;
        }

        public static void Stop(IEnumerator enumerator)
        {
            if (enumerator != null && _ourCoroutineStore.TryGetValue(enumerator, out var routine))
            {
                Component.Instance.StopCoroutine(routine);
            }
        }
    }

    [RegisterInIl2Cpp(typeof(Il2CppSystem.Collections.IEnumerator))]
    public sealed class CoroutineWrapper : Il2CppSystem.Object
    {
        public CoroutineWrapper(IntPtr ptr) : base(ptr) { }

        /// <summary>
        /// Creates an Il2Cpp IEnumerator with a System IEnumerator. Needs to be cast to Il2CppSystem.Collections.IEnumerator with the TryCast method
        /// </summary>
        public CoroutineWrapper(IEnumerator enumerator) : base(ClassInjector.DerivedConstructorPointer<CoroutineWrapper>())
        {
            ClassInjector.DerivedConstructorBody(this);
            Enumerator = enumerator;
        }

        private IEnumerator Enumerator { [HideFromIl2Cpp] get; [HideFromIl2Cpp] set; }

        public Il2CppSystem.Object Current
        {
            get
            {
                switch (Enumerator.Current)
                {
                    case IEnumerator systemEnumerator:
                        return new CoroutineWrapper(systemEnumerator);
                    case Il2CppSystem.Collections.IEnumerator il2cppEnumerator:
                        return new Il2CppCoroutineWrapper(il2cppEnumerator);
                }

                if (Enumerator.Current is not null and not Il2CppSystem.Object)
                {
                    Logger<ReactorPlugin>.Warning("Encountered unexpected yield value in coroutine, of type " + Enumerator.Current?.GetType().FullName);
                }

                return Enumerator.Current as Il2CppSystem.Object;
            }
        }

        public bool MoveNext() => Enumerator.MoveNext();

        public void Reset() => Enumerator.Reset();
    }

    [RegisterInIl2Cpp(typeof(Il2CppSystem.Collections.IEnumerator))]
    public sealed class Il2CppCoroutineWrapper : Il2CppSystem.Object
    {
        public Il2CppCoroutineWrapper(IntPtr ptr) : base(ptr) { }

        /// <summary>
        /// Creates an Il2Cpp IEnumerator class with an Il2Cpp IEnumerator method.
        /// </summary>
        public Il2CppCoroutineWrapper(Il2CppSystem.Collections.IEnumerator enumerator) : base(ClassInjector.DerivedConstructorPointer<Il2CppCoroutineWrapper>())
        {
            ClassInjector.DerivedConstructorBody(this);
            Enumerator = enumerator;
        }

        private Il2CppSystem.Collections.IEnumerator Enumerator { [HideFromIl2Cpp] get; [HideFromIl2Cpp] set; }

        public Il2CppSystem.Object Current => Enumerator.Current;

        public bool MoveNext() => Enumerator.MoveNext();

        public void Reset() => Enumerator.Reset();
    }
}
