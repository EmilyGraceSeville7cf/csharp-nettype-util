using System;
using System.Linq;
using System.IO;
using System.Reflection;

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
    string[] typeList = null;
    string[] memberList = null;

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
            typeList = args[j].Split(';', StringSplitOptions.RemoveEmptyEntries);
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
            memberList = args[j].Split(';', StringSplitOptions.RemoveEmptyEntries);
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
      OutputMemberInfo(assemblyName, typeList, memberList);
    }
    catch (FileNotFoundException e)
    {
      Console.WriteLine(e.StackTrace);
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
- --members|-m - specifies member names

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

  private static void OutputMemberInfo(string assemblyName, string[] typeList, string[] memberList)
  {
    if (assemblyName == null)
      throw new ArgumentNullException("assembly name can't be null", nameof(assemblyName));
    if (typeList == null)
      throw new ArgumentNullException("type list can't be null", nameof(typeList));
    if (memberList == null)
      throw new ArgumentNullException("member list can't be null", nameof(memberList));

    Assembly assembly = Assembly.LoadFile(assemblyName);
    foreach (var type in assembly.DefinedTypes.Where(t => typeList.Contains(t.FullName)))
    {
      foreach (var member in type.DeclaredMembers.Where(m => memberList.Contains(m.Name)))
      {
        string memberClass = string.Empty;
        switch (member.MemberType)
        {
          case MemberTypes.Field:
            memberClass = "field";
            break;
          case MemberTypes.Property:
            memberClass = "property";
            break;
          case MemberTypes.Method:
            memberClass = "method";
            break;
          default:
            throw new ArgumentException("Unsupported member type.", nameof(memberList));
        }

        string memberReturnType = string.Empty;
        switch (member.MemberType)
        {
          case MemberTypes.Field:
            memberReturnType = ((FieldInfo)member).FieldType.FullName;
            break;
          case MemberTypes.Property:
            memberReturnType = ((PropertyInfo)member).PropertyType.FullName;
            break;
          case MemberTypes.Method:
            memberReturnType = ((MethodInfo)member).ReturnType.FullName;
            break;
          default:
            throw new ArgumentException("Unsupported member type.", nameof(memberList));
        }

        Console.WriteLine($"{type}:{member.Name}:class={memberClass}");
        Console.WriteLine($"{type}:{member.Name}:return-type={memberReturnType}");

        switch (member.MemberType)
        {
          case MemberTypes.Method:
            Console.WriteLine($"{type}:{member.Name}:arguments={string.Join(';', ((MethodInfo)member).GetParameters().Select(p => $"{p.Name},{p.ParameterType.FullName}"))}");
            break;
        }
      }
    }
  }
}