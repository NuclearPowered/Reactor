using System;
using Reactor.Utilities.Extensions;
using UnityEngine;

namespace Reactor.Utilities.ImGui;

public class Window
{
    private static int _lastWindowId;

    public static int NextWindowId()
    {
        return _lastWindowId++;
    }

    public int Id { get; set; } = NextWindowId();

    public bool Enabled { get; set; } = true;
    public Rect Rect { get; set; }
    public Action<int> Func { get; set; }
    public string Title { get; set; }

    public Window(Rect rect, string title, Action<int> func)
    {
        Rect = rect;
        Title = title;
        Func = func;
    }

    public Window(Rect rect, string title, Action func) : this(rect, title, id => func())
    {
    }

    public void OnGUI()
    {
        if (Enabled)
        {
            if (Event.current.type == EventType.Layout)
            {
                Rect = Rect.ResetSize();
            }

            Rect = GUILayout.Window(Id, Rect, Func, Title);

            if ((Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1)) && Rect.Contains(new Vector2(Input.mousePosition.x, Screen.height - Input.mousePosition.y)))
            {
                Input.ResetInputAxes();
            }
        }
    }
}
