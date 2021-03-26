#!/usr/bin/env bash

mcs -out:nettype.exe nettype.cs
mcs -target:library -out:test_library.dll test_library.cs
mono nettype.exe --assembly "$PWD/test_library.dll" --types "First.A;First.B" --members "x;y"