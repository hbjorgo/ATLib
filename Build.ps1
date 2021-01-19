$artifacts = ".\artifacts"

if(Test-Path $artifacts) { Remove-Item $artifacts -Force -Recurse }

dotnet clean .\src\HeboTech.ATLib\HeboTech.ATLib.csproj -c Release
dotnet build .\src\HeboTech.ATLib\HeboTech.ATLib.csproj -c Release
dotnet pack .\src\HeboTech.ATLib\HeboTech.ATLib.csproj -c Release -o $artifacts --no-build
