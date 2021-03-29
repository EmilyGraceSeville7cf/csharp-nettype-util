#!/usr/bin/env bash

sudo npm install -g markdownlint-cli

for file in *.md
do
    markdownlint "$file"
done