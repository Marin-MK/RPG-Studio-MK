Write-Host "Building..."
dotnet publish --runtime win-x64 --self-contained true --configuration Release -p:PublishSingleFile=true -p:PublishTrimmed=true
Write-Host "Build complete."
Start-Sleep -Seconds 2