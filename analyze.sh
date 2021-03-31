#!/usr/bin/env bash

function installMdLinter()
{
  if markdownlint --version &> /dev/null
  then
    return
  fi

  sudo npm install -g markdownlint-cli
}

function installShLinter()
{
  if shellcheck --version &> /dev/null
  then
    return
  fi

  case "$OSTYPE" in
    linux-gnu*)
      sudo apt-get install shellcheck
      ;;
    darwin*)
      brew install shellcheck
      ;;
    msys)
      choco install shellcheck -y
      ;;
  esac
}

function checkMd()
{
  for file in *.md
  do
      markdownlint "$file"
  done
}

function checkSh()
{
  for file in *.sh
  do
      shellcheck "$file"
  done
}

installMdLinter
installShLinter
checkMd
checkSh
