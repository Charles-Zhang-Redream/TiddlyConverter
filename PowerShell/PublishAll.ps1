$PrevPath = Get-Location

Write-Host "Publish for Final Packaging build."
Set-Location $PSScriptRoot

$PublishFolder = "$PSScriptRoot\..\Publish"

# Delete current data
Remove-Item $PublishFolder -Recurse -Force

# Publish Executables
$PublishExecutables = @(
    "TiddlyConverter\TiddlyConverter.csproj"
)
foreach ($Item in $PublishExecutables)
{
    dotnet publish $PSScriptRoot\..\$Item --use-current-runtime --output $PublishFolder
}

# Create archive
$Date = Get-Date -Format yyyyMMdd
$ArchiveFolder = "$PublishFolder\..\Packages"
$ArchivePath = "$ArchiveFolder\TiddlyConverter_DistributionBuild_Windows_B$Date.zip"
New-Item -ItemType Directory -Force -Path $ArchiveFolder
Compress-Archive -Path $PublishFolder\* -DestinationPath $ArchivePath -Force

# Validation
if (-Not (Test-Path (Join-Path $PublishFolder "TiddlyConverter.exe")))
{
    Write-Host "Build failed."
    Exit
}

Set-Location $PrevPath