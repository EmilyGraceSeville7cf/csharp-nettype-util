using System;
using System.Linq;
using System.IO;
using System.Reflection;
using System.Collections.Generic;

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
            string typeFilter = string.Empty;
            string memberFilter = string.Empty;

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
                            typeFilter = args[j];
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
                            memberFilter = args[j];
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
                AssemblyTypeFilter assemblyTypeFilter = new AssemblyTypeFilter(Assembly.LoadFile(assemblyName));
                IEnumerable<TypeInfo> types = assemblyTypeFilter.Filter(typeFilter);
                
                AssemblyMemberFilter assemblyMemberFilter = new AssemblyMemberFilter(types);
                IDictionary<TypeInfo, IEnumerable<MemberInfo>> members = assemblyMemberFilter.Filter(memberFilter);
                
                MemberFormatter formatter = new MemberFormatter(members);
                Console.WriteLine(formatter.ToString());
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
<type-name>:<member-name>:is=<member-class>
<type-name>:<member-name>:return=<member-return-type>
[<type-name>:<member-name>:arguments=<arg1,arg1-type>;..;<argn,argn-type>]
...

- <member-class> is one of method|property|field.
- <member-return-type> is member type if it is field or property else method return type.
- <arg1,arg1-type>;..;<argn,argn-type> is argument list if member is method.

Examples:
- nettype --help
- nettype --assembly My.dll --types 'SomeNamespace.A|SomeNamespace.B' --members 'SampleMethod'
- nettype --assembly My.dll --types '@class' --members '@field|@property'");
        }

        private static void Version()
        {
            Console.WriteLine("2021 (c) Alvin Seville");
        }
    }
}
