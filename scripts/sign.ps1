if (-not $env:ZENVOICE_PFX -or -not $env:ZENVOICE_PFX_PASSWORD) {
    Write-Output 'ZENVOICE_PFX / ZENVOICE_PFX_PASSWORD not set; skip signing.'
    exit 0
}

$targets = if ($args.Count) { $args } else { @('dist\win-arm64\ZenVoice.exe') }
& signtool sign /fd SHA256 /td SHA256 /tr http://timestamp.digicert.com /f $env:ZENVOICE_PFX /p $env:ZENVOICE_PFX_PASSWORD @targets
exit $LASTEXITCODE
