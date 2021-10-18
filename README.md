# nettype

[![CI](https://github.com/emilyseville7cfg/mono-inspect-util/actions/workflows/ci.yml/badge.svg)](https://github.com/emilyseville7cfg/mono-inspect-util/actions/workflows/ci.yml) [![GitHub issues](https://img.shields.io/github/issues/emilyseville7cfg/mono-inspect-util.svg)](https://github.com/emilyseville7cfg/mono-inspect-util/issues) [![GitHub issues](https://img.shields.io/github/issues-closed/emilyseville7cfg/mono-inspect-util.svg)](https://github.com/emilyseville7cfg/mono-inspect-util/issues?q=is%3Aissue+is%3Aclosed) [![GitHub release](https://img.shields.io/github/release/emilyseville7cfg/mono-inspect-util.svg)](https://GitHub.com/emilyseville7cfg/mono-inspect-util/releases/) [![GitHub license](https://img.shields.io/github/license/emilyseville7cfg/mono-inspect-util.svg)](https://github.com/emilyseville7cfg/mono-inspect-util/blob/master/LICENSE)

## Description

> ⚠️ This project is no longer maintained.

Tool to inspect .NET assemblies.

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
  - `@@constructor` - specifies all static constructors in types
  - `@@method` - specifies all static methods in types

Output format:

```
<type-name>:<member-name>:is=<member-class>
<type-name>:<member-name>:return=<member-return-type>
[<type-name>:<member-name>:arguments=<arg1,arg1-type>;..;<argn,argn-type>]
...
```

- `<member-class>` is one of method|property|field.
- `<member-return-type>` is member type if it is field or property else method return type.
- `<arg1,arg1-type>;..;<argn,argn-type>` is argument list if member is method.

## Examples

- `nettype --help` - outputs help and exits
- `nettype --assembly My.dll --types 'SomeNamespace.A|SomeNamespace.B' --members 'SampleMethod'` - prints all SomeNamespace.A SomeNamespace.B type members in My.dll
- `nettype --assembly My.dll --types '@class' --members '@field|@property'` - prints all fields and properties in all classes in My.dll

## Implementation details

- `run.sh` - Bash script to compile and run project (uses `build.sh`)
- `build.sh` - Bash script to compile project (is used by `run.sh` and GitHub actions)
- `repl.it` - Repl.it config to compile and run project (uses `run.sh`)
