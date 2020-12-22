using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using Mono.Cecil;
using Reactor.OxygenFilter;

namespace Reactor.Greenhouse
{
    public static class Compiler
    {
        public static void Apply(this Mappings current, Mappings mappings)
        {
            static void ApplyType(List<MappedType> types, MappedType newType)
            {
                var type = types.SingleOrDefault(x => newType.Equals(x));
                if (type == null)
                {
                    types.Add(newType);
                }
                else
                {
                    type.Mapped = newType.Mapped;

                    void ApplyMembers(List<MappedMember> members, List<MappedMember> newMembers)
                    {
                        foreach (var newMember in newMembers)
                        {
                            var member = members.SingleOrDefault(x => newMember.Equals(x, false));

                            if (member == null)
                            {
                                members.Add(newMember);
                            }
                            else
                            {
                                member.Mapped = newMember.Mapped;
                                member.Original = newMember.Original;
                            }
                        }
                    }

                    ApplyMembers(type.Fields, newType.Fields);
                    ApplyMembers(type.Properties, newType.Properties);

                    foreach (var newMember in newType.Methods)
                    {
                        var member = type.Methods.SingleOrDefault(x => newMember.Equals(x, false));

                        if (member == null)
                        {
                            type.Methods.Add(newMember);
                        }
                        else
                        {
                            member.Mapped = newMember.Mapped;
                            member.Original = newMember.Original;
                            member.Parameters = newMember.Parameters;
                        }
                    }

                    foreach (var nestedType in newType.Nested)
                    {
                        ApplyType(type.Nested, nestedType);
                    }
                }
            }

            foreach (var newType in mappings.Types)
            {
                ApplyType(current.Types, newType);
            }
        }

        private static Regex Regex { get; } = new Regex(@"{{(?<expression>[\w\.]+)}}", RegexOptions.Compiled);

        public static string MapSignature(this OriginalDescriptor original, Mappings mappings)
        {
            var signature = original.Signature;

            var matches = Regex.Matches(signature);
            foreach (Match match in matches)
            {
                var group = match.Groups["expression"];
                if (group.Success)
                {
                    signature = signature.Replace(match.Value, mappings.FindByMapped(group.Value).Original.Name);
                }
            }

            return signature;
        }

        private static bool TestField(MappedMember field, TypeDefinition typeDef, FieldDefinition fieldDef, Mappings mappings)
        {
            return (field.Original.Name == null || field.Original.Name == fieldDef.Name) &&
                   (field.Original.Index == null || field.Original.Index == typeDef.Fields.IndexOf(fieldDef)) &&
                   (field.Original.Signature == null || field.Original.MapSignature(mappings) == fieldDef.GetSignature()) &&
                   (field.Original.Const == null || !typeDef.IsEnum && field.Original.Const.Value.Equals(fieldDef.Constant));
        }

        private static bool TestMethod(MappedMember method, TypeDefinition typeDef, MethodDefinition methodDef, Mappings mappings)
        {
            return (method.Original.Index == null || method.Original.Index == typeDef.Methods.IndexOf(methodDef)) &&
                   (method.Original.Signature == null || method.Original.MapSignature(mappings) == methodDef.GetSignature()) &&
                   (method.Original.Name == null || method.Original.Name == methodDef.Name);
        }

        private static bool TestProperty(MappedMember property, TypeDefinition typeDef, PropertyDefinition propertyDef, Mappings mappings)
        {
            return (property.Original.Name == null || property.Original.Name == propertyDef.Name) &&
                   (property.Original.Index == null || property.Original.Index == typeDef.Properties.IndexOf(propertyDef)) &&
                   (property.Original.Signature == null || property.Original.MapSignature(mappings) == propertyDef.GetSignature());
        }

        private static bool TestType(MappedType type, TypeDefinition typeDef, Mappings mappings)
        {
            return (type.Original.Name == null || type.Original.Name == typeDef.Name) &&
                   type.Methods.All(method => typeDef.Methods.Any(m => TestMethod(method, typeDef, m, mappings))) &&
                   type.Fields.All(field => typeDef.Fields.Any(f => TestField(field, typeDef, f, mappings))) &&
                   type.Properties.All(property => typeDef.Properties.Any(p => TestProperty(property, typeDef, p, mappings)));
        }

        private static void Compile(this MappedType type, TypeDefinition typeDef, Mappings mappings)
        {
            type.Original = new OriginalDescriptor { Name = typeDef.Name };

            foreach (var nested in type.Nested)
            {
                var nestedDef = typeDef.NestedTypes.Single(t =>
                    TestType(type, t, mappings) &&
                    nested.Original.Index == null || typeDef.NestedTypes.IndexOf(t) == nested.Original.Index
                );

                nested.Compile(nestedDef, mappings);
            }

            foreach (var property in type.Properties)
            {
                try
                {
                    var propertyDef = typeDef.Properties.Single(p => TestProperty(property, typeDef, p, mappings));

                    property.Original = new OriginalDescriptor { Name = propertyDef.Name };
                }
                catch (Exception e)
                {
                    throw new Exception($"Compilation of {property} failed", e);
                }
            }

            foreach (var field in type.Fields)
            {
                try
                {
                    var fieldDef = typeDef.Fields.Single(f => TestField(field, typeDef, f, mappings));

                    field.Original = new OriginalDescriptor { Name = fieldDef.Name };
                }
                catch (Exception e)
                {
                    throw new Exception($"Compilation of {field} failed", e);
                }
            }

            foreach (var method in type.Methods)
            {
                try
                {
                    var methodDef = typeDef.Methods.Single(m => TestMethod(method, typeDef, m, mappings));

                    method.Original = new OriginalDescriptor { Name = methodDef.Name, Signature = methodDef.GetSignature() };
                }
                catch (Exception e)
                {
                    throw new Exception($"Compilation of {method} failed", e);
                }
            }
        }

        public static void Compile(this Mappings mappings, ModuleDefinition moduleDef)
        {
            foreach (var type in mappings.Types)
            {
                try
                {
                    var typeDef = moduleDef.Types.Single(t => TestType(type, t, mappings));

                    type.Compile(typeDef, mappings);
                }
                catch (Exception e)
                {
                    throw new Exception($"Compilation of {type} failed", e);
                }
            }
        }
    }
}
