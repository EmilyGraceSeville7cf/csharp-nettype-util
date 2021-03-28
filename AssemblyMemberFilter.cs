using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

namespace NETType
{
    public class AssemblyMemberFilter
    {
        public IEnumerable<TypeInfo> Types
        {
            get => types;
            set => types = value ?? throw new ArgumentNullException(nameof(value), "Type list can't be null");
        }

        public string FilterItemDelimiter
        {
            get => filterItemDelimiter;
            set => filterItemDelimiter = value ?? throw new ArgumentNullException(nameof(value), "Filter item delimiter can't be null");
        }

        public AssemblyMemberFilter()
        {
        }
        
        public AssemblyMemberFilter(IEnumerable<TypeInfo> types, string filterItemDelimiter = "|")
        {
            Types = types;
            FilterItemDelimiter = filterItemDelimiter;
        }

        public IDictionary<TypeInfo, IEnumerable<MemberInfo>> Filter(string filter)
        {
            if (filter == null)
                throw new ArgumentNullException(nameof(filter), "Filter can't be null");

            IDictionary<TypeInfo, IEnumerable<MemberInfo>> map = new Dictionary<TypeInfo, IEnumerable<MemberInfo>>();

            foreach (var type in Types)
            {
                IEnumerable<MemberInfo> members = FilterForConcreteTypeInfo(type, filter);
                if (members.Any())
                    map.Add(type, FilterForConcreteTypeInfo(type, filter));
            }
            
            return map;
        }

        private IEnumerable<MemberInfo> FilterForConcreteTypeInfo(TypeInfo type, string filter)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(filter), "Type can't be null");
            if (filter == null)
                throw new ArgumentNullException(nameof(filter), "Filter can't be null");

            string[] memberList = filter.Split(FilterItemDelimiter, StringSplitOptions.RemoveEmptyEntries);

            HashSet<MemberInfo> memberSet = new HashSet<MemberInfo>();

            foreach (var member in memberList)
            {
                switch (member)
                {
                    case "@all":
                        foreach (var item in type.DeclaredMembers)
                            memberSet.Add(item);
                        break;
                    case "@field":
                        foreach (var item in type.DeclaredFields)
                            memberSet.Add(item);
                        break;
                    case "@property":
                        foreach (var item in type.DeclaredProperties)
                            memberSet.Add(item);
                        break;
                    case "@constructor":
                        foreach (var item in type.DeclaredConstructors)
                            memberSet.Add(item);
                        break;
                    case "@method":
                        foreach (var item in type.DeclaredMethods)
                            memberSet.Add(item);
                        break;
                    case "@@all":
                        foreach (var item in type.DeclaredMembers.Where(m => {
                            switch (m.MemberType)
                            {
                                case MemberTypes.Field:
                                    return ((ConstructorInfo)m).IsStatic;
                                case MemberTypes.Property:
                                    PropertyInfo property = (PropertyInfo)m;
                                    return property.GetMethod == null ? property.SetMethod.IsStatic : property.GetMethod.IsStatic;
                                case MemberTypes.Constructor:
                                    return ((ConstructorInfo)m).IsStatic;
                                case MemberTypes.Method:
                                    return ((MethodInfo)m).IsStatic;
                                default:
                                    return false;
                            }
                            }))
                            memberSet.Add(item);
                        break;
                    case "@@field":
                        foreach (var item in type.DeclaredFields.Where(m => m.IsStatic))
                            memberSet.Add(item);
                        break;
                    case "@@property":
                        foreach (var item in type.DeclaredProperties.Where(m => m.GetMethod == null ? m.SetMethod.IsStatic : m.GetMethod.IsStatic))
                            memberSet.Add(item);
                        break;
                    case "@@constructor":
                        foreach (var item in type.DeclaredConstructors.Where(m => m.IsStatic))
                            memberSet.Add(item);
                        break;
                    case "@@method":
                        foreach (var item in type.DeclaredMethods.Where(m => m.IsStatic))
                            memberSet.Add(item);
                        break;
                    default:
                        MemberInfo result = type.GetMember(member, BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static).FirstOrDefault();
                        if (result != null)
                            memberSet.Add(result);
                        break;
                }
            }

            return memberSet.Distinct(new MemberInfoComparerByName());
        }

        private IEnumerable<TypeInfo> types;
        private string filterItemDelimiter = "|";
    }
}
