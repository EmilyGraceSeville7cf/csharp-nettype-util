using System.Reflection;
using System.Collections.Generic;

namespace NETType
{
    internal class MemberInfoComparerByName : IEqualityComparer<MemberInfo>
    {
        public bool Equals(MemberInfo first, MemberInfo second)
        {
            return first.Name == second.Name;
        }

        public int GetHashCode(MemberInfo obj)
        {
            return obj.GetHashCode();
        }
    }
}
