rem build the code for specific targets
dotnet publish -r win10-x64 --self-contained true -o QuixCliWrapper/binaries/win10
dotnet publish -r linux-x64 --self-contained true -o QuixCliWrapper/binaries/linux
dotnet publish -r osx-x64 --self-contained true -o QuixCliWrapper/binaries/osx
dotnet publish -r osx-arm64 --self-contained true -o QuixCliWrapper/binaries/osx-arm

rem build the package
python setup.py sdist
python setup.py bdist_wheel

copy C:\Code\Quix\cs-cli\dist\*.whl c:\code\tests