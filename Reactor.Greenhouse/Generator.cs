using System.Linq;
using Mono.Cecil;
using Reactor.OxygenFilter;

namespace Reactor.Greenhouse
{
    public static class Generator
    {
        public static Mappings Generate(ModuleDefinition old, ModuleDefinition latest)
        {
            var result = new Mappings();

            foreach (var oldType in old.Types)
            {
                if (oldType.Name.StartsWith("<") || oldType.Namespace.StartsWith("GoogleMobileAds"))
                    continue;

                void AddType(TypeDefinition type)
                {
                    var mapped = new MappedType(new OriginalDescriptor { Name = type.Name }, oldType.Name);

                    var i = 0;
                    foreach (var field in type.Fields)
                    {
                        var j = 0;
                        var i1 = i;
                        var oldFields = oldType.Fields.Where(x => j++ == i1 && x.GetSignature().ToString() == field.GetSignature().ToString()).ToArray();
                        if (oldFields.Length == 1)
                        {
                            var oldField = oldFields.First();
                            if (oldField.Name == field.Name || !field.Name.IsObfuscated())
                                continue;

                            mapped.Fields.Add(new MappedMember(new OriginalDescriptor { Index = i1 }, oldField.Name));
                        }

                        i++;
                    }

                    foreach (var method in type.Methods)
                    {
                        if (!method.HasParameters || method.IsSetter || method.IsGetter)
                        {
                            continue;
                        }

                        var oldMethods = oldType.Methods.Where(x => x.Name == method.Name && x.Parameters.Count == method.Parameters.Count).ToArray();
                        if (oldMethods.Length != 1)
                        {
                            continue;
                        }

                        var oldMethod = oldMethods.Single();

                        var oldParameters = oldMethod.Parameters.Select(x => x.Name).ToList();

                        if (method.Parameters.Select(x => x.Name).SequenceEqual(oldParameters))
                        {
                            continue;
                        }

                        mapped.Methods.Add(new MappedMethod(new OriginalDescriptor { Name = method.Name, Signature = method.GetSignature() }, null)
                        {
                            Parameters = oldParameters
                        });
                    }

                    if (type.Name == oldType.Name || (!type.Name.IsObfuscated() && !mapped.Fields.Any() && !mapped.Methods.Any()))
                    {
                        return;
                    }

                    result.Types.Add(mapped);
                }

                var exact = latest.Types.FirstOrDefault(x => x.FullName == oldType.FullName);
                if (exact != null)
                {
                    AddType(exact);
                    continue;
                }

                if (oldType.IsEnum)
                {
                    var first = oldType.Fields.Select(x => x.Name).ToArray();
                    var type = latest.Types.SingleOrDefault(x => x.IsEnum && x.Fields.Select(f => f.Name).SequenceEqual(first));
                    if (type != null)
                    {
                        AddType(type);
                    }

                    continue;
                }

                static bool Test(TypeReference typeReference)
                {
                    return typeReference.IsGenericParameter || typeReference.Namespace != string.Empty || !typeReference.Name.IsObfuscated();
                }

                var methods = oldType.Methods.Where(x => Test(x.ReturnType) && x.Parameters.All(p => Test(p.ParameterType))).Select(x => x.GetSignature()).ToArray();
                var fields = oldType.Fields.Where(x => Test(x.FieldType) && (!x.FieldType.HasGenericParameters || x.FieldType.GenericParameters.All(Test))).Select(x => x.GetSignature()).ToArray();
                var properties = oldType.Properties.Select(x => x.Name).ToArray();

                var types = latest.Types
                    .Where(t => t.Attributes == oldType.Attributes)
                    .ToArray();

                TypeDefinition winner = null;
                var winnerPoints = -1;

                foreach (var t in types)
                {
                    var points = 0;
                    points += t.Properties.Count(p => properties.Contains((p.GetMethod?.Name ?? p.SetMethod.Name).Substring(4)));
                    points += fields.Count(s => t.Fields.Any(f => f.GetSignature().ToString() == s.ToString()));
                    points += methods.Count(s => t.Methods.Any(m => m.GetSignature().ToString() == s.ToString()));

                    if (points > winnerPoints)
                    {
                        winnerPoints = points;
                        winner = t;
                    }
                    else if (points == winnerPoints)
                    {
                        winner = null;
                    }
                }

                if (winner != null && winnerPoints > 0)
                {
                    AddType(winner);
                }
            }

            return result;
        }
    }
}
