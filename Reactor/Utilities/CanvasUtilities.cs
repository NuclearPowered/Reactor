using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Reactor.Utilities;

public static class CanvasUtilities
{
    public static GameObject CreateCanvas()
    {
        var gameObject = new GameObject("Canvas");
        var canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        var canvasScaler = gameObject.AddComponent<CanvasScaler>();

        canvasScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
        canvasScaler.referencePixelsPerUnit = 100f;

        gameObject.AddComponent<GraphicRaycaster>();

        return gameObject;
    }

    public static GameObject CreateEventSystem()
    {
        var gameObject = new GameObject("EventSystem");
        gameObject.AddComponent<EventSystem>();
        gameObject.AddComponent<StandaloneInputModule>();
        gameObject.AddComponent<BaseInput>();

        return gameObject;
    }

    /// <summary>
    /// Shortcut for empty texture
    /// </summary>
    public static Texture2D CreateEmptyTexture(int width = 0, int height = 0)
    {
        return new Texture2D(width, height, TextureFormat.RGBA32, Texture.GenerateAllMips, false, IntPtr.Zero);
    }
}
