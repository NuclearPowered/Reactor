using System;
using System.IO;
using System.Linq;
using Mono.Cecil;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Reactor.OxygenFilter
{
    public class ObfuscationMapper
    {
        public ModuleDefinition ModuleDef { get; }
        public Mappings Mappings { get; private set; }

        public ObfuscationMapper(ModuleDefinition moduleDef)
        {
            ModuleDef = moduleDef;
        }

        public void Map()
        {
            var mappedAttribute = new TypeDefinition("", "MappedAttribute", TypeAttributes.Public, ModuleDef.ImportReference(typeof(Attribute)))
            {
                Attributes = TypeAttributes.Class & TypeAttributes.Public
            };

            ModuleDef.Types.Add(mappedAttribute);

            var mappedAttributeCtor = new MethodReference(".ctor", ModuleDef.ImportReference(typeof(void)), mappedAttribute)
            {
                HasThis = true,
                Parameters = { new ParameterDefinition(ModuleDef.ImportReference(typeof(string))) }
            };

            void MapMember(ICustomAttributeProvider attributes, string mapped)
            {
                attributes.CustomAttributes.Add(new CustomAttribute(mappedAttributeCtor)
                {
                    ConstructorArguments = { new CustomAttributeArgument(ModuleDef.ImportReference(typeof(string)), mapped) }
                });
            }

            // beebyte 1 iq?
            foreach (var propertyDef in ModuleDef.Types.SelectMany(x => x.NestedTypes.Prepend(x)).SelectMany(x => x.Properties))
            {
                const string getPrefix = "get_";
                const string setPrefix = "set_";

                var getName = propertyDef.GetMethod != null && propertyDef.GetMethod.Name.StartsWith(getPrefix) ? propertyDef.GetMethod.Name.Substring(getPrefix.Length) : null;
                var setName = propertyDef.SetMethod != null && propertyDef.SetMethod.Name.StartsWith(setPrefix) ? propertyDef.SetMethod.Name.Substring(setPrefix.Length) : null;

                if (getName != null && setName != null && getName != setName)
                {
                    throw new Exception($"{propertyDef.FullName} has 2 different accessor names");
                }

                if (getName != null || setName != null)
                {
                    var name = getName ?? setName;

                    MapMember(propertyDef, name);

                    if (propertyDef.GetMethod != null)
                        MapMember(propertyDef.GetMethod, "get_" + name);

                    if (propertyDef.SetMethod != null)
                        MapMember(propertyDef.SetMethod, "set_" + name);
                }
            }

            void MapType(MappedType type, TypeDefinition typeDef)
            {
                if (typeDef == null)
                {
                    throw new NullReferenceException($"Type {type} was not found!");
                }

                if (type.Mapped != null)
                {
                    MapMember(typeDef, type.Mapped);
                }

                foreach (var property in type.Properties)
                {
                    var propertyDef = typeDef.Properties.SingleOrDefault(x => x.Name == property.Original.Name);

                    if (propertyDef == null)
                    {
                        throw new NullReferenceException($"Property {property} was not found in {type}!");
                    }

                    if (property.Mapped != null)
                    {
                        MapMember(propertyDef, property.Mapped);

                        if (propertyDef.GetMethod != null)
                            MapMember(propertyDef.GetMethod, "get_" + property.Mapped);

                        if (propertyDef.SetMethod != null)
                            MapMember(propertyDef.SetMethod, "set_" + property.Mapped);
                    }
                }

                foreach (var field in type.Fields)
                {
                    var fieldDef = typeDef.Fields.SingleOrDefault(x => x.Name == field.Original.Name);

                    if (fieldDef == null)
                    {
                        throw new NullReferenceException($"Field {field} was not found in {type}!");
                    }

                    if (field.Mapped != null)
                    {
                        MapMember(fieldDef, field.Mapped);
                    }
                }

                foreach (var method in type.Methods)
                {
                    var methodDef = typeDef.Methods
                        .SingleOrDefault(x => x.Name == method.Original.Name && (method.Original.Signature == null || x.GetSignature().ToString() == method.Original.Signature));

                    if (methodDef == null)
                    {
                        throw new NullReferenceException($"Method {method} was not found in {type}!");
                    }

                    foreach (var methodDef2 in ModuleDef.Types
                        .Where(x => x.BaseType == typeDef)
                        .Select(x => x.Methods.SingleOrDefault(m => m.Name == method.Original.Name && (method.Original.Signature == null || m.GetSignature().ToString() == method.Original.Signature)))
                        .Where(x => x != null)
                        .Prepend(methodDef)
                    )
                    {
                        if (method.Mapped != null)
                        {
                            MapMember(methodDef2, method.Mapped);
                        }

                        for (var i = 0; i < method.Parameters.Count; i++)
                        {
                            MapMember(methodDef2.Parameters.ElementAt(i), method.Parameters[i]);
                        }
                    }
                }

                foreach (var nested in type.Nested)
                {
                    MapType(nested, typeDef.NestedTypes.SingleOrDefault(x => x.Name == nested.Original.Name));
                }
            }

            foreach (var type in Mappings.Types)
            {
                MapType(type, ModuleDef.GetType(type.Original.Name));
            }

            foreach (var typeDef in ModuleDef.Types)
            {
                var i = 0;

                foreach (var member in typeDef.Properties)
                {
                    if (member.Name.IsObfuscated() && member.CustomAttributes.All(x => x.AttributeType != mappedAttribute))
                    {
                        MapMember(member, $"Property_{i}");
                    }

                    i++;
                }

                i = 0;

                foreach (var member in typeDef.Fields)
                {
                    if (member.Name.IsObfuscated() && member.CustomAttributes.All(x => x.AttributeType != mappedAttribute))
                    {
                        MapMember(member, $"Field_{i}");
                    }

                    i++;
                }

                i = 0;

                foreach (var member in typeDef.Methods)
                {
                    if (member.Name.IsObfuscated() && member.CustomAttributes.All(x => x.AttributeType != mappedAttribute))
                    {
                        MapMember(member, $"Method_{i}");
                    }

                    var j = 0;
                    foreach (var parameter in member.Parameters)
                    {
                        if (parameter.Name.IsObfuscated() && parameter.CustomAttributes.All(x => x.AttributeType != mappedAttribute))
                        {
                            MapMember(parameter, $"Parameter_{j}");
                        }

                        j++;
                    }

                    i++;
                }

                i = 0;

                foreach (var member in typeDef.NestedTypes)
                {
                    if (member.Name.IsObfuscated() && member.CustomAttributes.All(x => x.AttributeType != mappedAttribute))
                    {
                        MapMember(member, $"Nested_{i}");
                    }

                    i++;
                }
            }
        }

        public void LoadMappings(FileInfo file)
        {
            if (!file.Exists)
            {
                throw new FileNotFoundException();
            }

            JsonConvert.DefaultSettings = () => new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };

            Mappings = JsonConvert.DeserializeObject<Mappings>(File.ReadAllText(file.FullName));
        }
    }
}
