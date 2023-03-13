If ((Get-Content Program.cs) | Select-String -Pattern "public static bool DebugMode = true;") {
	Write-Host "ERROR: Make sure you are building in Release mode first!"
}
Else {
	Write-Host "Building..."
	dotnet publish -r win-x64 --self-contained true -p:PublishTrimmed=true -p:PublishSingleFile=true
	Write-Host "Removing old version text..."
	rm "bin/Debug/net7.0/win-x64/publish/version.txt"
	Write-Host "Writing new version..."
	$dllPath = [IO.Path]::Combine($pwd, "bin\\Debug\\net7.0\\win-x64\\RPG Studio MK.dll")
	[System.Reflection.Assembly]::LoadFrom($dllPath).GetName().Version.ToString() >> "bin/Debug/net7.0/win-x64/publish/version.txt"
	Write-Host "Build complete."
}
Start-Sleep -Seconds 2