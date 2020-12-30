// https://github.com/HerpDerpinstine/MelonLoader/blob/master/MelonLoader.Support.Il2Cpp/MelonCoroutines.cs

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnhollowerBaseLib;
using UnityEngine;

namespace Reactor
{
    public static class Coroutines
    {
        private struct CoroutineTuple
        {
            public object WaitCondition;
            public IEnumerator Coroutine;
        }

        private static readonly List<CoroutineTuple> _ourCoroutinesStore = new List<CoroutineTuple>();
        private static readonly List<IEnumerator> _ourNextFrameCoroutines = new List<IEnumerator>();
        private static readonly List<IEnumerator> _ourWaitForFixedUpdateCoroutines = new List<IEnumerator>();
        private static readonly List<IEnumerator> _ourWaitForEndOfFrameCoroutines = new List<IEnumerator>();

        private static readonly List<IEnumerator> _tempList = new List<IEnumerator>();

        public static object Start(IEnumerator routine)
        {
            if (routine != null) ProcessNextOfCoroutine(routine);
            return routine;
        }

        public static void Stop(IEnumerator enumerator)
        {
            if (_ourNextFrameCoroutines.Contains(enumerator)) // the coroutine is running itself
            {
                _ourNextFrameCoroutines.Remove(enumerator);
            }
            else
            {
                var coroutineTupleIndex = _ourCoroutinesStore.FindIndex(c => c.Coroutine == enumerator);
                if (coroutineTupleIndex != -1) // the coroutine is waiting for a subroutine
                {
                    var waitCondition = _ourCoroutinesStore[coroutineTupleIndex].WaitCondition;
                    if (waitCondition is IEnumerator waitEnumerator)
                    {
                        Stop(waitEnumerator);
                    }

                    _ourCoroutinesStore.RemoveAt(coroutineTupleIndex);
                }
            }
        }

        private static void ProcessCoroutineList(List<IEnumerator> target)
        {
            if (target.Count == 0) return;

            // use a temp list to make sure waits made during processing are not handled by same processing invocation
            // additionally, a temp list reduces allocations compared to an array
            _tempList.AddRange(target);
            target.Clear();
            foreach (var enumerator in _tempList) ProcessNextOfCoroutine(enumerator);
            _tempList.Clear();
        }

        internal static void Process()
        {
            for (var i = _ourCoroutinesStore.Count - 1; i >= 0; i--)
            {
                var tuple = _ourCoroutinesStore[i];
                if (tuple.WaitCondition is WaitForSeconds waitForSeconds)
                {
                    if ((waitForSeconds.m_Seconds -= Time.deltaTime) <= 0)
                    {
                        _ourCoroutinesStore.RemoveAt(i);
                        ProcessNextOfCoroutine(tuple.Coroutine);
                    }
                }
            }

            ProcessCoroutineList(_ourNextFrameCoroutines);
        }

        internal static void ProcessWaitForFixedUpdate() => ProcessCoroutineList(_ourWaitForFixedUpdateCoroutines);

        internal static void ProcessWaitForEndOfFrame() => ProcessCoroutineList(_ourWaitForEndOfFrameCoroutines);

        private static void ProcessNextOfCoroutine(IEnumerator enumerator)
        {
            try
            {
                if (!enumerator.MoveNext()) // Run the next step of the coroutine. If it's done, restore the parent routine
                {
                    var indices = _ourCoroutinesStore.Select((it, idx) => (idx, it)).Where(it => it.it.WaitCondition == enumerator).Select(it => it.idx).ToList();
                    for (var i = indices.Count - 1; i >= 0; i--)
                    {
                        var index = indices[i];
                        _ourNextFrameCoroutines.Add(_ourCoroutinesStore[index].Coroutine);
                        _ourCoroutinesStore.RemoveAt(index);
                    }

                    return;
                }
            }
            catch (Exception e)
            {
                PluginSingleton<ReactorPlugin>.Instance.Log.LogError(e.ToString());
                Stop(FindOriginalCoroutine(enumerator)); // We want the entire coroutine hierarchy to stop when an error happen
            }

            var next = enumerator.Current;
            switch (next)
            {
                case null:
                    _ourNextFrameCoroutines.Add(enumerator);
                    return;
                case WaitForFixedUpdate _:
                    _ourWaitForFixedUpdateCoroutines.Add(enumerator);
                    return;
                case WaitForEndOfFrame _:
                    _ourWaitForEndOfFrameCoroutines.Add(enumerator);
                    return;
                case WaitForSeconds _:
                    break; // do nothing, this one is supported in Process
                case Il2CppObjectBase il2CppObjectBase:
                    var nextAsEnumerator = il2CppObjectBase.TryCast<Il2CppSystem.Collections.IEnumerator>();
                    if (nextAsEnumerator != null) // il2cpp IEnumerator also handles CustomYieldInstruction
                        next = new Il2CppEnumeratorWrapper(nextAsEnumerator);
                    else
                        PluginSingleton<ReactorPlugin>.Instance.Log.LogWarning($"Unknown coroutine yield object of type {il2CppObjectBase} for coroutine {enumerator}");
                    break;
            }

            _ourCoroutinesStore.Add(new CoroutineTuple { WaitCondition = next, Coroutine = enumerator });

            if (next is IEnumerator nextCoroutine)
                ProcessNextOfCoroutine(nextCoroutine);
        }

        private static IEnumerator FindOriginalCoroutine(IEnumerator enumerator)
        {
            var index = _ourCoroutinesStore.FindIndex(ct => ct.WaitCondition == enumerator);
            return index == -1 ? enumerator : FindOriginalCoroutine(_ourCoroutinesStore[index].Coroutine);
        }

        private class Il2CppEnumeratorWrapper : IEnumerator
        {
            private readonly Il2CppSystem.Collections.IEnumerator _il2CPPEnumerator;

            public Il2CppEnumeratorWrapper(Il2CppSystem.Collections.IEnumerator il2CppEnumerator) => _il2CPPEnumerator = il2CppEnumerator;
            public bool MoveNext() => _il2CPPEnumerator.MoveNext();
            public void Reset() => _il2CPPEnumerator.Reset();
            public object Current => _il2CPPEnumerator.Current;
        }
    }
}
