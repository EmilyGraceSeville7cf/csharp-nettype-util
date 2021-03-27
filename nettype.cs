using System;
using System.Linq;
using System.IO;
using System.Reflection;
using System.Collections.Generic;

namespace NETType
{
    internal class TypeInfoComparerByFullName : IEqualityComparer<TypeInfo>
    {
        public bool Equals(TypeInfo x, TypeInfo y)
        {
            return x.FullName == y.FullName;
        }

        public int GetHashCode(TypeInfo obj)
        {
            return obj.GetHashCode();
        }
    }

    internal class MemberInfoComparerByName : IEqualityComparer<MemberInfo>
    {
        public bool Equals(MemberInfo x, MemberInfo y)
        {
            return x.Name == y.Name;
        }

        public int GetHashCode(MemberInfo obj)
        {
            return obj.GetHashCode();
        }
    }

    internal static class MainClass
    {
        private enum ExitStatus : byte
        {
            Success,
            NoOptionValueProvided,
            UnknownOption,
            IncorrectAssemblySpecified,
            IncorrectTypeSpecified
        }

        private static class Types
        {
            public const string All = "@all";
            public const string Class = "@class";
            public const string Struct = "@struct";

            public const string StaticClass = "@@class";
        }

        private static class Members
        {
            public const string All = "@all";
            public const string Field = "@field";
            public const string Property = "@property";
            public const string Constructor = "@constructor";
            public const string Method = "@method";

            public const string StaticAll = "@@all";
            public const string StaticField = "@@field";
            public const string StaticProperty = "@@property";
            public const string StaticConstructor = "@@constructor";
            public const string StaticMethod = "@@method";
        }

        private static void Main(string[] args)
        {
            if (args.Length == 0)
                Environment.Exit((int)ExitStatus.Success);

            string assemblyName = null;
            string[] typeNameList = null;
            string[] memberNameList = null;

            int i = 0;
            while (i < args.Length)
            {
                int j = i + 1;
                switch (args[i])
                {
                    case "--help":
                    case "-h":
                        Help();
                        Environment.Exit((int)ExitStatus.Success);
                        break;
                    case "--version":
                    case "-v":
                        Version();
                        Environment.Exit((int)ExitStatus.Success);
                        break;
                    case "--assembly":
                    case "-a":
                        if (j < args.Length)
                        {
                            assemblyName = args[j];
                            i += 2;
                        }
                        else
                        {
                            Console.Error.WriteLine("No option value provided.");
                            Environment.Exit((int)ExitStatus.NoOptionValueProvided);
                        }
                        break;
                    case "--types":
                    case "-t":
                        if (j < args.Length)
                        {
                            typeNameList = ProcessTypes(args[j]);
                            i += 2;
                        }
                        else
                        {
                            Console.Error.WriteLine("No option value provided.");
                            Environment.Exit((int)ExitStatus.NoOptionValueProvided);
                        }
                        break;
                    case "--members":
                    case "-m":
                        if (j < args.Length)
                        {
                            memberNameList = ProcessMembers(args[j]);
                            i += 2;
                        }
                        else
                        {
                            Console.Error.WriteLine("No option value provided.");
                            Environment.Exit((int)ExitStatus.NoOptionValueProvided);
                        }
                        break;
                    default:
                        Console.Error.WriteLine("Unknown option used.");
                        Environment.Exit((int)ExitStatus.UnknownOption);
                        break;
                }
            }

            try
            {
                OutputMemberInfo(assemblyName, typeNameList, memberNameList);
            }
            catch (FileNotFoundException)
            {
                Console.Error.WriteLine("Specified assembly isn't found.");
                Environment.Exit((int)ExitStatus.IncorrectAssemblySpecified);
            }
            catch (FileLoadException)
            {
                Console.Error.WriteLine("Specified assembly can't be loaded.");
                Environment.Exit((int)ExitStatus.IncorrectAssemblySpecified);
            }
            catch (ArgumentException)
            {
                Console.Error.WriteLine("Unsupported member type specified.");
                Environment.Exit((int)ExitStatus.IncorrectTypeSpecified);
            }
        }

