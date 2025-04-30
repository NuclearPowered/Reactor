using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Reactor.Utilities;

/// <summary>
/// Provides utilities for working with unity canvas ui.
/// </summary>
public static class CanvasUtilities
{
    /// <summary>
    /// Creates a canvas.
    /// </summary>
    /// <returns>A canvas <see cref="GameObject"/>.</returns>
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

    /// <summary>
    /// Creates an EventSystem.
    /// </summary>
    /// <returns>An EventSystem <see cref="GameObject"/>.</returns>
    public static GameObject CreateEventSystem()
    {
        var gameObject = new GameObject("EventSystem");
        gameObject.AddComponent<EventSystem>();
        gameObject.AddComponent<StandaloneInputModule>();
        gameObject.AddComponent<BaseInput>();

        return gameObject;
    }

    /// <summary>
    /// Creates an empty texture of specified <paramref name="width"/> and <paramref name="height"/>.
    /// </summary>
    /// <param name="width">The width of the texture.</param>
    /// <param name="height">The height of the texture.</param>
    /// <returns>An empty texture of specified size.</returns>
    public static Texture2D CreateEmptyTexture(int width = 0, int height = 0)
    {
        return new Texture2D(width, height, TextureFormat.RGBA32, Texture.GenerateAllMips, false);
    }
}
