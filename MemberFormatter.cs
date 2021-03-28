using System;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Collections.Generic;

namespace NETType
{
    public class MemberFormatter
    {
        public IDictionary<TypeInfo, IEnumerable<MemberInfo>> Map
        {
            get => map;
            set => map = value ?? throw new ArgumentNullException(nameof(value), "Map can't be null");
        }

        public MemberFormatter()
        {
        }

        public MemberFormatter(IDictionary<TypeInfo, IEnumerable<MemberInfo>> map)
        {
            Map = map;
        }

        public override string ToString()
        {
            StringBuilder result = new StringBuilder();

            foreach (var type in map.Keys)
            {
                foreach (var member in map[type])
                {
                    string prefix = $"{type.FullName}:{member.Name}";
                    result.AppendLine($"{prefix}:is={MemberTypeToString(member)}");
                    result.AppendLine($"{prefix}:return={MemberReturnTypeToString(member)}");

                    switch (member.MemberType)
                    {
                        case MemberTypes.Method:
                            result.AppendLine($"{prefix}:arguments={string.Join(';', ((MethodInfo)member).GetParameters().Select(p => $"{p.Name},{p.ParameterType.FullName}"))}");
                            break;
                        case MemberTypes.Constructor:
                            result.AppendLine($"{prefix}:arguments={string.Join(';', ((ConstructorInfo)member).GetParameters().Select(p => $"{p.Name},{p.ParameterType.FullName}"))}");
                            break;
                    }
                }
            }

            return result.ToString();
        }

        private string MemberTypeToString(MemberInfo member)
        {
            if (member == null)
                throw new ArgumentNullException(nameof(member), "Member can't be null");

            string memberType = string.Empty;
            switch (member.MemberType)
            {
                case MemberTypes.Field:
                    memberType = "field";
                    break;
                case MemberTypes.Property:
                    memberType = "property";
                    break;
                case MemberTypes.Constructor:
                    memberType = "constructor";
                    break;
                case MemberTypes.Method:
                    memberType = "method";
                    break;
                default:
                    throw new ArgumentException(nameof(member), "Unsupported member type.");
            }

            return memberType;
        }

        private static string MemberReturnTypeToString(MemberInfo member)
        {
            if (member == null)
                throw new ArgumentNullException(nameof(member), "Member can't be null");

            string memberReturnType = string.Empty;
            switch (member.MemberType)
            {
                case MemberTypes.Field:
                    memberReturnType = ((FieldInfo)member).FieldType.FullName;
                    break;
                case MemberTypes.Property:
                    memberReturnType = ((PropertyInfo)member).PropertyType.FullName;
                    break;
                case MemberTypes.Constructor:
                    memberReturnType = ((ConstructorInfo)member).DeclaringType.FullName;
                    break;
                case MemberTypes.Method:
                    memberReturnType = ((MethodInfo)member).ReturnType.FullName;
                    break;
                default:
                    throw new ArgumentException(nameof(member), "Unsupported member type.");
            }

            return memberReturnType;
        }

        private IDictionary<TypeInfo, IEnumerable<MemberInfo>> map;
    }
}