        private static void Help()
        {
            Console.WriteLine(@"Description:
nettype - program to extract .NET type info from assemblies

Options:
- --help|-h - outputs help and exits
- --version|-b - outputs version and exits
- --assembly|-a - specifies assembly name
- --types|-t - specifies type names
  - @all - specifies all types in assembly
  - @class - specifies reference types in assembly
  - @struct - specifies value types in assembly
  - @@class - specifies static reference types in assembly
- --members|-m - specifies member names
  - @all - specifies all members in types
  - @field - specifies all fields in types
  - @property - specifies all properties in types
  - @constructor - specifies all constructors in types
  - @method - specifies all methods in types
  - @@all - specifies all static members in types
  - @@field - specifies all static fields in types
  - @@property - specifies all static properties in types
  - @@constructor - specifies static all constructors in types
  - @@method - specifies static all methods in types

Output format:
<type-name>:<member-name>:class=<member-class>
<type-name>:<member-name>:return-type=<member-return-type>
[<type-name>:<member-name>:arguments=<arg1,arg1-type>;..;<argn,argn-type>]
...

- <member-class> is one of method|property|field.
- <member-return-type> is member type if it is field or property else method return type.
- <arg1,arg1-type>;..;<argn,argn-type> is argument list if member is method.

Examples:
- nettype --help
- nettype --assembly My.dll --types 'SomeNamespace.A;SomeNamespace.B' --members 'SampleMethod'
- nettype --assembly My.dll --types '@class' --members '@field|@property'");
        }

        private static void Version()
        {
            Console.WriteLine("2021 (c) Alvin Seville");
        }

        private static string[] ProcessTypes(string typeNameList)
        {
            if (typeNameList == null)
                throw new ArgumentNullException(nameof(typeNameList), "type list can't be null");

            return typeNameList.Split(';', StringSplitOptions.RemoveEmptyEntries);
        }

        private static string[] ProcessMembers(string memberNameList)
        {
            if (memberNameList == null)
                throw new ArgumentNullException(nameof(memberNameList), "member list can't be null");

            return memberNameList.Split(';', StringSplitOptions.RemoveEmptyEntries);
        }

        private static void OutputMemberInfo(string assemblyName, string[] typeNameList, string[] memberNameList)
        {
            if (assemblyName == null)
                throw new ArgumentNullException(nameof(assemblyName), "assembly name can't be null");
            if (typeNameList == null)
                throw new ArgumentNullException(nameof(typeNameList), "type list can't be null");
            if (memberNameList == null)
                throw new ArgumentNullException(nameof(memberNameList), "member list can't be null");

            Assembly assembly = Assembly.LoadFile(assemblyName);
            TypeInfo[] typeList = FilterTypes(assembly, typeNameList);

            foreach (var type in typeList)
            {
                foreach (var member in FilterMembers(type, memberNameList))
                {
                    string memberClass = ToMemberClass(member);
                    string memberReturnType = GetReturnType(member);

                    Console.WriteLine($"{type}:{member.Name}:class={memberClass}");
                    Console.WriteLine($"{type}:{member.Name}:return-type={memberReturnType}");

                    switch (member.MemberType)
                    {
                        case MemberTypes.Method:
                            Console.WriteLine($"{type}:{member.Name}:arguments={string.Join(';', ((MethodInfo)member).GetParameters().Select(p => $"{p.Name},{p.ParameterType.FullName}"))}");
                            break;
                        case MemberTypes.Constructor:
                            Console.WriteLine($"{type}:{member.Name}:arguments={string.Join(';', ((ConstructorInfo)member).GetParameters().Select(p => $"{p.Name},{p.ParameterType.FullName}"))}");
                            break;
                    }
                }
            }
        }

        private static TypeInfo[] FilterTypes(Assembly assembly, string[] typeNameList)
        {
            if (assembly == null)
                throw new ArgumentNullException(nameof(assembly), "assembly can't be null");
            if (typeNameList == null)
                throw new ArgumentNullException(nameof(typeNameList), "type list can't be null");

            HashSet<TypeInfo> typeList = new HashSet<TypeInfo>();

            foreach (var type in typeNameList)
            {
                switch (type)
                {
                    case Types.All:
                        foreach (var item in assembly.DefinedTypes)
                            typeList.Add(item);
                        break;
                    case Types.Class:
                        foreach (var item in assembly.DefinedTypes.Where(t => t.IsClass))
                            typeList.Add(item);
                        break;
                    case Types.Struct:
                        foreach (var item in assembly.DefinedTypes.Where(t => t.IsValueType))
                            typeList.Add(item);
                        break;
                    case Types.StaticClass:
                        foreach (var item in assembly.DefinedTypes.Where(t => t.IsClass && t.IsAbstract && t.IsSealed))
                            typeList.Add(item);
                        break;
                    default:
                        TypeInfo result = assembly.GetType(type)?.GetTypeInfo();
                        if (result != null)
                            typeList.Add(result);
                        break;
                }
            }

            return typeList.Distinct(new TypeInfoComparerByFullName()).ToArray();
        }

        private static MemberInfo[] FilterMembers(TypeInfo type, string[] memberNameList)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type), "type can't be null");
            if (memberNameList == null)
                throw new ArgumentNullException(nameof(memberNameList), "type list can't be null");

