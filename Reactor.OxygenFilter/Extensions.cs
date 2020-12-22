using System.Linq;
using System.Text;
using Mono.Cecil;

namespace Reactor.OxygenFilter
{
    public static class Extensions
    {
        public static bool IsObfuscated(this string text)
        {
            return text.Length == 11 && text.All(char.IsUpper);
        }

        public static string GetSignature(this MethodDefinition methodDefinition)
        {
            var sb = new StringBuilder();

            sb.Append(methodDefinition.ReturnType.FullName);
            sb.Append(" ");

            sb.Append("(");
            if (methodDefinition.HasParameters)
            {
                for (var i = 0; i < methodDefinition.Parameters.Count; i++)
                {
                    var parameterType = methodDefinition.Parameters[i].ParameterType;

                    if (i > 0)
                        sb.Append(",");

                    if (parameterType is SentinelType)
                        sb.Append("...,");

                    sb.Append(parameterType.FullName);
                }
            }

            sb.Append(")");

            return sb.ToString();
        }

        public static string GetSignature(this FieldDefinition fieldDefinition)
        {
            return fieldDefinition.FieldType.FullName;
        }

        public static string GetSignature(this PropertyDefinition propertyDefinition)
        {
            return propertyDefinition.PropertyType.FullName;
        }
    }
}
