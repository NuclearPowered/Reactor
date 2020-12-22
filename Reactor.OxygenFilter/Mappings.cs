using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace Reactor.OxygenFilter
{
    public class Mappings
    {
        public List<MappedType> Types { get; set; } = new List<MappedType>();

        public MappedType Find(string name, Func<MappedType, string> predicate)
        {
            var split = name.Split('.').SelectMany(x => x.Split('+')).ToArray();

            if (split.Length > 1)
            {
                MappedType type = null;

                foreach (var s in split)
                {
                    if (type == null)
                    {
                        type = Find(s, predicate);
                    }
                    else
                    {
                        type = type.Nested.SingleOrDefault(x => predicate(x) == s);
                    }
                }

                return type;
            }

            return Types.SingleOrDefault(x => predicate(x) == name);
        }

        public MappedType FindByMapped(string name)
        {
            return Find(name, x => x.Mapped);
        }

        public MappedType FindByOriginal(string name)
        {
            return Find(name, x => x.Original.Name);
        }
    }

    public class OriginalDescriptor
    {
        public string Name { get; set; }
        public int? Index { get; set; }
        public string Signature { get; set; }
        public Constant Const { get; set; }

        public bool Equals(OriginalDescriptor obj)
        {
            return Name == obj.Name && Index == obj.Index && Signature == obj.Signature && Const == obj.Const;
        }

        public bool IsEmpty()
        {
            return Name == null && Index == null;
        }

        public override string ToString()
        {
            return Name ?? Index?.ToString();
        }

        public class Constant
        {
            private object _value;

            public object Value
            {
                get => Convert.ChangeType(_value, Type);
                set => _value = value;
            }

            public Type Type { get; set; }
        }
    }

    public class OriginalDescriptorConverter : JsonConverter<OriginalDescriptor>
    {
        public override void WriteJson(JsonWriter writer, OriginalDescriptor value, JsonSerializer serializer)
        {
            if (value.Name != null && value.Index == null && value.Signature == null)
            {
                writer.WriteValue(value.Name);
            }
            else if (value.Index != null && value.Name == null && value.Signature == null)
            {
                writer.WriteValue(value.Index);
            }
            else
            {
                serializer.Serialize(writer, value);
            }
        }

        public override OriginalDescriptor ReadJson(JsonReader reader, Type objectType, OriginalDescriptor existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if (reader.Value is string name)
            {
                return new OriginalDescriptor { Name = name };
            }

            if (reader.Value is long index)
            {
                return new OriginalDescriptor { Index = (int?) index };
            }

            if (reader.TokenType == JsonToken.StartObject)
            {
                return serializer.Deserialize<OriginalDescriptor>(reader);
            }

            throw new JsonSerializationException();
        }
    }

    public class MappedMember
    {
        [JsonProperty(Order = -3)]
        [JsonConverter(typeof(OriginalDescriptorConverter))]
        public OriginalDescriptor Original { get; set; }

        [JsonProperty(Order = -2)]
        public string Mapped { get; set; }

        public override string ToString()
        {
            var result = Original.ToString();

            if (Mapped != null)
            {
                result += $" (name: {Mapped})";
            }

            return result;
        }

        public MappedMember()
        {
        }

        public MappedMember(OriginalDescriptor original, string mapped)
        {
            Original = original;

            if (original.Name != mapped)
            {
                Mapped = mapped;
            }
        }

        public bool Equals(MappedMember obj, bool compareOriginal = true)
        {
            if (obj == null)
            {
                return false;
            }

            return Mapped == obj.Mapped && (!compareOriginal || (Original.Name == "^" && obj.Mapped != null) || Original.Equals(obj.Original));
        }
    }

    public class MappedMethod : MappedMember
    {
        public List<string> Parameters { get; set; } = new List<string>();

        public MappedMethod()
        {
        }

        public MappedMethod(OriginalDescriptor original, string mapped) : base(original, mapped)
        {
        }
    }

    public class MappedType : MappedMember
    {
        public List<MappedMember> Properties { get; set; } = new List<MappedMember>();
        public List<MappedMember> Fields { get; set; } = new List<MappedMember>();
        public List<MappedMethod> Methods { get; set; } = new List<MappedMethod>();

        public List<MappedType> Nested { get; set; } = new List<MappedType>();

        public MappedType()
        {
        }

        public MappedType(OriginalDescriptor original, string mapped) : base(original, mapped)
        {
        }
    }
}
