set sln=DotUtilities

dotnet new solution -n %sln%
mkdir %sln%
mkdir %sln%.Test
cd %sln%
dotnet new classlib
cd ..
cd %sln%.Test
dotnet new mstest
cd ..
dotnet sln add %sln%\%sln%.csproj
dotnet sln add %sln%.Test\%sln%.Test.csproj