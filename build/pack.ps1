param(
  [string]$Configuration = "Release"
)

Write-Host "Restore..."
dotnet restore

Write-Host "Build..."
dotnet build -c $Configuration

Write-Host "Test..."
dotnet test -c $Configuration --no-build

Write-Host "Pack..."
dotnet pack .\\src\\LbxyCommonLib.ListCompression\\LbxyCommonLib.ListCompression.csproj -c $Configuration --no-build

Write-Host "Artifacts in artifacts\\packages"
