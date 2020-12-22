using System;
using Reactor.Extensions;
using UnityEngine;
using UnityEngine.UI;

namespace Reactor.Unstrip
{
    internal static class DefaultBundle
    {
        public static void Load()
        {
            var bundle = AssetBundle.LoadFromMemory(typeof(ReactorPlugin).Assembly.GetManifestResourceStream("Reactor.Assets.default.bundle").ReadFully());

            var backupShader = bundle.LoadAsset<Shader>("UI-Default");

            if (!Graphic.defaultGraphicMaterial.shader || Graphic.defaultGraphicMaterial.shader.name != "UI/Default")
            {
                Graphic.defaultGraphicMaterial.shader = backupShader;
            }

            var tex = new Texture2D(1, 1, TextureFormat.RGBA32, Texture.GenerateAllMips, false, IntPtr.Zero);
            tex.SetPixel(0, 0, Color.red);

            var sprite = tex.CreateSprite();

            GUIExtensions.StandardResources = new DefaultControls.Resources
            {
                background = sprite,
                checkmark = sprite,
                dropdown = sprite,
                knob = sprite,
                mask = sprite,
                standard = sprite,
                inputField = sprite
            };
        }
    }
}
