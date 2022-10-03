using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace Reactor.Networking.Extensions;

public static class UnityNetworkingExtensions
{
    /// <summary>
    /// Sends <see cref="UnityWebRequest"/> and return task that finishes on <paramref name="request"/> completion.
    /// </summary>
    public static Task SendAsync(this UnityWebRequest request)
    {
        var task = new TaskCompletionSource<object?>();

        request.Send().m_completeCallback = (Action<AsyncOperation>) (_ =>
        {
            task.SetResult(null);
        });

        return task.Task;
    }
}
