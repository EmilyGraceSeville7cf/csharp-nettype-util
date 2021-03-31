#!/usr/bin/env bash

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

checkMd
checkSh
