#!/usr/bin/env bash

[[ "$OSTYPE" == "msys" ]] && PATH="/c/Program Files/Mono/bin/mono:$PATH"

shopt -s extglob
mcs -out:nettype.exe !(*sample).cs
