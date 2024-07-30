using System;
using System.Collections;
using System.Linq;
using AmongUs.Data;
using BepInEx.Unity.IL2CPP.Utils;
using Il2CppInterop.Runtime.Attributes;
using Il2CppInterop.Runtime.InteropTypes;
using Reactor.Debugger.Utilities;
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

            if (GUILayout.Button("Hard crash"))
            {
                static unsafe void Corrupt(Il2CppObjectBase o)
                {
                    var x = (IntPtr*) o.Pointer;
                    x[0] = (IntPtr) 0xF00;
                }

                static IEnumerator CoCrash()
                {
                    if (!PlayerControl.LocalPlayer || !ShipStatus.Instance)
                    {
                        yield return AmongUsClient.Instance.CoCreateLocalGame(true);

                        while (!PlayerControl.LocalPlayer || !ShipStatus.Instance)
                        {
                            yield return null;
                        }
                    }

                    var usable = FindObjectsOfType<MonoBehaviour>().First(x => x.TryCast<IUsable>() != null);
                    if (!usable)
                    {
                        Error("Failed to find an IUsable to crash with");
                        yield break;
                    }

                    var cloned = Instantiate(usable, PlayerControl.LocalPlayer.transform.position, default);

                    Warning($"Crashing with {cloned.name}");

                    Corrupt(cloned);
                }

                this.StartCoroutine(CoCrash());
            }

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
