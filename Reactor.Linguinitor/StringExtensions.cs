using System.Text;

namespace Reactor.Linguinitor;

internal static class StringExtensions
{
    private static string ToCase(this string value, bool capitalizeFirst)
    {
        var stringBuilder = new StringBuilder();

        for (var i = 0; i < value.Length; i++)
        {
            var c = value[i];

            if (c == '-')
            {
                i++;
                stringBuilder.Append(char.ToUpperInvariant(value[i]));
            }
            else
            {
                stringBuilder.Append(capitalizeFirst && i == 0 ? char.ToUpperInvariant(c) : c);
            }
        }

        return stringBuilder.ToString();
    }

    public static string ToPascalCase(this string value)
    {
        return value.ToCase(true);
    }

    public static string ToCamelCase(this string value)
    {
        return value.ToCase(false);
    }
}
