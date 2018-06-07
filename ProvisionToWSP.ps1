function StartProvision {
    param(
        [Parameter(Mandatory=$true)]
        [string]$targetCDN
    )

    Write-Host "TargetCDN '$targetCDN'"
    
    $SPFxRoot = Get-Location
    $webParts = Get-ChildItem -Directory
    foreach ($webPart in $webParts) {
        Set-Location -Path $SPFxRoot"/"$webPart
        Write-Host "SPFx Web Part '$SPFxRoot'"

        npm install
        gulp bundle --ship --copytowsp --target-cdn $targetCDN
        gulp copyassetstowsp
        gulp package-solution --ship
        gulp copypackagetowsp
    }
}

function StartProvisionExact {
    param(
        [Parameter(Mandatory=$true)]
        [string]$targetCDN,
        [Parameter(Mandatory=$true)]
        [string]$webpartName
    )

    Write-Host "TargetCDN '$targetCDN'"

    $SPFxRoot = Get-Location
    $webParts = Get-ChildItem -Directory
    $found = $false
    foreach ($webPart in $webParts) {
        if ($webPart.Name -eq $webpartName) {
            $found = $true

            Set-Location -Path $SPFxRoot"/"$webPart
            Write-Host "SPFx Web Part '$SPFxRoot'"

            npm install
            gulp bundle --ship --copytowsp --target-cdn $targetCDN
            gulp copyassetstowsp
            gulp package-solution --ship
            gulp copypackagetowsp
        }
    }

    if (!$found) {
        Write-Error "Web Part with name '$webpartName' not found"
    }
}