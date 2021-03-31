#!/usr/bin/env bash

function installCsCompiler()
{
  if mono --version
  then
    return
  fi
  
  case "$OSTYPE" in
    darwin*)
      brew install mono
      ;;
    msys)
      choco install mono -y
      ;;
  esac
}

installCsCompiler

shopt -s extglob
mcs -out:nettype.exe !(*sample).cs
