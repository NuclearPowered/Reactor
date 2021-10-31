using Reactor.Extensions;
using UnityEngine;
using UnityEngine.UI;

namespace Reactor.Unstrip
{
    internal static class DefaultBundle
    {
        public static void Load()
        {
            using var stream = typeof(ReactorPlugin).Assembly.GetManifestResourceStream("Reactor.Assets.default.bundle");
            var bundle = AssetBundle.LoadFromStream(stream.AsIl2Cpp());

            var backupShader = bundle.LoadAsset<Shader>("UI-Default");

            if (!Graphic.defaultGraphicMaterial.shader || Graphic.defaultGraphicMaterial.shader.name != "UI/Default")
            {
                Graphic.defaultGraphicMaterial.shader = backupShader;
            }

            GUIExtensions.StandardResources = new DefaultControls.Resources
            {
                background = bundle.LoadAsset<Sprite>("Background").DontUnload(),
                checkmark = bundle.LoadAsset<Sprite>("Checkmark").DontUnload(),
                dropdown = bundle.LoadAsset<Sprite>("DropdownArrow").DontUnload(),
                knob = bundle.LoadAsset<Sprite>("Knob").DontUnload(),
                mask = bundle.LoadAsset<Sprite>("UIMask").DontUnload(),
                standard = bundle.LoadAsset<Sprite>("UISprite").DontUnload(),
                inputField = bundle.LoadAsset<Sprite>("InputFieldBackground").DontUnload()
            };

            bundle.Unload(false);
        }
    }
}
