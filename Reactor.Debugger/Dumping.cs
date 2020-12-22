using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using BepInEx;
using UnityEngine;

namespace Reactor.Debugger
{
    public static class Dumping
    {
        public static string DumpPath { get; } = Path.Combine(Paths.PluginPath, "dump");

        public static void Dump()
        {
            Directory.CreateDirectory(DumpPath);

            WritePalette();
            WriteEnum("Hat", HatManager.Instance.AllHats.ToArray().Select(hat => hat.name));
            WriteEnum("Pet", HatManager.Instance.AllPets.ToArray().Select(hat => hat.name));
            WriteEnum("Skin", HatManager.Instance.AllSkins.ToArray().Select(hat => hat.name));
        }

        private static void WritePalette()
        {
            var builder = new StringBuilder();

            builder.AppendLine("public static class Palette");
            builder.AppendLine("{");

            foreach (var propertyInfo in typeof(Palette).GetProperties())
            {
                if (propertyInfo.GetMethod == null || !propertyInfo.GetMethod.IsStatic)
                    continue;

                var value = propertyInfo.GetValue(null);

                if (value is Color f)
                {
                    value = (Color32) f;
                }

                if (value is Color32 color)
                {
                    builder.AppendLine($"    public static readonly Color {propertyInfo.Name} = Color.FromArgb({(color.a != byte.MaxValue ? $"{color.a}, " : string.Empty)}{color.r}, {color.g}, {color.b});");
                }
            }

            builder.AppendLine("}");

            File.WriteAllText(Path.Combine(DumpPath, $"Palette.cs"), builder.ToString());
        }

        private static void WriteEnum(string name, IEnumerable<string> values)
        {
            var builder = new StringBuilder();

            builder.AppendLine("public enum " + name);
            builder.AppendLine("{");

            foreach (var value in values)
            {
                builder.AppendLine("    " + value.Replace(" ", "").Replace("'", "") + ",");
            }

            builder.AppendLine("}");

            File.WriteAllText(Path.Combine(DumpPath, $"{name}.cs"), builder.ToString());
        }
    }
}
