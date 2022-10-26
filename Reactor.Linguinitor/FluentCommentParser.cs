using System.Text;
using System.Text.RegularExpressions;
using Linguini.Syntax.Ast;

namespace Reactor.Linguinitor;

internal static class FluentCommentParser
{
    private static readonly Regex _variableCommentRegex = new(@"^\$(?<id>[\w-]+) \((?<type>String|Number|Date)\)( - (?<description>.+))?$", RegexOptions.Compiled);

    public static MessageInfo Parse(AstComment astComment)
    {
        var descriptionBuilder = new StringBuilder();
        var variables = new Dictionary<string, VariableInfo>();

        foreach (var s in astComment.AsStr().Split("\n"))
        {
            var line = s.Trim();

            if (_variableCommentRegex.Match(line) is { Success: true } match)
            {
                variables.Add(match.Groups["id"].Value, new VariableInfo(match.Groups["type"].Value, match.Groups["description"].Value));
            }
            else
            {
                descriptionBuilder.AppendLine(line);
            }
        }

        return new MessageInfo(descriptionBuilder.ToString(), variables);
    }

    public record MessageInfo(string Description, Dictionary<string, VariableInfo> Variables);

    public record VariableInfo(string Type, string? Description);
}
