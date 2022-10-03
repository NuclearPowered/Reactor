using System;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace Reactor.Utilities;

public static class SceneManagerUtils
{
    /// <summary>
    /// Adds a delegate to get one notification when a scene has loaded.
    /// </summary>
    public static void once_sceneLoaded(Action<Scene, LoadSceneMode> value)
    {
        UnityAction<Scene, LoadSceneMode>? unityAction = null;

        unityAction = (Action<Scene, LoadSceneMode>) ((scene, loadMode) =>
        {
            SceneManager.remove_sceneLoaded(unityAction);
            value.Invoke(scene, loadMode);
        });

        SceneManager.add_sceneLoaded(unityAction);
    }
}
