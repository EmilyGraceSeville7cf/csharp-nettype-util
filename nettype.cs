using System;
using System.Linq;
using System.IO;
using System.Reflection;

namespace NETType
{
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
- --members|-m - specifies member names
  - @all - specifies all members in types
  - @field - specifies all fields in types
  - @property - specifies all properties in types
  - @constructor - specifies all constructors in types
  - @method - specifies all methods in types

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
- nettype --assembly My.dll --types SomeNamespace.A;SomeNamespace.B --members SampleMethod");
        }

        private static void Version()
        {
            Console.WriteLine("2021 (c) Alvin Seville");
        }

        private static string[] ProcessTypes(string typeNameList)
        {
            if (typeNameList == null)
                throw new ArgumentNullException(nameof(typeNameList), "type list can't be null");

            string[] result = typeNameList.Split(';', StringSplitOptions.RemoveEmptyEntries);
            foreach (var item in new string[] { "@all", "@class", "@struct" })
                result = result.Contains(item) ? new string[] { item } : result;
            return result;
        }

        private static string[] ProcessMembers(string memberNameList)
        {
            if (memberNameList == null)
                throw new ArgumentNullException(nameof(memberNameList), "member list can't be null");

            string[] result = memberNameList.Split(';', StringSplitOptions.RemoveEmptyEntries);
            foreach (var item in new string[] { "@all", "@field", "@property", "@constructor", "@method" })
                result = result.Contains(item) ? new string[] { item } : result;
            return result;
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

            TypeInfo[] typeList = assembly.DefinedTypes.ToArray();

            if (typeNameList.Length == 1)
            {
                switch (typeNameList[0])
                {
                    case "@class":
                        typeList = typeList.Where(t => !t.IsValueType).ToArray();
                        break;
                    case "@struct":
                        typeList = typeList.Where(t => t.IsValueType).ToArray();
                        break;
                }
            }
            else
                typeList = typeList.Where(t => typeNameList.Contains(t.FullName)).ToArray();

            return typeList;
        }

        private static MemberInfo[] FilterMembers(TypeInfo type, string[] memberNameList)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type), "type can't be null");
            if (memberNameList == null)
                throw new ArgumentNullException(nameof(memberNameList), "type list can't be null");

            MemberInfo[] memberList = type.DeclaredMembers.ToArray();

            if (memberNameList.Length == 1)
            {
                switch (memberNameList[0])
                {
                    case "@field":
                        memberList = memberList.Where(m => m.MemberType == MemberTypes.Field).ToArray();
                        break;
                    case "@property":
                        memberList = memberList.Where(m => m.MemberType == MemberTypes.Property).ToArray();
                        break;
                    case "@constructor":
                        memberList = memberList.Where(m => m.MemberType == MemberTypes.Constructor).ToArray();
                        break;
                    case "@method":
                        memberList = memberList.Where(m => m.MemberType == MemberTypes.Method).ToArray();
                        break;
                }
            }
            else
                memberList = memberList.Where(m => memberNameList.Contains(m.Name)).ToArray();

            return memberList;
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
