using System;
using System.Collections.Generic;
using UnhollowerBaseLib.Attributes;
using UnityEngine;

namespace Reactor.Extensions;

[RegisterInIl2Cpp]
public class Dispatcher : MonoBehaviour
{
    public Dispatcher(IntPtr ptr) : base(ptr)
    {
    }

    public static Dispatcher Instance { get; private set; } = null!;

    private static readonly Queue<Action> _queue = new();

    private void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        lock (_queue)
        {
            while (_queue.Count > 0)
            {
                _queue.Dequeue().Invoke();
            }
        }
    }

    [HideFromIl2Cpp]
    public void Enqueue(Action action)
    {
        lock (_queue)
        {
            _queue.Enqueue(action);
        }
    }
}
