using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

namespace NETType
{
    public class AssemblyTypeFilter
    {
        public Assembly Assembly
        {
            get => assembly;
            set => assembly = value ?? throw new ArgumentNullException(nameof(value), "Assembly can't be null");
        }

        public string FilterItemDelimiter
        {
            get => filterItemDelimiter;
            set => filterItemDelimiter = value ?? throw new ArgumentNullException(nameof(value), "Filter item delimiter can't be null");
        }

        public AssemblyTypeFilter()
        {
        }

        public AssemblyTypeFilter(Assembly assembly, string filterItemDelimiter = "|")
        {
            Assembly = assembly;
            FilterItemDelimiter = filterItemDelimiter;
        }

        public IEnumerable<TypeInfo> Filter(string filter)
        {
            if (filter == null)
                throw new ArgumentNullException(nameof(filter), "Filter can't be null");

            string[] typeList = filter.Split(FilterItemDelimiter, StringSplitOptions.RemoveEmptyEntries);

            HashSet<TypeInfo> typeSet = new HashSet<TypeInfo>();

            foreach (var type in typeList)
            {
                switch (type)
                {
                    case "@all":
                        foreach (var item in assembly.DefinedTypes)
                            typeSet.Add(item);
                        break;
                    case "@class":
                    case "@reference":
                        foreach (var item in assembly.DefinedTypes.Where(t => t.IsClass))
                            typeSet.Add(item);
                        break;
                    case "@structure":
                    case "@value":
                        foreach (var item in assembly.DefinedTypes.Where(t => t.IsValueType))
                            typeSet.Add(item);
                        break;
                    case "@@class":
                    case "@@reference":
                        foreach (var item in assembly.DefinedTypes.Where(t => t.IsClass && t.IsAbstract && t.IsSealed))
                            typeSet.Add(item);
                        break;
                    default:
                        TypeInfo result = assembly.GetType(type)?.GetTypeInfo();
                        if (result != null)
                            typeSet.Add(result);
                        break;
                }
            }

            return typeSet.Distinct(new TypeInfoComparerByFullName());
        }

        private Assembly assembly;
        private string filterItemDelimiter = "|";
    }
}