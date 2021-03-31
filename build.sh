#!/usr/bin/env bash

shopt -s extglob
mcs -out:nettype.exe !(*sample).cs
