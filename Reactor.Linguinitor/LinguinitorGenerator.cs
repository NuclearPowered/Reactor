using CSharpPoet;
using Linguini.Syntax.Ast;
using Linguini.Syntax.Parser;
using Microsoft.CodeAnalysis;

namespace Reactor.Linguinitor;

[Generator(LanguageNames.CSharp)]
public class LinguinitorGenerator : IIncrementalGenerator
{
    private static CSharpFile CreateTranslateFile(string @namespace, params CSharpType.IMember[] members)
    {
        return new CSharpFile(@namespace)
        {
            new CSharpClass(Visibility.Internal, "Translate")
            {
                IsStatic = true,
                IsPartial = true,
                Members = members,
            },
        };
    }

    private static IEnumerable<Identifier> GetVariables(AstMessage astMessage)
    {
        foreach (var placeable in astMessage.Value!.Elements.OfType<Placeable>())
        {
            switch (placeable.Expression)
            {
                case VariableReference variableReference:
                    yield return variableReference.Id;
                    break;
            }
        }
    }

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var rootNamespaceProvider =
            context.AnalyzerConfigOptionsProvider
                .Select((options, _) => options.GlobalOptions.TryGetValue("build_property.RootNamespace", out var rootNamespace) ? rootNamespace : throw new InvalidOperationException("Failed to get RootNamespace"));

        context.RegisterSourceOutput(rootNamespaceProvider, (spc, rootNamespace) =>
        {
            const string ProviderType = "Reactor.Localization.Providers.FluentLocalizationProvider";
            spc.AddSource("Translate", CreateTranslateFile(
                rootNamespace,
                new CSharpField(Visibility.Private, ProviderType + "?", "_provider") { IsStatic = true },
                new CSharpProperty(ProviderType, "Provider")
                {
                    IsStatic = true,
                    Getter = new CSharpProperty.Accessor
                    {
                        Body = writer => writer.Write("_provider ?? throw new System.InvalidOperationException(\"Provider must be set before using Translate\");"),
                    },
                    Setter = new CSharpProperty.Accessor
                    {
                        Body = writer => writer.Write("_provider = value;"),
                    },
                }
            ).ToString());
        });

        var textFiles = context.AdditionalTextsProvider.Where(static file => file.Path.EndsWith(".ftl", StringComparison.Ordinal));

        var namesAndContents = textFiles.Select((text, cancellationToken) => (
            Name: Path.GetFileNameWithoutExtension(text.Path),
            Content: text.GetText(cancellationToken)!.ToString()
        ));

        context.RegisterSourceOutput(namesAndContents.Combine(rootNamespaceProvider), (spc, pair) =>
        {
            var ((name, content), rootNamespace) = pair;
            var fixedName = name.ToPascalCase();

            var idsClass = new CSharpClass("Ids")
            {
                IsStatic = true,
            };

            var fileClass = new CSharpClass(fixedName)
            {
                IsStatic = true,
                Members = { idsClass },
            };

            var resource = new LinguiniParser(content).ParseWithComments();

            foreach (var message in resource.Entries.OfType<AstMessage>())
            {
                var id = message.GetId();
                var fixedId = id.ToPascalCase();

                idsClass.Members.Add(new CSharpField(Visibility.Public, "string", fixedId)
                {
                    IsConst = true,
                    DefaultValue = $"\"{id}\"",
                });

                var messageInfo = message.Comment != null ? FluentCommentParser.Parse(message.Comment) : null;
                var variables = GetVariables(message).Select(m => m.ToString()).ToDictionary(
                    m => m,
                    m =>
                    {
                        if (messageInfo != null && messageInfo.Variables.TryGetValue(m, out var variableInfo))
                        {
                            return variableInfo.Type switch
                            {
                                "String" => "string",
                                "Number" => "double",
                                _ => throw new NotSupportedException($"Variable type {variableInfo.Type} is not supported"),
                            };
                        }

                        return "string";
                    }
                );

                void WriteComment(CodeWriter writer)
                {
                    if (messageInfo == null) return;

                    writer.WriteLine("<summary>");

                    foreach (var line in messageInfo.Description.Trim().Split("\n"))
                    {
                        writer.WriteLine(line);
                    }

                    writer.WriteLine("</summary>");

                    foreach (var (variableId, info) in messageInfo.Variables)
                    {
                        writer.WriteLine($"<param name=\"{variableId}\">{info.Description}</param>");
                    }
                }

                if (variables.Count <= 0)
                {
                    fileClass.Members.Add(new CSharpProperty("string", fixedId)
                    {
                        XmlComment = WriteComment,
                        IsStatic = true,
                        Getter = new CSharpProperty.Accessor
                        {
                            Body = writer => writer.Write($"Provider.GetText(Ids.{fixedId});"),
                        },
                    });
                }
                else
                {
                    fileClass.Members.Add(new CSharpMethod("string", fixedId)
                    {
                        XmlComment = WriteComment,
                        IsStatic = true,
                        Parameters = variables.Select(v => new CSharpParameter(v.Value, v.Key)).ToArray(),
                        BodyType = BodyType.Expression,
                        Body = writer => writer.WriteLine($"Provider.GetText(Ids.{fixedId}, {string.Join(", ", variables.Select(v => $"(\"{v.Key}\", {v.Key.ToCamelCase()})"))});"),
                    });
                }
            }

            spc.AddSource("Translate." + fixedName, CreateTranslateFile(rootNamespace, fileClass).ToString());
        });
    }
}
