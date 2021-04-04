using System;
using UnityEngine.SceneManagement;

namespace Reactor.Patches
{
    internal static class SplashSkip
    {
        internal static void Initialize()
        {
            SceneManager.add_sceneLoaded((Action<Scene, LoadSceneMode>) ((scene, _) =>
            {
                if (scene.name == "SplashIntro")
                {
                    SceneManager.LoadScene("MainMenu");
                }
            }));
        }
    }
}
