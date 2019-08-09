set sln=DotUtilities

dotnet new solution -n %sln%
mkdir %sln%
mkdir %sln%.Test
cd %sln%
dotnet new classlib
cd ..
cd %sln%.Test
dotnet new mstest
dotnet add reference ..\%sln%\%sln%.csproj
cd ..
dotnet sln add **/*.csproj

rem modify the files to take into account the project name
rem add *.targets and modify the library .csproj
rem generate a new nuget key and put it in appveyor.xml