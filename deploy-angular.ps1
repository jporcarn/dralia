# Build and deploy an Angular application to Azure Static Web Apps

param (
  [Parameter(Mandatory = $true)]
  [string]$AzureStaticWebAppsApiToken
)

$resourceGroup = "docplanner-dev-rg"
$appName = "dralia-spa"


$angularProjectRoot = "$PSScriptRoot\webapp"
Write-Host "Building Angular project in $angularProjectRoot"

$buildOutput = "$angularProjectRoot\dist\webapp"  
Write-Host "$buildOutput"

Push-Location $angularProjectRoot
npm install

npx swa init --yes
npx swa build
npx swa login --resource-group $resourceGroup --app-name $appName
Write-Host "✅ Angular build completed."

# Set the SWA_CLI_OUTPUT_LOCATION environment variable to the directory containing the index.html file:
$env:SWA_CLI_OUTPUT_LOCATION = "dist/webapp"

# Deploy an Angular application to Azure Static Web Apps
$env:AZURE_STATIC_WEB_APPS_API_TOKEN = $AzureStaticWebAppsApiToken

# Make sure the deployment token is set
if (-not $env:AZURE_STATIC_WEB_APPS_API_TOKEN) {
  Write-Error "Environment variable 'AZURE_STATIC_WEB_APPS_API_TOKEN' is not set."
  exit 1
}

# Verify the Azure Static Web App Exists
az staticwebapp show --name dralia-spa --resource-group docplanner-dev-rg

# Deploy using Static Web Apps CLI
npx swa deploy --app-name $appName --resource-group $resourceGroup --env production --output-location $buildOutput

# TODO: Review deployment error from PS local script 
# Checking project "dralia-spa" settings...
# ✖ The project "dralia-spa" is linked to "GitHub"!
# ✖ Unlink the project from the "GitHub" provider and try again.

# # Deploy using Azure CLI
# az staticwebapp update --name dralia-spa --resource-group docplanner-dev-rg --source dist/webapp 

# az staticwebapp update `
#   --name $appName `
#   --resource-group $resourceGroup `
#   --source dist/webapp 

Write-Host "🚀 Angular SPA deployed to Azure Static Web App."

Pop-Location








