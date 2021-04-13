using System;
using System.Linq;
using UnityEngine;

namespace Reactor.Extensions
{
    public static class TextRendererExtensions
    {
        private static readonly Lazy<TextAsset> _fontData = new Lazy<TextAsset>(() => FontCache.Instance.DefaultFonts.ToArray().Single(x => x.name == "Arial"));

        public static TextRenderer AddTextRenderer(this GameObject gameObject)
        {
            var renderer = gameObject.AddComponent<MeshRenderer>();
            renderer.material = new Material(Shader.Find("Unlit/VectFontShader"));

            gameObject.AddComponent<MeshFilter>();

            var textRenderer = gameObject.AddComponent<TextRenderer>();
            textRenderer.FontData = _fontData.Value;

            return textRenderer;
        }
    }
}
