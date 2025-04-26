# Build and deploy an Angular application to Azure Static Web Apps

param (
    [Parameter(Mandatory = $true)]
    [string]$AzureStaticWebAppsApiToken
)

$resourceGroup = "docplanner-dev-rg"
$appName = "dralia-spa"

$angularProjectRoot = "$PSScriptRoot\webapp"
Write-Host "Building Angular project in $angularProjectRoot"

Push-Location $angularProjectRoot
npm install
ng build --configuration production
Pop-Location

$buildOutput = "$angularProjectRoot\dist\webapp"  
Write-Host "$buildOutput"

Write-Host "✅ Angular build completed."

# Deploy an Angular application to Azure Static Web Apps
$env:AZURE_STATIC_WEB_APPS_API_TOKEN = $AzureStaticWebAppsApiToken

# Make sure the deployment token is set
if (-not $env:AZURE_STATIC_WEB_APPS_API_TOKEN) {
  Write-Error "Environment variable 'AZURE_STATIC_WEB_APPS_API_TOKEN' is not set."
  exit 1
}

az staticwebapp update `
  --name $appName `
  --resource-group $resourceGroup `
  --source $buildOutputPath 

Write-Host "🚀 Angular SPA deployed to Azure Static Web App."
