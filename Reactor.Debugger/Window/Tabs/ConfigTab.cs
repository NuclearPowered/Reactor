using System.Linq;
using BepInEx.Configuration;
using Reactor.Utilities;
using UnityEngine;

namespace Reactor.Debugger.Window.Tabs;

internal sealed class ConfigTab : BaseTab
{
    public override string Name => "Config";

    public override void OnGUI()
    {
        var configs = PluginSingleton<ReactorPlugin>.Instance.Config.Concat(PluginSingleton<DebuggerPlugin>.Instance.Config);

        foreach (var section in configs.GroupBy(e => e.Key.Section))
        {
            GUILayout.Label(section.Key);

            foreach (var (definition, entry) in section)
            {
                if (entry is ConfigEntry<bool> booleanEntry)
                {
                    booleanEntry.Value = GUILayout.Toggle(booleanEntry.Value, definition.Key);
                }
            }
        }
    }
}
