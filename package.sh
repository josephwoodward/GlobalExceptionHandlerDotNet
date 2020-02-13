#!/usr/bin/env bash

rm -rf ./package
dotnet pack ./src/GlobalExceptionHandler/GlobalExceptionHandler.csproj -o ./package/ -c release
