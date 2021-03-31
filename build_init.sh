#!/usr/bin/env bash

function installCsCompiler()
{
  if mono --version &> /dev/null
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
