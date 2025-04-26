# Build and deploy an Angular application to Azure Static Web Apps

param (
    [Parameter(Mandatory = $true)]
    [string]$AzureStaticWebAppsApiToken
)

$angularProjectRoot = "$PSScriptRoot\webapp"
Write-Host "Building Angular project in $angularProjectRoot"

Push-Location $angularProjectRoot
npm install
ng build --configuration production
Pop-Location

Write-Host "✅ Angular build completed."

# Deploy an Angular application to Azure Static Web Apps
$env:AZURE_STATIC_WEB_APPS_API_TOKEN = $AzureStaticWebAppsApiToken
$buildOutput = "$angularProjectRoot\dist\webapp"  

az staticwebapp update `
  --name dralia-spa `
  --resource-group docplanner-dev-rg `
  --source $buildOutput

Write-Host "🚀 Angular SPA deployed to Azure Static Web App."
