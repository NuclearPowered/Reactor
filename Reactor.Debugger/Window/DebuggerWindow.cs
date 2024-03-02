using System;
using AmongUs.Data;
using Il2CppInterop.Runtime.Attributes;
using Reactor.Debugger.Window.Tabs;
using Reactor.Utilities.Attributes;
using Reactor.Utilities.ImGui;
using UnityEngine;

namespace Reactor.Debugger.Window;

[RegisterInIl2Cpp]
internal sealed class DebuggerWindow : MonoBehaviour
{
    private readonly DragWindow _window;

    [HideFromIl2Cpp]
    public BaseTab[] Tabs { get; } =
    {
        new ConfigTab(),
        new GameTab(),
        new AutoJoinTab(),
    };

    [HideFromIl2Cpp]
    public BaseTab SelectedTab { get; private set; }

    public DebuggerWindow(IntPtr ptr) : base(ptr)
    {
        SelectedTab = Tabs[0];

        _window = new DragWindow(new Rect(20, 20, 0, 0), "Reactor.Debugger", () =>
        {
            var clientName = DataManager.Player.Customization.Name;
            if (AmongUsClient.Instance && AmongUsClient.Instance.AmHost) clientName += " (host)";
            GUILayout.Label("Name: " + clientName);

            GUILayout.BeginHorizontal();
            {
                foreach (var tab in Tabs)
                {
                    if (GUILayout.Toggle(tab == SelectedTab, tab.Name, new GUIStyle(GUI.skin.button)))
                    {
                        SelectedTab = tab;
                    }
                }
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(5f);

            SelectedTab.OnGUI();
        })
        {
            Enabled = false,
        };
    }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.F1))
        {
            _window.Enabled = !_window.Enabled;
        }
    }

    public void OnGUI()
    {
        _window.OnGUI();
    }
}
