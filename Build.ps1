$artifacts = ".\artifacts"

Write-Host "### Cleaning artifacts folder... ###"
if(Test-Path $artifacts) { Remove-Item $artifacts -Force -Recurse }

Write-Host "### dotnet clean... ###"
dotnet clean .\src\HeboTech.ATLib\HeboTech.ATLib.csproj -c Release

Write-Host "### dotnet build... ###"
dotnet build .\src\HeboTech.ATLib\HeboTech.ATLib.csproj -c Release

Write-Host "### dotnet test... ###"
dotnet test .\src\HeboTech.ATLib.Tests\HeboTech.ATLib.Tests.csproj -c Release

Write-Host "### dotnet pack... ###"
dotnet pack .\src\HeboTech.ATLib\HeboTech.ATLib.csproj -c Release -o $artifacts --no-build
