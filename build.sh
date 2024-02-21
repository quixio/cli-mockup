#!/bin/bash

# build the code for specific targets
dotnet publish -r win10-x64 --self-contained true -o QuixCliWrapper/binaries/win10
dotnet publish -r linux-x64 --self-contained true -o QuixCliWrapper/binaries/linux
dotnet publish -r osx-x64 --self-contained true -o QuixCliWrapper/binaries/osx
dotnet publish -r osx-arm64 --self-contained true -o QuixCliWrapper/binaries/osx-arm

# build the package
python3 setup.py sdist
python3 setup.py bdist_wheel

# copy the .whl files
cp /Code/Quix/cs-cli/dist/*.whl /code/tests