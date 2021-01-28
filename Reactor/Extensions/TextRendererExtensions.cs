using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Reactor.Extensions
{
    public static class TextRendererExtensions
    {
        public static TextRendererPrefab Prefab { get; internal set; }

        public static TextRenderer AddTextRenderer(this GameObject gameObject)
        {
            if (Prefab == null)
            {
                throw new ArgumentException("TextRenderer prefab hasn't been initialized yet");
            }

            var renderer = gameObject.AddComponent<MeshRenderer>();
            renderer.material = Prefab.Material;

            gameObject.AddComponent<MeshFilter>();

            var textRenderer = gameObject.AddComponent<TextRenderer>();
            textRenderer.FontData = Prefab.FontData;

            return textRenderer;
        }
    }

    public class TextRendererPrefab
    {
        public Material Material { get; }
        public TextAsset FontData { get; }

        internal TextRendererPrefab(TextRenderer textRenderer)
        {
            Material = Object.Instantiate(textRenderer.GetComponent<MeshRenderer>().material).DontDestroy();
            FontData = Object.Instantiate(textRenderer.FontData).DontDestroy();
        }
    }
}
