#!/bin/bash
dotnet tool restore
dotnet cake "$@"
exit 0
