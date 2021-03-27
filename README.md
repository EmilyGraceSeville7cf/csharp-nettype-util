[![CI](https://github.com/alvinseville7cf/CSharp---Exercise---Other---Terminal-reflection-client/actions/workflows/ci.yml/badge.svg)](https://github.com/alvinseville7cf/CSharp---Exercise---Other---Terminal-reflection-client/actions/workflows/ci.yml)

# nettype

## Description

nettype - program to extract .NET type info from assemblies

## Options

- `--help`|`-h` - outputs help and exits
- `--version`|`-b` - outputs version and exits
- `--assembly`|`-a` - specifies assembly name
- `--types`|`-t` - specifies type names
  - `@all` - specifies all types in assembly
  - `@class` - specifies reference types in assembly
  - `@struct` - specifies value types in assembly
  - `@@class` - specifies static reference types in assembly
- `--members`|`-m` - specifies member names
  - `@all` - specifies all members in types
  - `@field` - specifies all fields in types
  - `@property` - specifies all properties in types
  - `@constructor` - specifies all constructors in types
  - `@method` - specifies all methods in types
  - `@@all` - specifies all static members in types
  - `@@field` - specifies all static fields in types
  - `@@property` - specifies all static properties in types
  - `@@constructor` - specifies static all constructors in types
  - `@@method` - specifies static all methods in types

Output format:
```
<type-name>:<member-name>:class=<member-class>
<type-name>:<member-name>:return-type=<member-return-type>
[<type-name>:<member-name>:arguments=<arg1,arg1-type>;..;<argn,argn-type>]
...
```

- `<member-class>` is one of method|property|field.
- `<member-return-type>` is member type if it is field or property else method return type.
- `<arg1,arg1-type>;..;<argn,argn-type>` is argument list if member is method.

## Examples

- `nettype --help` - outputs help and exits
- `nettype --assembly My.dll --types 'SomeNamespace.A;SomeNamespace.B' --members 'SampleMethod'` - prints all SomeNamespace.A SomeNamespace.B type members in My.dll
- `nettype --assembly My.dll --types '@class' --members '@field|@property'` - prints all fields and properties in all classes in My.dll

[![Try on repl.it](https://repl-badge.jajoosam.repl.co/try.png)](https://repl.it/@AlvinSeville7cf/CSharp-Exercise-Other-Terminal-reflection-client?ref=button)
