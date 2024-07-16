param (
    $connectionString,
    $extractedPath,
    $changeReportDirectory,
    $environmentName
)

$ExecutionLocation = "$extractedPath/Trident.Database.DbUp.exe"

if ([string]::IsNullOrWhiteSpace($changeReportDirectory) -eq $true)
{
    Write-Host "Doing a regular migration - executing $executionLocation"
    dotnet $ExecutionLocation --ConnectionString="$connectionString"
}
else
{
    Write-Host "Doing a what-if migration, generating a change report at $changeReportDirectory"

    if ((test-path $changeReportDirectory) -eq $false){
        Write-Host "The directory $changeReportDirectory does not exist, creating it now"
        New-Item $changeReportDirectory -ItemType "directory"
    }

    dotnet $ExecutionLocation --ConnectionString="$connectionString" --PreviewReportPath="$changeReportDirectory"

    $generatedReport = "$changeReportDirectory/UpgradeReport.html"
    New-OctopusArtifact -Path "$generatedReport" -Name "$environmentName.UpgradeReport.html"
}