using System.Linq;
using Mono.Cecil;

namespace Reactor.Greenhouse
{
    public static class Extensions
    {
        public static CustomAttribute GetCustomAttribute(this ICustomAttributeProvider cap, string attribute)
        {
            if (!cap.HasCustomAttributes)
            {
                return null;
            }

            return cap.CustomAttributes.FirstOrDefault(attrib => attrib.AttributeType.FullName == attribute);
        }

        public static uint? GetOffset(this MethodDefinition methodDef)
        {
            var attribute = methodDef.GetCustomAttribute("Il2CppDummyDll.AddressAttribute");
            if (attribute == null)
                return null;

            var offset = attribute.Fields.Single(x => x.Name == "Offset");
            return new System.ComponentModel.UInt32Converter().ConvertFrom(offset) as uint?;
        }
    }
}
