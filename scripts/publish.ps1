$outputPath = ".nuget"

New-Item -ItemType Directory -Force $outputPath

dotnet pack -o $outputPath