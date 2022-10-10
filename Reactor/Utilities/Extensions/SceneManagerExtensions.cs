using System;
using System.Diagnostics.CodeAnalysis;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace Reactor.Utilities.Extensions;

/// <summary>
/// Provides extension methods for <see cref="SceneManager"/>.
/// </summary>
public static class SceneManagerExtensions
{
    /// <summary>
    /// Adds a delegate to get one notification when a scene has loaded.
    /// </summary>
    /// <param name="value">The action.</param>
    [SuppressMessage("Naming", "CA1707:Identifiers should not contain underscores", Justification = "This supplements existing methods like add_sceneLoaded and remove_sceneLoaded")]
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
