using System;
using Reactor.Utilities.Extensions;
using UnityEngine;

namespace Reactor.Utilities.ImGui;

/// <summary>
/// Draggable version of <see cref="Window"/>.
/// </summary>
public class DragWindow : Window
{
    /// <inheritdoc />
    public DragWindow(Rect rect, string title, Action<int> func) : base(rect, title, func)
    {
        Func = id =>
        {
            func(id);

            GUI.DragWindow(new Rect(0, 0, 10000, 20));
            Rect = Rect.ClampScreen();
        };
    }

    /// <inheritdoc />
    public DragWindow(Rect rect, string title, Action func) : this(rect, title, _ => func())
    {
    }
}
