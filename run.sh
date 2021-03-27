#!/usr/bin/env bash

nettype="nettype"

mcs "-out:$nettype.exe" "$nettype.cs"
mono nettype.exe --version
