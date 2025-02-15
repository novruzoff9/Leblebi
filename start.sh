#!/bin/bash

# Navigate to the project directory
cd /C:/Users/User/source/repos/Leblebi

# Restore dependencies
dotnet restore

# Build the project
dotnet build

# Run the project
dotnet run