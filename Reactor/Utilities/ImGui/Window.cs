using System;
using Reactor.Utilities.Extensions;
using UnityEngine;

namespace Reactor.Utilities.ImGui;

/// <summary>
/// Utility wrapper over <see cref="GUI"/>.<see cref="GUI.Window(int,UnityEngine.Rect,UnityEngine.GUI.WindowFunction,UnityEngine.GUIContent,UnityEngine.GUIStyle)"/>.
/// </summary>
public class Window
{
    private static int _lastWindowId = 2135184938;

    /// <summary>
    /// Gets the next window id.
    /// </summary>
    /// <returns>A window id.</returns>
    public static int NextWindowId()
    {
        return _lastWindowId++;
    }

    /// <summary>
    /// Gets or sets the id of the window.
    /// </summary>
    public int Id { get; set; } = NextWindowId();

    /// <summary>
    /// Gets or sets a value indicating whether the window is enabled and shown.
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Gets or sets the rect of the window.
    /// </summary>
    public Rect Rect { get; set; }

    /// <summary>
    /// Gets or sets the render function of the window.
    /// </summary>
    public Action<int> Func { get; set; }

    /// <summary>
    /// Gets or sets the title of the window.
    /// </summary>
    public string Title { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Window"/> class.
    /// </summary>
    /// <param name="rect">The rect.</param>
    /// <param name="title">The title.</param>
    /// <param name="func">The render function.</param>
    public Window(Rect rect, string title, Action<int> func)
    {
        Rect = rect;
        Title = title;
        Func = func;
    }

    /// <inheritdoc />
    public Window(Rect rect, string title, Action func) : this(rect, title, _ => func())
    {
    }

    /// <summary>
    /// Draws the window gui.
    /// </summary>
    public virtual void OnGUI()
    {
        if (Enabled)
        {
            if (Event.current.type == EventType.Layout)
            {
                Rect = Rect.ResetSize();
            }

            GUI.skin.label.wordWrap = false;
            Rect = GUILayout.Window(Id, Rect, Func, Title, GUILayout.MinWidth(GUI.skin.label.CalcSize(new GUIContent(Title)).x * 2));

            if ((Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1)) && Rect.Contains(new Vector2(Input.mousePosition.x, Screen.height - Input.mousePosition.y)))
            {
                Input.ResetInputAxes();
            }
        }
    }
}