            HashSet<MemberInfo> memberList = new HashSet<MemberInfo>();

            foreach (var member in memberNameList)
            {
                switch (member)
                {
                    case Members.All:
                        foreach (var item in type.DeclaredMembers)
                            memberList.Add(item);
                        break;
                    case Members.Field:
                        foreach (var item in type.DeclaredFields)
                            memberList.Add(item);
                        break;
                    case Members.Property:
                        foreach (var item in type.DeclaredProperties)
                            memberList.Add(item);
                        break;
                    case Members.Constructor:
                        foreach (var item in type.DeclaredConstructors)
                            memberList.Add(item);
                        break;
                    case Members.Method:
                        foreach (var item in type.DeclaredMethods)
                            memberList.Add(item);
                        break;
                    case Members.StaticAll:
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
                                    throw new ArgumentException(nameof(memberNameList), "Unsupported member type.");
                            }
                            }))
                            memberList.Add(item);
                        break;
                    case Members.StaticField:
                        foreach (var item in type.DeclaredFields.Where(m => m.IsStatic))
                            memberList.Add(item);
                        break;
                    case Members.StaticProperty:
                        foreach (var item in type.DeclaredProperties.Where(m => m.GetMethod == null ? m.SetMethod.IsStatic : m.GetMethod.IsStatic))
                            memberList.Add(item);
                        break;
                    case Members.StaticConstructor:
                        foreach (var item in type.DeclaredConstructors.Where(m => m.IsStatic))
                            memberList.Add(item);
                        break;
                    case Members.StaticMethod:
                        foreach (var item in type.DeclaredMethods.Where(m => m.IsStatic))
                            memberList.Add(item);
                        break;
                    default:
                        MemberInfo result = type.GetMember(member, BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static).FirstOrDefault();
                        if (result != null)
                            memberList.Add(result);
                        break;
                }
            }

            return memberList.Distinct(new MemberInfoComparerByName()).ToArray();
        }

        private static string ToMemberClass(MemberInfo type)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type), "type can't be null");

            string memberClass = string.Empty;
            switch (type.MemberType)
            {
                case MemberTypes.Field:
                    memberClass = "field";
                    break;
                case MemberTypes.Property:
                    memberClass = "property";
                    break;
                case MemberTypes.Constructor:
                    memberClass = "constructor";
                    break;
                case MemberTypes.Method:
                    memberClass = "method";
                    break;
                default:
                    throw new ArgumentException("Unsupported member type.", nameof(type));
            }

            return memberClass;
        }

        private static string GetReturnType(MemberInfo type)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type), "type can't be null");

            string memberReturnType = string.Empty;
            switch (type.MemberType)
            {
                case MemberTypes.Field:
                    memberReturnType = ((FieldInfo)type).FieldType.FullName;
                    break;
                case MemberTypes.Property:
                    memberReturnType = ((PropertyInfo)type).PropertyType.FullName;
                    break;
                case MemberTypes.Constructor:
                    memberReturnType = ((ConstructorInfo)type).DeclaringType.FullName;
                    break;
                case MemberTypes.Method:
                    memberReturnType = ((MethodInfo)type).ReturnType.FullName;
                    break;
                default:
                    throw new ArgumentException("Unsupported member type.", nameof(type));
            }

            return memberReturnType;
        }
    }
}