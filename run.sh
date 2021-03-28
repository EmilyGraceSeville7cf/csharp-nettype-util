#!/usr/bin/env bash

mcs -out:nettype.exe nettype.cs TypeInfoComparerByFullName.cs MemberInfoComparerByName.cs AssemblyTypeFilter.cs AssemblyMemberFilter.cs MemberFormatter.cs
mono nettype.exe --version
