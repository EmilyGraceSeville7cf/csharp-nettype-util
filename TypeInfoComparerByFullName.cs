using System.Reflection;
using System.Collections.Generic;

namespace NETType
{
    internal class TypeInfoComparerByFullName : IEqualityComparer<TypeInfo>
    {
        public bool Equals(TypeInfo first, TypeInfo second)
        {
            return first.FullName == second.FullName;
        }

        public int GetHashCode(TypeInfo obj)
        {
            return obj.GetHashCode();
        }
    }
}
