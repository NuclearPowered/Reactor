using Reactor.Utilities;
using Reactor.Utilities.Extensions;
using UnityEngine;
using UnityEngine.UI;

namespace Reactor.Resources;

// TODO: this needs to be moved
internal static class DefaultBundle
{
    public static DefaultControls.Resources StandardResources { get; internal set; } = null!;

    public static void Load()
    {
        var bundle = AssetBundleManager.Load("default");

        var backupShader = bundle.LoadAsset<Shader>("UI-Default");

        if (!Graphic.defaultGraphicMaterial.shader || Graphic.defaultGraphicMaterial.shader.name != "UI/Default")
        {
            Graphic.defaultGraphicMaterial.shader = backupShader;
        }

        StandardResources = new DefaultControls.Resources
        {
            background = bundle.LoadAsset<Sprite>("Background")!.DontUnload(),
            checkmark = bundle.LoadAsset<Sprite>("Checkmark")!.DontUnload(),
            dropdown = bundle.LoadAsset<Sprite>("DropdownArrow")!.DontUnload(),
            knob = bundle.LoadAsset<Sprite>("Knob")!.DontUnload(),
            mask = bundle.LoadAsset<Sprite>("UIMask")!.DontUnload(),
            standard = bundle.LoadAsset<Sprite>("UISprite")!.DontUnload(),
            inputField = bundle.LoadAsset<Sprite>("InputFieldBackground")!.DontUnload(),
        };

        bundle.Unload(false);
    }
}
