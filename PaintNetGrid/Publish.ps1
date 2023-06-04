param([string]$libraryPublishPath)

$inputPath = "$libraryPublishPath*"
$outputPath = "C:\Program Files\paint.net\Effects\PaintNetGrid\"

if (!(Test-Path -path $outputPath)) {
	New-Item $outputPath -Type Directory
}

Write-Host "Copying $inputPath into $outputPath"

Copy-Item $inputPath -Destination $outputPath -Force

Write-Host "Done!"